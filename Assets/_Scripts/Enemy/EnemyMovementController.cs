using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class EnemyMovementController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private AStarPathfinder _pathfinder;

        [Header("Pathfinding")] 
        [SerializeField] private float _nextPathPointDistance = 0.2f;

        [Header("Chase")] 
        [SerializeField] private float _repathInterval = 0.5f;
        [SerializeField] private float _pathTargetDistance = 0.15f;
        [SerializeField] private float _targetRepathMoveDistance = 0.5f;

        [Header("Patrol")] 
        [SerializeField] private PatrolRoute[] _patrolRoutes;
        [SerializeField] private bool _switchRouteWhenFinished = true;
        [SerializeField] private float _waitAtWaypointTime = 1f;

        private EnemyController _enemyController;
        private PatrolRoute _currentRoute;
        private int _currentWaypointIndex;

        private readonly List<Vector3> _path = new();
        private int _pathIndex;
        private float _repathTimer;
        private float _waitTimer;
        private Vector3 _lastPathTargetPosition;
        private bool _isStopped;

        private void Awake()
        {
            _enemyController = GetComponent<EnemyController>();
        }

        private void Update()
        {
            if (_enemyController.IsDead)
            {
                return;
            }

            if (!_enemyController.CanMove)
            {
                StopMoving();
                return;
            }

            if (_enemyController.CurrentState == EnemyState.Chase && _enemyController.Target != null)
            {
                var targetPosition = _enemyController.Target.position;
                var position = transform.position;
                var attackRange = _enemyController.AttackRange;
                var distanceToTargetSqr = ((Vector2)(targetPosition - position)).sqrMagnitude;

                if (distanceToTargetSqr <= attackRange * attackRange)
                {
                    _enemyController.ChangeState(EnemyState.Attack);
                    return;
                }
                
                _repathTimer -= Time.deltaTime;

                if (_repathTimer <= 0f)
                {
                    var targetMoveDistanceSqr = (targetPosition - _lastPathTargetPosition).sqrMagnitude;
                    var repathMoveDistanceSqr = _targetRepathMoveDistance * _targetRepathMoveDistance;

                    if (_path.Count == 0 || targetMoveDistanceSqr >= repathMoveDistanceSqr)
                    {
                        RefreshPathToTarget();
                        _repathTimer = _repathInterval;
                    }
                    
                    _repathTimer = _repathInterval;
                }
            }
            else if (_enemyController.CurrentState == EnemyState.Patrol)
            {
                if (_currentRoute == null)
                {
                    return;
                }

                if (_path.Count > 0 && _pathIndex < _path.Count)
                {
                    return;
                }

                _waitTimer -= Time.deltaTime;

                if (_waitTimer > 0f)
                {
                    StopMoving();
                    return;
                }

                GoToNextWaypoint();
            }
        }

        private void FixedUpdate()
        {
            if (_enemyController.IsDead)
            {
                return;
            }
            
            if (!_enemyController.CanMove)
            {
                StopMoving();
                return;
            }

            switch (_enemyController.CurrentState)
            {
                case EnemyState.Idle:
                    StopMoving();
                    break;

                case EnemyState.Patrol:
                    FollowPath(_enemyController.WalkSpeed);
                    break;

                case EnemyState.Chase:
                    FollowPath(_enemyController.RunSpeed);
                    break;
            }
        }

        public void StartPatrol()
        {
            if (_patrolRoutes == null || _patrolRoutes.Length == 0)
            {
                _enemyController.ChangeState(EnemyState.Idle);
                return;
            }

            _currentRoute = _patrolRoutes[Random.Range(0, _patrolRoutes.Length)];
            _currentWaypointIndex = 0;
            SetPathToCurrentWaypoint();

            _enemyController.ChangeState(EnemyState.Patrol);
            _enemyController.DetectionController.ResetDetectionRange();
        }

        private void FollowPath(float speed)
        {
            if (_path.Count == 0 || _pathIndex >= _path.Count)
            {
                _path.Clear();
                StopMoving();
                return;
            }

            Vector2 targetPoint = _path[_pathIndex];
            var currentPosition = _enemyController.Rigidbody.position;
            var direction = targetPoint - currentPosition;

            if (direction.sqrMagnitude <= _nextPathPointDistance * _nextPathPointDistance)
            {
                _pathIndex++;

                if (_pathIndex >= _path.Count)
                {
                    StopMoving();
                    return;
                }

                targetPoint = _path[_pathIndex];
                direction = targetPoint - currentPosition;
            }

            direction.Normalize();

            _isStopped = false;
            _enemyController.Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _enemyController.Rigidbody.linearVelocity = direction * speed;
            _enemyController.UpdateAnimation(direction, speed);
        }

        private void GoToNextWaypoint()
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _currentRoute.Count)
            {
                if (_currentRoute.Loop)
                {
                    _currentWaypointIndex = 0;
                }
                else if (_switchRouteWhenFinished)
                {
                    _currentRoute = _patrolRoutes[Random.Range(0, _patrolRoutes.Length)];
                    _currentWaypointIndex = 0;
                }
                else
                {
                    _enemyController.ChangeState(EnemyState.Idle);
                    return;
                }
            }

            SetPathToCurrentWaypoint();
            _waitTimer = _waitAtWaypointTime;
        }

        private void SetPathToCurrentWaypoint()
        {
            if (!_currentRoute || _currentRoute.Count == 0)
            {
                return;
            }

            var waypoint = _currentRoute.GetWaypoint(_currentWaypointIndex);

            if (!waypoint)
            {
                return;
            }

            var pathFound = _pathfinder.FindPath(transform.position, waypoint.position, _path);

            if (!pathFound || _path.Count == 0)
            {
                return;
            }

            _pathIndex = _path.Count > 1 ? 1 : 0;
        }

        private void RefreshPathToTarget()
        {
            if (!_enemyController.Target)
            {
                return;
            }

            _lastPathTargetPosition = _enemyController.Target.position;
            var pathFound = _pathfinder.FindPathNearTarget(transform.position, _enemyController.Target.position, _pathTargetDistance, _path);

            if (!pathFound || _path.Count == 0)
            {
                return;
            }

            _pathIndex = _path.Count > 1 ? 1 : 0;
        }

        public void StopMoving()
        {
            if (_isStopped)
            {
                return;
            }

            _isStopped = true;
            _enemyController.Rigidbody.linearVelocity = Vector2.zero;
            _enemyController.Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            _enemyController.Animator.SetFloat(Utils.SPEED_HASH, 0f);
        }

        public void FoundTarget()
        {
            _repathTimer = Random.Range(0f, _repathInterval);//stagger repathing
        }

        public void LostTarget()
        {
            _path.Clear();
            StartPatrol();
        }
    }
}
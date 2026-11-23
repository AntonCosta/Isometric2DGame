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
        [SerializeField] private float _repathInterval = 0.3f;
        [SerializeField] private float _pathTargetDistance = 0.15f;

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
                _repathTimer -= Time.deltaTime;

                if (_repathTimer <= 0f)
                {
                    RefreshPathToTarget();
                    _repathTimer = _repathInterval;
                }

                var distanceToTarget = Vector2.Distance(transform.position, _enemyController.Target.position);

                if (distanceToTarget <= _enemyController.AttackRange)
                {
                    _enemyController.ChangeState(EnemyState.Attack);
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

            _enemyController.Rigidbody.linearVelocity = direction * speed;
            _enemyController.UpdateAnimation(direction, speed);
        }

        private void GoToNextWaypoint()
        {
            _waitTimer = _waitAtWaypointTime;
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

            var newPath = _pathfinder.FindPath(transform.position, waypoint.position);

            if (newPath == null || newPath.Count == 0)
            {
                return;
            }

            _path.Clear();
            _path.AddRange(newPath);
            _pathIndex = _path.Count > 1 ? 1 : 0;
        }

        private void RefreshPathToTarget()
        {
            if (!_enemyController.Target)
            {
                return;
            }

            var newPath = _pathfinder.FindPathNearTarget(
                transform.position,
                _enemyController.Target.position,
                _pathTargetDistance
            );

            if (newPath == null || newPath.Count == 0)
            {
                return;
            }

            _path.Clear();
            _path.AddRange(newPath);
            _pathIndex = _path.Count > 1 ? 1 : 0;
        }

        public void StopMoving()
        {
            _enemyController.Rigidbody.linearVelocity = Vector2.zero;
            _enemyController.Animator.SetFloat(Utils.SPEED_HASH, 0f);
        }

        public void FoundTarget()
        {
            RefreshPathToTarget();
            _repathTimer = _repathInterval;
        }

        public void LostTarget()
        {
            _path.Clear();
            StartPatrol();
        }
    }
}
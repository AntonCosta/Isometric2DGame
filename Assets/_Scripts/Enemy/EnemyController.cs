using System;
using System.Collections.Generic;
using Pathfinding;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AStarPathfinder _pathfinder;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        [Header("Stats")]
        [SerializeField] private float _runSpeed = 1.5f;
        [SerializeField] private float _walkSpeed = 0.5f;
        [SerializeField] private float _nextPathPointDistance = 0.2f;
        [SerializeField] private float _detectionRange = 1f;
        [SerializeField] private CircleCollider2D _detectionCollider;
        [SerializeField] private float _damage = 10;
        [SerializeField] private float _attackSpeed = 1f;
        [SerializeField] private float _attackDistance = 0.3f;
        
        [Header("Chase")]
        [SerializeField] private float _repathInterval = 0.3f;
        [SerializeField] private float _pathTargetDistance = 0.45f;
        
        [Header("Patrol")]
        [SerializeField] private PatrolRoute[] _patrolRoutes;
        [SerializeField] private bool _switchRouteWhenFinished = true;
        [SerializeField] private float _waitAtWaypointTime = 1f;

        private EnemyState _currentState = EnemyState.Idle;
        private Rigidbody2D _rb;
        private Transform _target;
        private PlayerController _playerController;
        private Vector2 _lastMoveDirection = Vector2.down;
        
        private readonly List<Vector3> _path = new();
        private int _pathIndex;
        private float _repathTimer;
        
        private PatrolRoute _currentRoute;
        private int _currentWaypointIndex;
        private float _waitTimer;
        private float _attackTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            StartPatrol();
            _detectionCollider.radius = _detectionRange;
            _attackTimer = 1f / _attackSpeed;
        }

        private void Update()
        {
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
            
            if (_currentState == EnemyState.Chase && _target != null)
            {
                _repathTimer -= Time.deltaTime;

                if (_repathTimer <= 0f)
                {
                    RefreshPathToTarget();
                    _repathTimer = _repathInterval;
                }

                var distanceToTarget = Vector2.Distance(transform.position, _target.position);

                if (distanceToTarget <= _attackDistance)
                {
                    _animator.SetBool(Utils.IS_ATTACKING_HASH, true);
                    ChangeState(EnemyState.Attack);
                }
            }
            else if (_currentState == EnemyState.Attack && _target != null)
            {
                var distanceToTarget = Vector2.Distance(transform.position, _target.position);

                if (distanceToTarget > _attackDistance)
                {
                    ChangeState(EnemyState.Chase);
                    _animator.SetBool(Utils.IS_ATTACKING_HASH, false);
                }
            }
            else if (_currentState == EnemyState.Patrol)
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
            switch (_currentState)
            {
                case EnemyState.Idle:
                    StopMoving();
                    break;

                case EnemyState.Patrol:
                    FollowPath(_walkSpeed, _walkSpeed);
                    break;

                case EnemyState.Chase:
                    FollowPath(_runSpeed, _runSpeed);
                    break;

                case EnemyState.Attack:
                    StopMoving();
                    Attack();
                    break;
            }
        }

        private void Attack()
        {
            if (!_playerController)
            {
                ChangeState(EnemyState.Chase);
                _animator.SetBool(Utils.IS_ATTACKING_HASH, false);
                return;
            }

            _attackTimer -= Time.fixedDeltaTime;

            if (_attackTimer > 0f)
            {
                return;
            }
            
            _playerController.TakeDamage(_damage);
            _attackTimer = 1f / _attackSpeed;
        }
        
        private void RefreshPathToTarget()
        {
            if (!_target)
            {
                return;
            }

            var newPath = _pathfinder.FindPathNearTarget(
                transform.position,
                _target.position,
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
        
        private void FollowPath(float moveSpeed, float animationSpeed)
        {
            if (_path.Count == 0 || _pathIndex >= _path.Count)
            {
                if (_currentState == EnemyState.Chase)
                {
                    RefreshPathToTarget();
                }
                else
                {
                    _path.Clear();
                }

                StopMoving();
                return;
            }

            Vector2 targetPoint = _path[_pathIndex];
            var currentPosition = _rb.position;
            var direction = targetPoint - currentPosition;

            if (direction.magnitude <= _nextPathPointDistance)
            {
                _pathIndex++;

                if (_pathIndex >= _path.Count)
                {
                    if (_currentState == EnemyState.Chase)
                    {
                        RefreshPathToTarget();
                    }

                    StopMoving();
                    return;
                }

                targetPoint = _path[_pathIndex];
                direction = targetPoint - currentPosition;
            }

            direction.Normalize();

            _rb.linearVelocity = direction * moveSpeed;
            UpdateAnimation(direction, animationSpeed);
        }

        private void StopMoving()
        {
            _rb.linearVelocity = Vector2.zero;
            _animator.SetFloat(Utils.SPEED_HASH, 0f);
        }

        protected void ChangeState(EnemyState newState)
        {
            _currentState = newState;
        }
        
        private void StartPatrol()
        {
            if (_patrolRoutes == null || _patrolRoutes.Length == 0)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            _currentRoute = _patrolRoutes[Random.Range(0, _patrolRoutes.Length)];
            _currentWaypointIndex = 0;
            SetPathToCurrentWaypoint();

            ChangeState(EnemyState.Patrol);
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
                    ChangeState(EnemyState.Idle);
                    return;
                }
            }

            SetPathToCurrentWaypoint();
        }
        
        private void SetPathToCurrentWaypoint()
        {
            if (_currentRoute == null || _currentRoute.Count == 0)
            {
                return;
            }

            var waypoint = _currentRoute.GetWaypoint(_currentWaypointIndex);

            if (waypoint == null)
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
        
        private void UpdateAnimation(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = ConvertToEightDirection(direction);

                _animator.SetFloat(Utils.MOVE_X_HASH, _lastMoveDirection.x);
                _animator.SetFloat(Utils.MOVE_Y_HASH, _lastMoveDirection.y);
            }

            _animator.SetFloat(Utils.SPEED_HASH, speed);
        }

        private Vector2 ConvertToEightDirection(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            if (angle >= 337.5f || angle < 22.5f)
            {
                return new Vector2(1, 0);
            }

            if (angle < 67.5f)
            {
                return new Vector2(1, 1);
            }

            if (angle < 112.5f)
            {
                return new Vector2(0, 1);
            }

            if (angle < 157.5f)
            {
                return new Vector2(-1, 1);
            }

            if (angle < 202.5f)
            {
                return new Vector2(-1, 0);
            }

            if (angle < 247.5f)
            {
                return new Vector2(-1, -1);
            }

            if (angle < 292.5f)
            {
                return new Vector2(0, -1);
            }

            if (angle >= 292.5f || angle < 337.5f)
            {
                return new Vector2(1, -1);
            }

            return new Vector2(0, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _target = other.transform;
            _playerController = other.GetComponent<PlayerController>();
            _currentState = EnemyState.Chase;
            
            RefreshPathToTarget();
            _repathTimer = _repathInterval;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _target = null;
            _playerController = null;
            _path.Clear();
            _rb.linearVelocity = Vector2.zero;

            StartPatrol();
        }
    }
}
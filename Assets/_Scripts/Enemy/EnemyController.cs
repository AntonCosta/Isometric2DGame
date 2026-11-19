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
        public EnemyState CurrentState => _currentState;
        public Rigidbody2D Rigidbody => _rb;
        public Animator Animator => _animator;
        public float AttackRange => _attackRange;
        public Transform Target => _target;
        public float RunSpeed => _runSpeed;
        public float WalkSpeed => _walkSpeed;
        
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        [Header("Stats")]
        [SerializeField] private float _runSpeed = 1.5f;
        [SerializeField] private float _walkSpeed = 0.5f;
        [SerializeField] private float _detectionRange = 1f;
        [SerializeField] private CircleCollider2D _detectionCollider;
        [SerializeField] private float _damage = 10;
        [SerializeField] private float _attackSpeed = 1f;
        [SerializeField] private float _attackRange = 0.3f;

        private EnemyState _currentState = EnemyState.Idle;
        private Rigidbody2D _rb;
        private EnemyMovementController _enemyMovementController;
        private Transform _target;
        private PlayerController _playerController;
        private Vector2 _lastMoveDirection = Vector2.down;
        
        private float _attackTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _enemyMovementController = GetComponent<EnemyMovementController>();
        }

        private void Start()
        {
            _enemyMovementController.StartPatrol();
            _detectionCollider.radius = _detectionRange;
            _attackTimer = 1f / _attackSpeed;
        }

        private void Update()
        {
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
            
            if (_currentState == EnemyState.Attack && _target != null)
            {
                var distanceToTarget = Vector2.Distance(transform.position, _target.position);

                if (distanceToTarget > _attackRange)
                {
                    ChangeState(EnemyState.Chase);
                    _animator.SetBool(Utils.IS_ATTACKING_HASH, false);
                }
            }
        }

        private void FixedUpdate()
        {
            switch (_currentState)
            {
                case EnemyState.Attack:
                    _enemyMovementController.StopMoving();
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

        public void ChangeState(EnemyState newState)
        {
            _currentState = newState;
        }
        
        public void UpdateAnimation(Vector2 direction, float speed)
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
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _target = null;
            _playerController = null;
            _rb.linearVelocity = Vector2.zero;
        }
    }
}
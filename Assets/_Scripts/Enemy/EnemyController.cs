using System;
using System.Collections.Generic;
using Combat;
using DG.Tweening;
using Pathfinding;
using Player;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class EnemyController : AnimationEventsListener, IDamageable
    {
        private const float DAMAGE_FLASH_DURATION = 0.3f;

        public EnemyState CurrentState => _currentState;
        public Rigidbody2D Rigidbody => _rb;
        public Animator Animator => _animator;
        public float AttackRange => _attackRange;
        public Transform Target => _target;
        public float RunSpeed => _runSpeed;
        public float WalkSpeed => _walkSpeed;
        public bool IsAttackLocked => _isAttacking;
        public bool CanMove => _canMove;
        public bool IsDead => _isDead;
        public bool LostTargetDuringAttack { get; set; }

        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _attackHitbox;
        [SerializeField] private Collider2D _attackHitboxCollider;
        [SerializeField] private UIHealthController _uiHealthController;
        
        [Header("Stats")]
        [SerializeField] private float _maxHealth = 30f;
        [SerializeField] private float _health = 30f;
        [SerializeField] private float _runSpeed = 1.5f;
        [SerializeField] private float _walkSpeed = 0.5f;
        [SerializeField] private float _attackRange = 0.3f;

        private EnemyState _currentState = EnemyState.Idle;
        private Rigidbody2D _rb;
        private EnemyMovementController _enemyMovementController;
        private Transform _target;
        private PlayerController _playerController;
        private Vector2 _lastFacingDirection = Vector2.down;
        private Tween _damageFlashTween;
        private bool _isAttacking;
        private bool _canMove;
        private bool _isDead;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _enemyMovementController = GetComponent<EnemyMovementController>();
        }

        private void Start()
        {
            _enemyMovementController.StartPatrol();
            _attackHitbox.gameObject.SetActive(false);
            _canMove = true;
        }

        private void Update()
        {
            if (_isDead)
            {
                return;
            }

            //This is a rule used by all the sprites in the project to add depth and help with sorting
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }

        private void FixedUpdate()
        {
            if (_isDead)
            {
                return;
            }

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
                _isAttacking = false;
                _canMove = true;
                ChangeState(EnemyState.Chase);
                _animator.SetBool(Utils.IS_ATTACKING_HASH, _isAttacking);
                return;
            }
            
            if (_isAttacking)
            {
                return;
            }

            if (_playerController.IsDead)
            {
                _isAttacking = false;
                _canMove = true;
                _enemyMovementController.LostTarget();
                _animator.SetBool(Utils.IS_ATTACKING_HASH, _isAttacking);
                return;
            }
            
            _isAttacking = true;
            _canMove = false;
            _animator.SetBool(Utils.IS_ATTACKING_HASH, _isAttacking);
            _animator.Play(Utils.ATTACK_STATE_NAME, 0, 0f);
        }
        
        public override void EnableAttackHitbox()
        {
            PositionAttackHitbox();
            _attackHitbox.gameObject.SetActive(true);
        }
        
        public override void DisableAttackHitbox()
        {
            _attackHitbox.gameObject.SetActive(false);
        }
        
        private void PositionAttackHitbox()
        {
            if (!_attackHitbox)
            {
                return;
            }

            var attackDirection = _lastFacingDirection;

            if (attackDirection.sqrMagnitude < 0.01f)
            {
                attackDirection = Vector2.down;
            }

            attackDirection.Normalize();

            var angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            _attackHitbox.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        public override void FinishAttack()
        {
            _isAttacking = false;
            _canMove = true;
            _animator.SetBool(Utils.IS_ATTACKING_HASH, _isAttacking);
            
            _enemyMovementController.StopMoving();
            
            if (!_target || !_playerController || LostTargetDuringAttack || _playerController && _playerController.IsDead)
            {
                ClearTarget();
                _enemyMovementController.LostTarget();
                return;
            }

            var distanceToTarget = Vector2.Distance(transform.position, _target.position);

            if (distanceToTarget > _attackRange)
            {
                ChangeState(EnemyState.Chase);
                _animator.SetBool(Utils.IS_ATTACKING_HASH, false);
            }
        }

        public void ChangeState(EnemyState newState)
        {
            if (IsAttackLocked && newState != EnemyState.Attack)
            {
                return;
            }

            _currentState = newState;
        }
        
        public void UpdateAnimation(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                _lastFacingDirection = ConvertToEightDirection(direction);

                _animator.SetFloat(Utils.MOVE_X_HASH, _lastFacingDirection.x);
                _animator.SetFloat(Utils.MOVE_Y_HASH, _lastFacingDirection.y);
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

        public void SetTarget(PlayerController newTarget)
        {
            _target = newTarget.transform;
            _playerController = newTarget;
        }

        public void ClearTarget()
        {
            _target = null;
            _playerController = null;
            LostTargetDuringAttack = false;
        }

        public void TakeDamage(float amount)
        {
            _health -= amount;

            if (_health <= 0)
            {
                Die();
                return;
            }
            
            _damageFlashTween?.Kill();
            _spriteRenderer.color = Color.white;

            _damageFlashTween = _spriteRenderer
                .DOColor(Color.red, DAMAGE_FLASH_DURATION)
                .SetLoops(2, LoopType.Yoyo);
            _uiHealthController.UpdateHealth(_health, _maxHealth);
            
            _canMove = false;
            _animator.SetBool(Utils.TAKE_DAMAGE_HASH, true);
        }
        
        public override void EndHurt()
        {
            _canMove = true;
            _animator.SetBool(Utils.TAKE_DAMAGE_HASH, false);
        }

        public void Die()
        {
            _isDead = true;
            _animator.SetTrigger(Utils.DIE_HASH);
            _uiHealthController.gameObject.SetActive(false);
            gameObject.tag = "Dead";
            GetComponent<CapsuleCollider2D>().enabled = false;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
}
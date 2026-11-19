using System;
using Combat;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : AnimationEventsListener, IDamageable
    {
        private const float DAMAGE_FLASH_DURATION = 0.3f;
        
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _attackHitbox;
        [SerializeField] private Collider2D _attackHitboxCollider;

        [Header("Stats")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _acceleration = 0f;

        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private Vector2 _moveValue;
        private float _attackValue;
        private Vector2 _lastFacingDirection = Vector2.down;
        private Tween _damageFlashTween;
        private bool _isAttacking;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

       private void Start()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _attackAction = InputSystem.actions.FindAction("Attack");
            _attackHitbox.gameObject.SetActive(false);
        }

        private void Update()
        {
            var wantsToAttack = _attackAction.IsPressed();

            if (!_isAttacking)
            {
                _moveValue = _moveAction.ReadValue<Vector2>();
                
                if (_moveValue.sqrMagnitude > 0.01f)
                {
                    _lastFacingDirection = new Vector2(
                        _moveValue.x == 0 ? 0 : Mathf.Sign(_moveValue.x),
                        _moveValue.y == 0 ? 0 : Mathf.Sign(_moveValue.y)
                    );
                }

                if (wantsToAttack)
                {
                    StartAttack();
                }
            }
            
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_isAttacking)
            {
                _rb.linearVelocity = Vector2.zero;
                _animator.SetFloat(Utils.SPEED_HASH, 0f);
                return;
            }
            
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, _moveValue.normalized * _moveSpeed, _acceleration != 0 ? _acceleration * Time.fixedDeltaTime : 1);
            
            var speed = _moveValue.sqrMagnitude;
            if (speed > 0.01f)
            {
                _animator.SetFloat(Utils.MOVE_X_HASH, _lastFacingDirection.x);
                _animator.SetFloat(Utils.MOVE_Y_HASH, _lastFacingDirection.y);
            }
            _animator.SetFloat(Utils.SPEED_HASH, speed);
            PositionAttackHitbox();
        }

        private void StartAttack()
        {
            if (_isAttacking)
            {
                return;
            }

            _isAttacking = true;
            _moveValue = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            
            _animator.SetBool(Utils.IS_ATTACKING_HASH, _isAttacking);
            _animator.SetFloat(Utils.SPEED_HASH, 0f);
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
            _animator.SetBool(Utils.IS_ATTACKING_HASH, _isAttacking);
        }

        public void TakeDamage(float amount)
        {
            _health -= amount;
            _damageFlashTween?.Kill();
            _spriteRenderer.color = Color.white;

            _damageFlashTween = _spriteRenderer
                .DOColor(Color.red, DAMAGE_FLASH_DURATION)
                .SetLoops(2, LoopType.Yoyo);
            
        }
    }
}
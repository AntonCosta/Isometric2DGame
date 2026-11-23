using System;
using Combat;
using DG.Tweening;
using Dialogue;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player
{
    public class PlayerController : AnimationEventsListener, IDamageable
    {
        private const float DAMAGE_FLASH_DURATION = 0.3f;
        private const float HEAL_VALUE = 25f;

        public Action OnPlayerDeath;
        public float AutoAttackSpeed => _autoAttackSpeed;
        public bool CanAutoAttack => _canAutoAttack;
        public bool IsDead => _isDead;
        
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _attackHitbox;
        [SerializeField] private Collider2D _attackHitboxCollider;
        [SerializeField] private UIHealthController _uiHealthController;
        [SerializeField] private Image _autoAttackUI;
        [SerializeField] private Image _healUI;
        [SerializeField] private VFXController _healFX;
        [SerializeField] private DialogueManager _dialogueManager;

        [Header("Stats")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _acceleration = 0f;
        [SerializeField] private float _autoAttackSpeed = 1f;
        [SerializeField] private float _healUses = 1f;

        [Header("Weapons")]
        [SerializeField] private ProjectilePool _projectilePool;

        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _autoAttackAction;
        private InputAction _healAction;
        private Vector2 _moveValue;
        private float _attackValue;
        private Vector2 _lastFacingDirection = Vector2.down;
        private Tween _damageFlashTween;
        private bool _isAttacking;
        private bool _canAutoAttack = false;
        private float _healedCounter;
        private bool _isDead;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

       private void Start()
        {
            _moveAction = InputSystem.actions.FindAction(Utils.MOVE_ACTION);
            _attackAction = InputSystem.actions.FindAction(Utils.ATTACK_ACTION);
            _autoAttackAction = InputSystem.actions.FindAction(Utils.AUTO_ATTACK_ACTION);
            _healAction = InputSystem.actions.FindAction(Utils.HEAL_ACTION);
            _attackHitbox.gameObject.SetActive(false);
            _autoAttackUI.color = Color.gray;
        }

        private void Update()
        {
            if (_isDead)
            {
                return;
            }

            if (_dialogueManager.DialogueOpen)
            {
                return;
            }
            
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

            if (_autoAttackAction.WasPressedThisFrame())
            {
                _canAutoAttack = !_canAutoAttack;
                _autoAttackUI.color = _canAutoAttack ? Color.white : Color.gray;
            }

            if (_healAction.WasPressedThisFrame() && _healUses > _healedCounter && _health < _maxHealth)
            {
                _healedCounter++;
                Heal();
            }
            
            //This is a rule used by all the sprites in the project to add depth and help with sorting
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }

        void FixedUpdate()
        {
            if (_dialogueManager.DialogueOpen)
            {
                return;
            }
            
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

        public void Fire(Transform target)
        {
            if (!target)
            {
                return;
            }
            
            var direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            _projectilePool.SpawnProjectile(transform.position, direction);
        }

        private void Heal()
        {
            _healFX.Play();
            _health = Mathf.Clamp(_health + HEAL_VALUE, 0, _maxHealth);
            _uiHealthController.UpdateHealth(_health, _maxHealth);
            _healUI.fillAmount = 1f - _healedCounter / _healUses;
        }

        public void TakeDamage(float amount, bool stun = true)
        {
            _health -= amount;
            
            if (_health <= 0)
            {
                Die();
            }
            
            _damageFlashTween?.Kill();
            _spriteRenderer.color = Color.white;

            _damageFlashTween = _spriteRenderer
                .DOColor(Color.red, DAMAGE_FLASH_DURATION)
                .SetLoops(2, LoopType.Yoyo);
            
            _uiHealthController.UpdateHealth(_health, _maxHealth);
            _animator.SetBool(Utils.TAKE_DAMAGE_HASH, true);
        }

        public void Die()
        {
            _isDead = true;
            _animator.SetBool(Utils.DIE_HASH, true);
            _uiHealthController.gameObject.SetActive(false);
            gameObject.tag = "Dead";
            GetComponent<CapsuleCollider2D>().enabled = false;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            OnPlayerDeath?.Invoke();
            _dialogueManager.EndDialogue();
        }

        public override void EndHurt()
        {
            _animator.SetBool(Utils.TAKE_DAMAGE_HASH, false);
        }
    }
}
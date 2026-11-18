using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private static float DAMAGE_FLASH_DURATION = 0.3f;
        
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _acceleration = 0f;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private Vector2 _moveValue;
        private Vector2 _lastFacingDirection = Vector2.down;
        private Tween _damageFlashTween;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

       private void Start()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
        }

        private void Update()
        {
            _moveValue = _moveAction.ReadValue<Vector2>();

            if (_moveValue.sqrMagnitude > 0.01f)
            {
                _lastFacingDirection = new Vector2(
                    _moveValue.x == 0 ? 0 : Mathf.Sign(_moveValue.x),
                    _moveValue.y == 0 ? 0 : Mathf.Sign(_moveValue.y)
                );
            }
            
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, _moveValue.normalized * _moveSpeed, _acceleration != 0 ? _acceleration * Time.fixedDeltaTime : 1);
            
            var speed = _moveValue.sqrMagnitude;
            if (speed > 0.01f)
            {
                _animator.SetFloat(Utils.MOVE_X_HASH, _lastFacingDirection.x);
                _animator.SetFloat(Utils.MOVE_Y_HASH, _lastFacingDirection.y);
            }
            _animator.SetFloat(Utils.SPEED_HASH, speed);
        }

        public void TakeDamage(float ammount)
        {
            _health -= ammount;
            _damageFlashTween?.Kill();
            _spriteRenderer.color = Color.white;

            _damageFlashTween = _spriteRenderer
                .DOColor(Color.red, DAMAGE_FLASH_DURATION)
                .SetLoops(2, LoopType.Yoyo);
            
        }
    }
}
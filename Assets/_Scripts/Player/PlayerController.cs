using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private const string MOVE_X = "MoveX";
        private const string MOVE_Y = "MoveY";
        private const string SPEED = "Speed";
        private static readonly int MOVE_X_HASH = Animator.StringToHash(MOVE_X);
        private static readonly int MOVE_Y_HASH = Animator.StringToHash(MOVE_Y);
        private static readonly int SPEED_HASH = Animator.StringToHash(SPEED);
        
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _acceleration = 0f;
        [SerializeField]  private Animator _animator;
        [SerializeField]  private SpriteRenderer _spriteRenderer;

        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private Vector2 _moveValue;
        private Vector2 _lastFacingDirection = Vector2.down;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
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
                _animator.SetFloat(MOVE_X_HASH, _lastFacingDirection.x);
                _animator.SetFloat(MOVE_Y_HASH, _lastFacingDirection.y);
            }
            _animator.SetFloat(SPEED_HASH, speed);
        }
    }
}
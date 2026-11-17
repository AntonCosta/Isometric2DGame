using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private const string IS_RUNNING = "IsRunning";
        private static readonly int RUNNING = Animator.StringToHash(IS_RUNNING);
        
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _acceleration = 0f;
        [SerializeField]  private Animator _animator;
        [SerializeField]  private SpriteRenderer _spriteRenderer;

        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private Vector2 _moveValue;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _moveAction = InputSystem.actions.FindAction("Move");
        }

        private void Update()
        {
            _moveValue = _moveAction.ReadValue<Vector2>();

            if (_moveValue.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else if (_moveValue.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, _moveValue.normalized * _moveSpeed, _acceleration != 0 ? _acceleration * Time.fixedDeltaTime : 1);
            _animator.SetBool(RUNNING, _moveValue != Vector2.zero);
        }
    }
}
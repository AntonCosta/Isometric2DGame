using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {

        [SerializeField] private float _moveSpeed = 5f;

        private Rigidbody2D rb;
        private InputAction _moveAction;
        private Vector2 _moveValue;
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            _moveAction = InputSystem.actions.FindAction("Move");
        }

        private void Update()
        {
            _moveValue = _moveAction.ReadValue<Vector2>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            rb.linearVelocity = _moveValue.normalized * _moveSpeed;
        }
    }
}
using System;
using UnityEngine;

namespace Combat
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private TargetTag _targetTag = TargetTag.Enemy;
        [SerializeField] private float _speed = 2f;
        [SerializeField] private float _damage = 5f;
        [SerializeField] private float _range = 20f;

        private Rigidbody2D _rb;
        private Vector2 _startPosition;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Vector2.Distance(_startPosition, transform.position) >= _range)
            {
                Deactivate();
            }
        }

        public void Fire(Vector2 direction)
        {
            gameObject.SetActive(true);
            _startPosition = transform.position;
            
            direction.Normalize();

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            
            _rb.linearVelocity = direction * _speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(_targetTag.ToString()))
            {
                return;
            }

            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage, false);
            }

            Deactivate();
        }

        private void Deactivate()
        {
            _rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
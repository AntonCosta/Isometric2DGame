using System;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyDetectionController : MonoBehaviour
    {
        [SerializeField] private EnemyController _enemyController;
        [SerializeField] private EnemyMovementController _enemyMovementController;
        [SerializeField] private CircleCollider2D _detectionCollider;
        [SerializeField] private float _detectionRange = 1f;
        [SerializeField] private float _maximumDetectionRange = 3f;

        private float _initialDetectionRange;

        private void Awake()
        {
            _detectionCollider.radius = _detectionRange;
            _initialDetectionRange = _detectionRange;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_enemyController.IsDead)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            var playerController = other.GetComponent<PlayerController>();

            if (!playerController || playerController.IsDead)
            {
                return;
            }
            
            _enemyController.SetTarget(playerController);
            _enemyController.ChangeState(EnemyState.Chase);

            _enemyMovementController.FoundTarget();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_enemyController.IsDead)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (_enemyController.IsAttackLocked)
            {
                _enemyController.LostTargetDuringAttack = true;
                return;
            }

            _enemyController.ClearTarget();
            _enemyController.Rigidbody.linearVelocity = Vector2.zero;

            _enemyMovementController.LostTarget();
        }
        
        public void IncreaseDetectionRange(float amount)
        {
            _detectionRange = Mathf.Clamp(_detectionRange + amount, 0, _maximumDetectionRange);
            _detectionCollider.radius = _detectionRange;
        }
        
        public void DecreaseDetectionRange(float amount)
        {
            _detectionRange = Mathf.Clamp(_detectionRange - amount, 0, _maximumDetectionRange);
            _detectionCollider.radius = _detectionRange;
        }

        public void ResetDetectionRange()
        {
            _detectionRange = _initialDetectionRange;
            _detectionCollider.radius = _detectionRange;
        }

    }
}
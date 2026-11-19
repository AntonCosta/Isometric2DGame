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

        private void Start()
        {
            _detectionCollider.radius = _detectionRange;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            var playerController = other.GetComponent<PlayerController>();
            _enemyController.SetTarget(playerController);
            _enemyController.ChangeState(EnemyState.Chase);


            _enemyMovementController.FoundTarget();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
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
    }
}
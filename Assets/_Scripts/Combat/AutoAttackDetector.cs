using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Combat
{
    public class AutoAttackDetector : MonoBehaviour
    {
        [SerializeField] private TargetTag _targetTag = TargetTag.Enemy;
        [SerializeField] private PlayerController _player;
        
        private readonly List<Transform> _targetsInRange = new ();
        private float _attackCooldownTimer;

        private void Update()
        {
            if (!_player.CanAutoAttack)
            {
                return;
            }
            
            RemoveInvalidTargets();

            if (_attackCooldownTimer > 0f)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            if (_targetsInRange.Count == 0)
            {
                return;
            }

            if (_attackCooldownTimer > 0f)
            {
                return;
            }

            var target = GetClosestTarget();

            if (!target)
            {
                return;
            }

            _player.Fire(target);
            _attackCooldownTimer = _player.AutoAttackSpeed;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(_targetTag.ToString()))
            {
                return;
            }

            if (_targetsInRange.Contains(other.transform))
            {
                return;
            }

            _targetsInRange.Add(other.transform);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(_targetTag.ToString()))
            {
                return;
            }

            _targetsInRange.Remove(other.transform);
        }
        
        private Transform GetClosestTarget()
        {
            Transform closestTarget = null;
            var closestDistance = float.MaxValue;

            foreach (var target in _targetsInRange)
            {
                if (!target)
                {
                    continue;
                }

                var distance = Vector2.Distance(_player.transform.position, target.position);

                if (distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = distance;
                closestTarget = target;
            }

            return closestTarget;
        }
        
        private void RemoveInvalidTargets()
        {
            for (var i = _targetsInRange.Count - 1; i >= 0; i--)
            {
                if (_targetsInRange[i] && _targetsInRange[i].CompareTag(_targetTag.ToString()))
                {
                    continue;
                }

                _targetsInRange.RemoveAt(i);
            }
        }
    }
}

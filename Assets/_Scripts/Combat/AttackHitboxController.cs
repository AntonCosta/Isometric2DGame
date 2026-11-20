using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace Combat
{
    public enum TargetTag
    {
        Player,
        Enemy
    }
    
    public class AttackHitboxController : MonoBehaviour
    {
        [SerializeField] private TargetTag _targetTag = TargetTag.Enemy;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private Collider2D _collider;
        
        private readonly HashSet<IDamageable> _hitTargets = new();
        private readonly List<Collider2D> _overlapResults = new();

        private void OnEnable()
        {
            _hitTargets.Clear();
            CheckCurrentOverlaps();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }
        
        private void CheckCurrentOverlaps()
        {
            if (!_collider)
            {
                return;
            }

            _overlapResults.Clear();
            _collider.Overlap(ContactFilter2D.noFilter, _overlapResults);

            foreach (var overlap in _overlapResults)
            {
                TryDamage(overlap);
            }
        }

        private void TryDamage(Collider2D other)
        {
            if (!other.CompareTag(_targetTag.ToString()))
            {
                return;
            }

            var damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null)
            {
                return;
            }

            if (!_hitTargets.Add(damageable))
            {
                return;
            }

            damageable.TakeDamage(_damage);
        }
    }
}
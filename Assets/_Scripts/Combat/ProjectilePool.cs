using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private ProjectileController _projectile;
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private bool _canExpand = true;

        private readonly List<ProjectileController> _projectiles = new List<ProjectileController>();

        private void Awake()
        {
            for (var i = 0; i < _initialPoolSize; i++)
            {
                CreateProjectile();
            }
        }
        
        public void SpawnProjectile(Vector2 position, Vector2 direction)
        {
            var projectile = GetAvailableProjectile();

            if (!projectile)
            {
                return;
            }

            projectile.transform.position = position;
            projectile.Fire(direction);
        }
        
        private ProjectileController GetAvailableProjectile()
        {
            foreach (var projectile in _projectiles)
            {
                if (!projectile.gameObject.activeInHierarchy)
                {
                    return projectile;
                }
            }

            return _canExpand ? CreateProjectile() : null;
        }
        
        private ProjectileController CreateProjectile()
        {
            var projectile = Instantiate(_projectile, transform);
            projectile.gameObject.SetActive(false);
            _projectiles.Add(projectile);

            return projectile;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIHealthController : MonoBehaviour
    {
        [SerializeField] private Image _health;

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            var healthPercent = currentHealth / maxHealth;
            var alpha = 1f - healthPercent;

            _health.color = new Color(1f, healthPercent, healthPercent, alpha);
        }
    }
}

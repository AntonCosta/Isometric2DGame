using UnityEngine;

namespace Enemy
{
    public class PatrolRoute : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private bool _loop = true;

        public int Count => _waypoints.Length;
        public bool Loop => _loop;

        public Transform GetWaypoint(int index)
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                return null;
            }

            return _waypoints[index];
        }
    }
}
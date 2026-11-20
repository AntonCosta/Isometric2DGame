using Player;
using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private AnimationEventsListener _listenerController;

    public void FinishAttack()
    {
        _listenerController.FinishAttack();
    }

    public void EnableAttackHitbox()
    {
        _listenerController.EnableAttackHitbox();
    }

    public void DisableAttackHitbox()
    {
        _listenerController.DisableAttackHitbox();
    }
    
    public void EndHurt()
    {
        _listenerController.EndHurt();
    }
}
using UnityEngine;


public abstract class AnimationEventsListener : MonoBehaviour
{
    public abstract void FinishAttack();

    public abstract void EnableAttackHitbox();

    public abstract void DisableAttackHitbox();

    public abstract void EndHurt();
}
using UnityEngine;

public static class Utils
{
    public const string ATTACK_STATE_NAME = "AttackingBlendTree";

    private const string MOVE_X = "MoveX";
    private const string MOVE_Y = "MoveY";
    private const string SPEED = "Speed";
    private const string IS_ATTACKING = "IsAttacking";

    public static readonly int MOVE_X_HASH = Animator.StringToHash(MOVE_X);
    public static readonly int MOVE_Y_HASH = Animator.StringToHash(MOVE_Y);
    public static readonly int SPEED_HASH = Animator.StringToHash(SPEED);
    public static readonly int IS_ATTACKING_HASH = Animator.StringToHash(IS_ATTACKING);
}

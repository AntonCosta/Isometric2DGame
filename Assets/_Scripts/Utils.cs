using UnityEngine;

public static class Utils
{
    public const string MOVE_X = "MoveX";
    public const string MOVE_Y = "MoveY";
    public const string SPEED = "Speed";
    public const string IS_ATTACKING = "IsAttacking";

    public static readonly int MOVE_X_HASH = Animator.StringToHash(MOVE_X);
    public static readonly int MOVE_Y_HASH = Animator.StringToHash(MOVE_Y);
    public static readonly int SPEED_HASH = Animator.StringToHash(SPEED);
    public static readonly int IS_ATTACKING_HASH = Animator.StringToHash(IS_ATTACKING);
}

using UnityEngine;

public static class Utils
{
    public const string ATTACK_STATE_NAME = "AttackingBlendTree";
    public const string PLAYER_TAG = "Player";
    public const string MOVE_ACTION = "Move";
    public const string ATTACK_ACTION = "Attack";
    public const string AUTO_ATTACK_ACTION = "AutoAttack";
    public const string INTERACT_ACTION = "Interact";
    public const string HEAL_ACTION = "Heal";
    public const string SKIP_DIALOGUE_ACTION = "Jump";
    

    private const string MOVE_X = "MoveX";
    private const string MOVE_Y = "MoveY";
    private const string SPEED = "Speed";
    private const string IS_ATTACKING = "IsAttacking";
    private const string TAKE_DAMAGE = "TakeDamage";
    private const string DIE = "Die";

    public static readonly int MOVE_X_HASH = Animator.StringToHash(MOVE_X);
    public static readonly int MOVE_Y_HASH = Animator.StringToHash(MOVE_Y);
    public static readonly int SPEED_HASH = Animator.StringToHash(SPEED);
    public static readonly int IS_ATTACKING_HASH = Animator.StringToHash(IS_ATTACKING);
    public static readonly int TAKE_DAMAGE_HASH = Animator.StringToHash(TAKE_DAMAGE);
    public static readonly int DIE_HASH = Animator.StringToHash(DIE);
}

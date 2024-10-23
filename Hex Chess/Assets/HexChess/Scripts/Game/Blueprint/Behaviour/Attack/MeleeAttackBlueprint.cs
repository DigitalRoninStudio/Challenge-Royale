using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttack", menuName = "BehaviourData/Attack/MeleeAttack")]
public class MeleeAttackBlueprint : AttackBehaviourBlueprint
{
    public override Behaviour CreateBehaviour() => new MeleeAttackBehaviour(this);
}

public abstract class AttackBehaviourBlueprint : BehaviourBlueprint
{
    public int BaseDamage;
    public int AttackRange;
    public DamageType DamageType;
    public float TimeToPerformAttack;

    [Header("VFX")]
    public GameObject HitImpact;
}

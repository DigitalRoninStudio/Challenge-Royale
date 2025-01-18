using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttack", menuName = "BehaviourData/Attack/MeleeAttack")]
public class MeleeAttackBlueprint : AttackBehaviourBlueprint
{
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new MeleeAttackBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new MeleeAttackBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new MeleeAttackBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as MeleeAttackData)
            .Build();
    }
}

public abstract class AttackBehaviourBlueprint : BehaviourBlueprint
{
    public int BaseDamage;
    public int AttackRange;
    public DamageType DamageType;
    public float TimeToPerformAttack;
}

public abstract class AbilityBlueprint : BehaviourBlueprint
{

}
public abstract class ActiveAbilityBlueprint : AbilityBlueprint
{
    public int MaxTargets;
    public int Radius;
    public int MaxCooldown;

}
public abstract class PassiveAbilityBlueprint : AbilityBlueprint
{

}

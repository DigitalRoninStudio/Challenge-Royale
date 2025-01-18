using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttack", menuName = "BehaviourData/Attack/RangedAttack")]
public class RangedAttackBlueprint : AttackBehaviourBlueprint
{
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new RangedAttackBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new RangedAttackBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new RangedAttackBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as RangedAttackData)
            .Build();
    }
}

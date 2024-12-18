using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttack", menuName = "BehaviourData/Attack/RangedAttack")]
public class RangedAttackBlueprint : AttackBehaviourBlueprint
{
    public override Behaviour CreateBehaviour()
    {
        return new RangedAttackBehaviour.Builder()
           .WithGeneratedId()
           .WithBlueprint(this)
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new RangedAttackBehaviour.Builder()
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new RangedAttackBehaviour.Builder()
            .WithBlueprint(this)
            .WithData(behaviourData as RangedAttackData)
            .Build();
    }
}

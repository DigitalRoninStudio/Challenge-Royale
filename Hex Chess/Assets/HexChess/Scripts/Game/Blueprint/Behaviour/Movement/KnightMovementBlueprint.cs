using System;
using UnityEngine;

[CreateAssetMenu(fileName = "KnightMovement", menuName = "BehaviourData/Movement/KnightMovement")]
public class KnightMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new KnightMovementBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new KnightMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new KnightMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as KnightMovementData)
            .Build();
    }
}

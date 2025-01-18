using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DirectionMovement", menuName = "BehaviourData/Movement/DirectionMovement")]
public class DirectionMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new DirectionMovementBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new DirectionMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new DirectionMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as DirectionMovementData)
            .Build();
    }
}

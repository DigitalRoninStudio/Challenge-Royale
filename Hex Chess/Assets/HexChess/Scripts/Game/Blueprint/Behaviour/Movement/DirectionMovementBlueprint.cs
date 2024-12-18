using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DirectionMovement", menuName = "BehaviourData/Movement/DirectionMovement")]
public class DirectionMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour()
    {
        return new DirectionMovementBehaviour.Builder()
           .WithGeneratedId()
           .WithBlueprint(this)
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new DirectionMovementBehaviour.Builder()
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new DirectionMovementBehaviour.Builder()
            .WithBlueprint(this)
            .WithData(behaviourData as DirectionMovementData)
            .Build();
    }
}

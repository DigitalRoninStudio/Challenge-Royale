using System;
using UnityEngine;

[CreateAssetMenu(fileName = "KnightMovement", menuName = "BehaviourData/Movement/KnightMovement")]
public class KnightMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour()
    {
        return new KnightMovementBehaviour.Builder()
           .WithGeneratedId()
           .WithBlueprint(this)
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new KnightMovementBehaviour.Builder()
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new KnightMovementBehaviour.Builder()
            .WithBlueprint(this)
            .WithData(behaviourData as KnightMovementData)
            .Build();
    }
}

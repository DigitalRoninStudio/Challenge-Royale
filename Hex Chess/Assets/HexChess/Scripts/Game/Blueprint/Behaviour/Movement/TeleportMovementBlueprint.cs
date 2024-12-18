using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportMovement", menuName = "BehaviourData/Movement/TeleportMovement")]
public class TeleportMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour()
    {
        return new TeleportMovementBehaviour.Builder()
           .WithGeneratedId()
           .WithBlueprint(this)
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new TeleportMovementBehaviour.Builder()
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new TeleportMovementBehaviour.Builder()
            .WithBlueprint(this)
            .WithData(behaviourData as TeleportMovementData)
            .Build();
    }
}
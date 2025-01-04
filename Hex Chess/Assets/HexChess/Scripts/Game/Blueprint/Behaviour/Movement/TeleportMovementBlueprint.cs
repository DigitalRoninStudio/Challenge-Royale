using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TeleportMovement", menuName = "BehaviourData/Movement/TeleportMovement")]
public class TeleportMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new TeleportMovementBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new TeleportMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new TeleportMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as TeleportMovementData)
            .Build();
    }
}
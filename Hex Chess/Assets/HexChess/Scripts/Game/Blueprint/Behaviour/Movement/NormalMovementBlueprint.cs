using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NormalMovement", menuName = "BehaviourData/Movement/NormalMovement")]
public class NormalMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new NormalMovementBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new NormalMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new NormalMovementBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as NormalMovementData)
            .Build();
    }
}

public abstract class MovementBehaviourBlueprint : BehaviourBlueprint
{
    public float Speed;
    public int Range;
}

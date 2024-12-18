using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NormalMovement", menuName = "BehaviourData/Movement/NormalMovement")]
public class NormalMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour()
    {
        return new NormalMovementBehaviour.Builder()
           .WithGeneratedId()
           .WithBlueprint(this)
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new NormalMovementBehaviour.Builder()
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new NormalMovementBehaviour.Builder()
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

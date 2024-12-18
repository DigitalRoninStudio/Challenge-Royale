using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Damageable", menuName = "BehaviourData/Damageable/Damageable")]
public class DamageableBlueprint : BehaviourBlueprint
{
    public int MaxHealth;
    public override Behaviour CreateBehaviour()
    {
        return new DamageableBehaviour.Builder()
           .WithGeneratedId()
           .WithBlueprint(this)
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new DamageableBehaviour.Builder()
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new DamageableBehaviour.Builder()
            .WithBlueprint(this)
            .WithData(behaviourData as DamageableBehaviourData)
            .Build();
    }
}

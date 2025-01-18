using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Damageable", menuName = "BehaviourData/Damageable/Damageable")]
public class DamageableBlueprint : BehaviourBlueprint
{
    public int MaxHealth;
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new DamageableBehaviour.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new DamageableBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new DamageableBehaviour.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as DamageableBehaviourData)
            .Build();
    }
}

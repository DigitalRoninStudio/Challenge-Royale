using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Disarm", menuName = "StatusEffectBlueprint/Disarm")]
public class DisarmBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new Disarm.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new Disarm.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new Disarm.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as DisarmData)
            .Build();
    }
}


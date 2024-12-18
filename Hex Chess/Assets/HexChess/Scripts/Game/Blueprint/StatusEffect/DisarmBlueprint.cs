using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Disarm", menuName = "StatusEffectBlueprint/Disarm")]
public class DisarmBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect(Behaviour owner)
    {
        return new Disarm.Builder()
             .WithGeneratedId(Guid.NewGuid().ToString())
             .WithBlueprint(this)
             .WithOwner(owner)
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner)
    {
        return new Disarm.Builder()
            .WithGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .WithOwner(owner)
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner)
    {
        return new Disarm.Builder()
            .WithBlueprint(this)
            .WithData(statusEffectData as DisarmData)
            .WithOwner(owner)
            .Build();
    }
}


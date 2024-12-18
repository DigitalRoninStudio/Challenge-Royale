using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageImmune", menuName = "StatusEffectBlueprint/DamageImmune")]
public class DamageImmuneBlueprint : StatusEffectBlueprint
{
    public DamageType DamageType;

    public override StatusEffect CreateStatusEffect(Behaviour owner)
    {
        return new DamageImmune.Builder()
             .WithGeneratedId(Guid.NewGuid().ToString())
             .WithBlueprint(this)
             .WithOwner(owner)
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner)
    {
        return new DamageImmune.Builder()
            .WithGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .WithOwner(owner)
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner)
    {
        return new DamageImmune.Builder()
            .WithBlueprint(this)
            .WithData(statusEffectData as DamageImmuneData)
            .WithOwner(owner)
            .Build();
    }
}

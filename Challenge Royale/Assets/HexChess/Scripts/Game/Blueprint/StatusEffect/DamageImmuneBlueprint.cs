using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageImmune", menuName = "StatusEffectBlueprint/DamageImmune")]
public class DamageImmuneBlueprint : StatusEffectBlueprint
{
    public DamageType DamageType;

    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new DamageImmune.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new DamageImmune.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new DamageImmune.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as DamageImmuneData)
            .Build();
    }
}

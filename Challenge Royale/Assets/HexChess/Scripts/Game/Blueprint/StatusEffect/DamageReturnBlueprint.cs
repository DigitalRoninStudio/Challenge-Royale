using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageReturn", menuName = "StatusEffectBlueprint/DamageReturn")]
public class DamageReturnBlueprint : StatusEffectBlueprint
{
    [Range(0f,1f)]
    public float ReturnPercentage;
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new DamageReturn.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new DamageReturn.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new DamageReturn.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as DamageReturnData)
            .Build();
    }
}

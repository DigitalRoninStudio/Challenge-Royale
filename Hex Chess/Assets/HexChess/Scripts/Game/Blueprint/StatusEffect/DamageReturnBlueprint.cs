using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageReturn", menuName = "StatusEffectBlueprint/DamageReturn")]
public class DamageReturnBlueprint : StatusEffectBlueprint
{
    [Range(0f,1f)]
    public float ReturnPercentage;
    public override StatusEffect CreateStatusEffect(Behaviour owner)
    {
        return new DamageReturn.Builder()
             .WithGeneratedId(Guid.NewGuid().ToString())
             .WithBlueprint(this)
             .WithOwner(owner)
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner)
    {
        return new DamageReturn.Builder()
            .WithGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .WithOwner(owner)
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner)
    {
        return new DamageReturn.Builder()
            .WithBlueprint(this)
            .WithData(statusEffectData as DamageReturnData)
            .WithOwner(owner)
            .Build();
    }
}

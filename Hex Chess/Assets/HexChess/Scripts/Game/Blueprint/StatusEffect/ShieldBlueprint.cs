using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "StatusEffectBlueprint/Shield")]
public class ShieldBlueprint : StatusEffectBlueprint
{
    public DamageType DamageType;
    public int MaxShieldHealth;
    public override StatusEffect CreateStatusEffect(Behaviour owner)
    {
        return new Shield.Builder()
             .WithGeneratedId(Guid.NewGuid().ToString())
             .WithBlueprint(this)
             .WithOwner(owner)
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner)
    {
        return new Shield.Builder()
            .WithGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .WithOwner(owner)
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner)
    {
        return new Shield.Builder()
            .WithBlueprint(this)
            .WithData(statusEffectData as ShieldData)
            .WithOwner(owner)
            .Build();
    }
}

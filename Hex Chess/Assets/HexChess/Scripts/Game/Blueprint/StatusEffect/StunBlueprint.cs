using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun", menuName = "StatusEffectBlueprint/Stun")]
public class StunBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect(Behaviour owner)
    {
        return new Stun.Builder()
             .WithGeneratedId(Guid.NewGuid().ToString())
             .WithBlueprint(this)
             .WithOwner(owner)
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner)
    {
        return new Stun.Builder()
            .WithGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .WithOwner(owner)
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner)
    {
        return new Stun.Builder()
            .WithBlueprint(this)
            .WithData(statusEffectData as StunData)
            .WithOwner(owner)
            .Build();
    }
}

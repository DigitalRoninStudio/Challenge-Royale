using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun", menuName = "StatusEffectBlueprint/Stun")]
public class StunBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new Stun.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new Stun.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new Stun.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as StunData)
            .Build();
    }
}

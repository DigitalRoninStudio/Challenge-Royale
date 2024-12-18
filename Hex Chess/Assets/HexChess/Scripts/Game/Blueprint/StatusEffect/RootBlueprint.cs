using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Root", menuName = "StatusEffectBlueprint/Root")]
public class RootBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect(Behaviour owner)
    {
        return new Root.Builder()
             .WithGeneratedId(Guid.NewGuid().ToString())
             .WithBlueprint(this)
             .WithOwner(owner)
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner)
    {
        return new Root.Builder()
            .WithGeneratedId(randomGenerator.NextGuid())
            .WithBlueprint(this)
            .WithOwner(owner)
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner)
    {
        return new Root.Builder()
            .WithBlueprint(this)
            .WithData(statusEffectData as RootData)
            .WithOwner(owner)
            .Build();
    }
}

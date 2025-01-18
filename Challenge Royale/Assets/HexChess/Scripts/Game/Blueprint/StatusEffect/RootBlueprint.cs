using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Root", menuName = "StatusEffectBlueprint/Root")]
public class RootBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new Root.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new Root.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new Root.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as RootData)
            .Build();
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "HealthModifier", menuName = "StatusEffectBlueprint/Modifiers/HealthModifier")]
public class HealthModifierBlueprint : StatusEffectBlueprint
{
    public int Value;
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new HealthModifier.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new HealthModifier.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new HealthModifier.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as HealthModifierData)
            .Build();
    }
}

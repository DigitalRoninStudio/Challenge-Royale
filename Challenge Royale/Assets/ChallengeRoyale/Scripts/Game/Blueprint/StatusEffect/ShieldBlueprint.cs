using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "StatusEffectBlueprint/Shield")]
public class ShieldBlueprint : StatusEffectBlueprint
{
    public DamageType DamageType;
    public int MaxShieldPoints;
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new Shield.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new Shield.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new Shield.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as ShieldData)
            .Build();
    }
}


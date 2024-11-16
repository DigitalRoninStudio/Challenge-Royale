using UnityEngine;

[CreateAssetMenu(fileName = "DamageImmune", menuName = "StatusEffectBlueprint/DamageImmune")]
public class DamageImmuneBlueprint : StatusEffectBlueprint
{
    public DamageType DamageType;
    public override StatusEffect CreateStatusEffect() => new DamageImmune(this);
}

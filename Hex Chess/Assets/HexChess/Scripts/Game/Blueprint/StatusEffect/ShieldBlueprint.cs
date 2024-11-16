using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "StatusEffectBlueprint/Shield")]
public class ShieldBlueprint : StatusEffectBlueprint
{
    public DamageType DamageType;
    public int MaxShieldHealth;
    public override StatusEffect CreateStatusEffect() => new Shield(this);
}

using UnityEngine;

[CreateAssetMenu(fileName = "DamageReturn", menuName = "StatusEffectBlueprint/DamageReturn")]
public class DamageReturnBlueprint : StatusEffectBlueprint
{
    [Range(0f,1f)]
    public float ReturnPercentage;
    public override StatusEffect CreateStatusEffect() => new DamageReturn(this);
}

using UnityEngine;

[CreateAssetMenu(fileName = "Disarm", menuName = "StatusEffectBlueprint/Disarm")]
public class DisarmBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect() => new Disarm(this);
}


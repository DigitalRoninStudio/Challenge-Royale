using UnityEngine;

[CreateAssetMenu(fileName = "Stun", menuName = "StatusEffectBlueprint/Stun")]
public class StunBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect() => new Stun(this);
}

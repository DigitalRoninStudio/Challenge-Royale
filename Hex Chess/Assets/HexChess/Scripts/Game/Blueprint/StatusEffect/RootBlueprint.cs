using UnityEngine;

[CreateAssetMenu(fileName = "Root", menuName = "StatusEffectBlueprint/Root")]
public class RootBlueprint : StatusEffectBlueprint
{
    public override StatusEffect CreateStatusEffect() => new Root(this);
}

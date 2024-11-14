public class Stun : StatusEffect
{
    public Stun() : base() { }
    public Stun(StunBlueprint blueprint) : base(blueprint) { }

    public override StatusEffectData GetStatusEffectData() => new StunData(this);
}

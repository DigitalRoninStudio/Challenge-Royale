using System;

public class Stun : StatusEffect
{
    public Stun() : base() { }
    public Stun(StunBlueprint blueprint) : base(blueprint) { }

    public override StatusEffectData GetStatusEffectData() => new StunData(this);
}

public class Disarm : StatusEffect
{
    public Disarm() : base() { }
    public Disarm(DisarmBlueprint blueprint) : base(blueprint) { }

    public override StatusEffectData GetStatusEffectData() => new DisarmData(this);
}

public class Root : StatusEffect
{
    public Root() : base() { }
    public Root(RootBlueprint blueprint) : base(blueprint) { }

    public override StatusEffectData GetStatusEffectData() => new RootData(this);
}

public class DamageImmune : StatusEffect
{
    public DamageType DamageType { get; private set; }
    public DamageImmune() : base() { }
    public DamageImmune(DamageImmuneBlueprint blueprint) : base(blueprint) 
    {
        DamageType = blueprint.DamageType;
    }

    public override StatusEffectData GetStatusEffectData() => new DamageImmuneData(this);
}

public class Shield : StatusEffect
{
    public DamageType DamageType { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public Shield() : base() { }
    public Shield(ShieldBlueprint blueprint) : base(blueprint)
    {
        DamageType = blueprint.DamageType;
        MaxHealth = blueprint.MaxShieldHealth;
        CurrentHealth = blueprint.MaxShieldHealth;
    }

    public override void FillWithData(StatusEffectData statusEffectData)
    {
        base.FillWithData(statusEffectData);
        if (statusEffectData is ShieldData shieldData)
        {
            CurrentHealth = shieldData.CurrentHealth;
        }
    }

    public override StatusEffectData GetStatusEffectData() => new ShieldData(this);

    public void ReceiveDamage(Damage damage)
    {
        int absorbedDamage = Math.Min(damage.Amount, CurrentHealth);
        CurrentHealth -= absorbedDamage;
        int remainingDamage = damage.Amount - absorbedDamage;

        damage.SetAmount(remainingDamage);
    }

    public bool IsShieldActive() => CurrentHealth > 0;
}

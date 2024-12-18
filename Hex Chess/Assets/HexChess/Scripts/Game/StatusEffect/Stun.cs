using System;

public class Stun : StatusEffect
{
    #region Builder
    public class Builder : Builder<Stun, StunBlueprint, StunData>
    {
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new StunData(this);
}

public class Disarm : StatusEffect
{
    #region Builder
    public class Builder : Builder<Disarm, DisarmBlueprint, DisarmData>
    {
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new DisarmData(this);
}

public class Root : StatusEffect
{
    #region Builder
    public class Builder : Builder<Root, RootBlueprint, RootData>
    {
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new RootData(this);
}

public class DamageImmune : StatusEffect
{
    public DamageType DamageType { get; private set; }

    #region Builder
    public class Builder : Builder<DamageImmune, DamageImmuneBlueprint, DamageImmuneData>
    {
        public new  Builder WithBlueprint(DamageImmuneBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.DamageType = blueprint.DamageType;

            return this;
        }
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new DamageImmuneData(this);
}

public class DamageReturn : StatusEffect
{
    public float ReturnPercentage { get; private set; }

    #region Builder
    public class Builder : Builder<DamageReturn, DamageReturnBlueprint, DamageReturnData>
    {
        public new Builder WithBlueprint(DamageReturnBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.ReturnPercentage = blueprint.ReturnPercentage;

            return this;
        }
    }
    #endregion

    public override StatusEffectData GetStatusEffectData() => new DamageReturnData(this);
}


public class Shield : StatusEffect
{
    public DamageType DamageType { get; private set; }
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }

    #region Builder
    public class Builder : Builder<Shield, ShieldBlueprint, ShieldData>
    {
        public new Builder WithBlueprint(ShieldBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.DamageType = blueprint.DamageType;
            _statusEffect.MaxHealth = blueprint.MaxShieldHealth;
            _statusEffect.CurrentHealth = blueprint.MaxShieldHealth;

            return this;
        }

        public new Builder WithData(ShieldData shieldData)
        {
            base.WithData(shieldData);
            _statusEffect.CurrentHealth = shieldData.CurrentHealth;

            return this;
        }
    }
    #endregion

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

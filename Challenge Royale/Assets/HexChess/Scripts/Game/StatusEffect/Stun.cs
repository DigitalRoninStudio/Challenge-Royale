using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Stun : StatusEffect
{

    #region Builder
    public class Builder : Builder<Stun, StunBlueprint, StunData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new StunData(this);
}

public class Disarm : StatusEffect
{
    #region Builder
    public class Builder : Builder<Disarm, DisarmBlueprint, DisarmData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new DisarmData(this);
}

public class Root : StatusEffect
{
    #region Builder
    public class Builder : Builder<Root, RootBlueprint, RootData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
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
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
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
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
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

public class DamageModifier : StatusEffect
{
    public int Value { get; private set; }

    #region Builder
    public class Builder : Builder<DamageModifier, DamageModifierBlueprint, DamageModifierData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }

        public new Builder WithBlueprint(DamageModifierBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.Value = blueprint.Value;

            return this;
        }
    }
    #endregion
    public override void ApplyEffect()
    {
        base.ApplyEffect();

        AttackBehaviour attackBehaviour = target.GetBehaviour<AttackBehaviour>();
        if (attackBehaviour != null)
            attackBehaviour.ModifyDamage(Value);
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();

        AttackBehaviour attackBehaviour = target.GetBehaviour<AttackBehaviour>();
        if (attackBehaviour != null)
            attackBehaviour.ModifyDamage(-Value);
    }
    public override StatusEffectData GetStatusEffectData() => new DamageModifierData(this);

}

public class HealthModifier : StatusEffect
{
    public int Value { get; private set; }

    #region Builder
    public class Builder : Builder<HealthModifier, HealthModifierBlueprint, HealthModifierData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
        public new Builder WithBlueprint(HealthModifierBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.Value = blueprint.Value;

            return this;
        }
    }
    #endregion

    public override void ApplyEffect()
    {
        base.ApplyEffect();

        DamageableBehaviour damageable = target.GetBehaviour<DamageableBehaviour>();
        if (damageable != null)
        {
            damageable.IncreaseMaxHealth(Value);
            damageable.IncreaseCurrentHealth(Value);
        }
    }

    public override void RemoveEffect()
    {
        base.RemoveEffect();

        DamageableBehaviour damageable = target.GetBehaviour<DamageableBehaviour>();
        if(damageable != null)
        {
            damageable.IncreaseMaxHealth(-Value);
            if (damageable.CurrentHealth > damageable.MaxHealth)
                damageable.SetCurrentHealth(damageable.MaxHealth);
        }

    }
    public override StatusEffectData GetStatusEffectData() => new HealthModifierData(this);
}

public class Shield : StatusEffect
{
    public DamageType DamageType { get; private set; }
    public int MaxPoints { get; private set; }
    public int ShieldPoints { get; private set; }

    #region Builder
    public class Builder : Builder<Shield, ShieldBlueprint, ShieldData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
        public new Builder WithBlueprint(ShieldBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.DamageType = blueprint.DamageType;
            _statusEffect.MaxPoints = blueprint.MaxShieldPoints;
            _statusEffect.ShieldPoints = blueprint.MaxShieldPoints;

            return this;
        }

        public new Builder WithData(ShieldData shieldData)
        {
            base.WithData(shieldData);
            _statusEffect.ShieldPoints = shieldData.ShieldPoints;

            return this;
        }
    }
    #endregion

    public override StatusEffectData GetStatusEffectData() => new ShieldData(this);

    public void ReceiveDamage(Damage damage)
    {
        int absorbedDamage = Math.Min(damage.Amount, ShieldPoints);
        ShieldPoints -= absorbedDamage;
        int remainingDamage = damage.Amount - absorbedDamage;

        damage.SetAmount(remainingDamage);
    }

    public bool IsShieldActive() => ShieldPoints > 0;
}
public class DodgeCastAttack : StatusEffect
{
    public int DodgeChance;
    #region Builder
    public class Builder : Builder<DodgeCastAttack, DodgeCastAttackBlueprint, DodgeCastAttackData>
    {
        public Builder(Behaviour owner, Entity target) : base(owner, target) { }
        public new Builder WithBlueprint(DodgeCastAttackBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _statusEffect.DodgeChance = blueprint.DodgeChance;
            return this;
        }
    }
    #endregion
    public override StatusEffectData GetStatusEffectData() => new DodgeCastAttackData(this);
    public async Task<bool> TryToDodge(Game game)
    {
        if(DodgeChance == 1) return true;

        ExecutedAction executedAction = game.executedClientsActions.LastOrDefault();
        if(executedAction != null && executedAction.actionData != null && executedAction.actionData is AttackActionData attackAction)
        {
            if (owner.Owner.TryGetBehaviour<DamageableBehaviour>(out var damageableBehaviour))
            {
                if (attackAction.DamageableGUID == damageableBehaviour.guid)
                    return await game.dice.RollAndCheck(DodgeChance);
            }
        }

        return false;
    }
}
public class DodgeCastSpell : StatusEffect
{
    public int DodgeChance;
     #region Builder
     public class Builder : Builder<DodgeCastSpell, DodgeCastSpellBlueprint, DodgeCastSpellData>
     {
         public Builder(Behaviour owner, Entity target) : base(owner, target) { }
         public new Builder WithBlueprint(DodgeCastSpellBlueprint blueprint)
         {
             base.WithBlueprint(blueprint);
             _statusEffect.DodgeChance = blueprint.DodgeChance;
             return this;
         }
     }
     #endregion
    public override StatusEffectData GetStatusEffectData() => new DodgeCastSpellData(this);
    public async Task<bool> TryToDodge(Game game)
    {
        return false;
    }
}

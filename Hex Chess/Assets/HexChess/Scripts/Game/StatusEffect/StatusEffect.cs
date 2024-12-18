using UnityEngine;

public abstract class StatusEffect
{
    public string guid;
    public Behaviour owner;
    public int duration;
    public StatusEffectBlueprint StatusEffectBlueprint { get; private set; }
    protected StatusEffect() { }
    #region Builder
    public class Builder<T, TB, TD>
       where T : StatusEffect, new()
       where TB : StatusEffectBlueprint
       where TD : StatusEffectData
    {
        protected readonly T _statusEffect;

        public Builder()
        {
            _statusEffect = new T();
        }
        public virtual Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            _statusEffect.StatusEffectBlueprint = blueprint;
            _statusEffect.duration = blueprint.Duration;
            return this;
        }

        public Builder<T, TB, TD> WithGeneratedId(string guid)
        {
            _statusEffect.guid = guid;
            return this;
        }

        public Builder<T, TB, TD> WithOwner(Behaviour behaviour)
        {
            _statusEffect.owner = behaviour;
            return this;
        }

        public virtual Builder<T, TB, TD> WithData(TD entityData)
        {
            _statusEffect.duration = entityData.Duration;
            return this;
        }

        public T Build() => _statusEffect;
    }
    #endregion
    public virtual void ApplyEffect() { }
    public virtual void ExecuteEffect() { }
    public virtual void RemoveEffect() { }

    public abstract StatusEffectData GetStatusEffectData();
}


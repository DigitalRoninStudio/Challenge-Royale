using System;
using UnityEngine;

public abstract class StatusEffect
{
    public string guid;
    public Behaviour owner;
    public int duration;
    public Entity target;
    public StatusEffectBlueprint StatusEffectBlueprint { get; private set; }
    protected StatusEffect() { }
    #region Builder
    public class Builder<T, TB, TD>
       where T : StatusEffect, new()
       where TB : StatusEffectBlueprint
       where TD : StatusEffectData
    {
        protected readonly T _statusEffect;
        /*
         
          public Builder(T behaviour, Entity owner)
        {
            _behaviour = behaviour;
            _behaviour.Owner = owner;
        }
         
         */
        public Builder(Behaviour owner, Entity target)
        {
            _statusEffect = new T(); 
            _statusEffect.owner = owner;
            _statusEffect.target = target;
        }
        public virtual Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            _statusEffect.StatusEffectBlueprint = blueprint;
            _statusEffect.duration = blueprint.Duration; 
            return this;
        }

        public Builder<T, TB, TD> WithGeneratedId()
        {
            _statusEffect.guid = Guid.NewGuid().ToString();
            return this;
        }
        public Builder<T, TB, TD> WithSyncGeneratedId(string guid)
        {
            _statusEffect.guid = guid;
            return this;
        }

        public virtual Builder<T, TB, TD> WithData(TD statisEffectData)
        {
            _statusEffect.guid = statisEffectData.Guid;
            _statusEffect.duration = statisEffectData.Duration;
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


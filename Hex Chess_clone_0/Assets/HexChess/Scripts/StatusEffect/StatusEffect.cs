using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class StatusEffect
{
    public string OwnerId => ownerId;
    public string ownerId;
    public string Name;
    public StatusEffectType Status;
    public StackType StackType;

    public int Duration;
    public bool IsPermanent;

    public StatusEffect() { }
    public StatusEffect(string ownerId) => this.ownerId = ownerId;
    public abstract void ApplyEffect();
    public abstract void ExecuteEffect();
    public abstract void RemoveEffect();

}

public class StatusEffectController
{
    private List<StatusEffect> StatusEffects;
    public StatusEffectController()
    {
        StatusEffects = new List<StatusEffect>();
    }

    public void ProcessStatusEffects()
    {
        List<StatusEffect> statusEffectsToRemove = new List<StatusEffect>();
        foreach (var statusEffect in StatusEffects)
        {
            statusEffect.ExecuteEffect();
            if (ProcessStatusEffectDuration(statusEffect))
                statusEffectsToRemove.Add(statusEffect);
        }

        foreach (var statusEffect in statusEffectsToRemove)
            RemoveStatusEffect(statusEffect);

        foreach (var statusEffect in StatusEffects)
        {
            Debug.Log("CONTROLLER CONTAIN: " + statusEffect.Name);
        }
    }
    public void AddStatusEffect(StatusEffect statusEffect)
    {
        if (string.IsNullOrEmpty(statusEffect.OwnerId) || (statusEffect.Duration == 0 && !statusEffect.IsPermanent))
        {
            statusEffect.ExecuteEffect();
            return;
        }

        var existingEffect = GetExistingStatusEffect(statusEffect);

        if (existingEffect == null)
        {
            StatusEffects.Add(statusEffect);
            statusEffect.ApplyEffect();
            return;
        }

        switch (statusEffect.StackType)
        {
            case StackType.Stacks:
                Debug.Log("STACK");
                statusEffect.Duration += existingEffect.Duration;
                RemoveStatusEffect(existingEffect);

                StatusEffects.Add(statusEffect);
                statusEffect.ApplyEffect();
                break;

            case StackType.Refresh:
                Debug.Log("REFRESH");
                RemoveStatusEffect(existingEffect);

                StatusEffects.Add(statusEffect);
                statusEffect.ApplyEffect();
                break;

            case StackType.None:
                Debug.Log("NONE");
                StatusEffects.Add(statusEffect);
                statusEffect.ApplyEffect();
                break;
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        StatusEffects.Remove(statusEffect);
        statusEffect.RemoveEffect();
    }
    public bool HasStatusEffect<T>() where T : StatusEffect
    {
        return StatusEffects.Exists(b => b is T);
    }
    public T GetStatusEffect<T>() where T : StatusEffect
    {
        return StatusEffects.OfType<T>().FirstOrDefault();
    }

    public List<T> GetBuffsOfType<T>() where T : StatusEffect
    {
        return StatusEffects.OfType<T>().ToList();
    }

    public void RemoveAllStatusEffects()
    {
        foreach (var statusEffect in StatusEffects.ToList())
            RemoveStatusEffect(statusEffect);
        StatusEffects.Clear();
    }

    public void RemoveAllDebuffs()
    {
        var debuffsToRemove = StatusEffects.Where(b =>
            b.Status == StatusEffectType.Debuff);

        foreach (var debuff in debuffsToRemove)
            RemoveStatusEffect(debuff);
    }

    protected virtual bool ProcessStatusEffectDuration(StatusEffect statusEffect)
    {
        if (statusEffect.IsPermanent) return false;

        statusEffect.Duration--;
        return statusEffect.Duration <= 0;
    }
    private StatusEffect GetExistingStatusEffect(StatusEffect newStatusEffect)
    {
        return StatusEffects.FirstOrDefault(effect =>
            effect.GetType() == newStatusEffect.GetType() &&
            effect.OwnerId == newStatusEffect.OwnerId);
    }
}

public enum StatusEffectType
{
    Buff, Debuff
}

public enum StackType
{
    None, Refresh, Stacks
}
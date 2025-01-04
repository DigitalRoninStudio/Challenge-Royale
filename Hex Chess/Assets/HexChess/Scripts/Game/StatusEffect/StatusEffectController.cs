using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectController
{
    public List<StatusEffect> StatusEffects => statusEffects;
    private List<StatusEffect> statusEffects;
    public StatusEffectController()
    {
        statusEffects = new List<StatusEffect>();
    }

    public void ProcessStatusEffects()
    {
        List<StatusEffect> statusEffectsToRemove = new List<StatusEffect>();
        foreach (var statusEffect in statusEffects)
        {
            statusEffect.ExecuteEffect();
            if (ProcessStatusEffectDuration(statusEffect))
                statusEffectsToRemove.Add(statusEffect);
        }

        foreach (var statusEffect in statusEffectsToRemove)
            RemoveStatusEffect(statusEffect);
    }
    public void AddStatusEffect(StatusEffect statusEffect)
    {
        if (statusEffect.duration == 0 && !statusEffect.StatusEffectBlueprint.IsPermanent)
        {
            statusEffect.ExecuteEffect();
            return;
        }

        var existingEffect = GetExistingStatusEffect(statusEffect);

        if (existingEffect == null)
        {
            statusEffects.Add(statusEffect);
            Debug.Log($"STATUS EFFECT {statusEffect.GetType()} ADDED");
            statusEffect.ApplyEffect();
            return;
        }

        switch (statusEffect.StatusEffectBlueprint.StackType)
        {
            case StackType.Stacks:
                Debug.Log("STACK");
                statusEffect.duration += existingEffect.duration;
                RemoveStatusEffect(existingEffect);

                statusEffects.Add(statusEffect);
                statusEffect.ApplyEffect();
                break;

            case StackType.Refresh:
                Debug.Log("REFRESH");
                RemoveStatusEffect(existingEffect);

                statusEffects.Add(statusEffect);
                statusEffect.ApplyEffect();
                break;

            case StackType.None:
                Debug.Log("NONE");
                statusEffects.Add(statusEffect);
                statusEffect.ApplyEffect();
                break;
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect);
        statusEffect.RemoveEffect();
        Debug.Log($"STATUS EFFECT {statusEffect.GetType()} REMOVED");
    }
    public bool HasStatusEffect<T>() where T : StatusEffect
    {
        return statusEffects.Exists(b => b is T);
    }
    public T GetStatusEffect<T>() where T : StatusEffect
    {
        return statusEffects.OfType<T>().FirstOrDefault();
    }

    public List<T> GetStatusEffectsOfType<T>() where T : StatusEffect
    {
        return statusEffects.OfType<T>().ToList();
    }

    public void RemoveAllStatusEffects()
    {
        foreach (var statusEffect in statusEffects.ToList())
            RemoveStatusEffect(statusEffect);
        statusEffects.Clear();
    }

    public void RemoveAllDebuffs()
    {
        var debuffsToRemove = statusEffects.Where(b =>
            b.StatusEffectBlueprint.Status == StatusEffectType.Debuff);

        foreach (var debuff in debuffsToRemove)
            RemoveStatusEffect(debuff);
    }

    protected virtual bool ProcessStatusEffectDuration(StatusEffect statusEffect)
    {
        if (statusEffect.StatusEffectBlueprint.IsPermanent) return false;

        statusEffect.duration--;
        return statusEffect.duration <= 0;
    }
    private StatusEffect GetExistingStatusEffect(StatusEffect newStatusEffect)
    {
        return statusEffects.FirstOrDefault(effect =>
            effect.GetType() == newStatusEffect.GetType());
            // && effect.OwnerId == newStatusEffect.OwnerId); maybe use blueprintid
    }
}

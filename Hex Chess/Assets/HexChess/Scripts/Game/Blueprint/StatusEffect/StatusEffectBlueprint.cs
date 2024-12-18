using UnityEngine;

public abstract class StatusEffectBlueprint : ScriptableObject
{
    public string Id;
    public string Name;
    public StatusEffectType Status;
    public StackType StackType;

    public int Duration;
    public bool IsPermanent;

    public abstract StatusEffect CreateStatusEffect(Behaviour Owner);
    public abstract StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour Owner);
    public abstract StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour Owner);
}

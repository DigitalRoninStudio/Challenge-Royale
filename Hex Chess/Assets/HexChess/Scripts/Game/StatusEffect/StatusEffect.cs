public abstract class StatusEffect
{
    public int Duration;
    public StatusEffectBlueprint StatusEffectBlueprint { get; private set; }
    public StatusEffect() { }
    public StatusEffect(StatusEffectBlueprint blueprint)
    {
        StatusEffectBlueprint = blueprint;
        Duration = blueprint.Duration;
    }
    public virtual void ApplyEffect() { }
    public virtual void ExecuteEffect() { }
    public virtual void RemoveEffect() { }

    public abstract StatusEffectData GetStatusEffectData();
    public virtual void FillWithData(StatusEffectData statusEffectData)
    {
        Duration = statusEffectData.Duration;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalBlueprints", menuName = "GlobalBlueprints")]
public class GlobalBlueprints : ScriptableObject
{
    public List<MapBlueprint> MapBlueprints = new List<MapBlueprint>();
    public List<FractionBlueprint> FractionBlueprints = new List<FractionBlueprint>();
    public BehaviourBlueprintsContainer BehaviourDatasContainer = new BehaviourBlueprintsContainer();
    public StatusEffectBlueprintsContainer StatusEffectBlueprintsContainer = new StatusEffectBlueprintsContainer();
}
[Serializable]
public class FractionBlueprint
{
    public FractionType FractionType;
    public List<EntityBlueprint> EntityBlueprints = new List<EntityBlueprint>();

}

[Serializable]
public class BehaviourBlueprintsContainer
{
    public List<BehaviourBlueprint> MovementBehaviourBlueprints = new List<BehaviourBlueprint>();
    public List<BehaviourBlueprint> AttackBehaviourBlueprints = new List<BehaviourBlueprint>();
    public List<BehaviourBlueprint> DamageableBehaviourBlueprints = new List<BehaviourBlueprint>();
    public List<BehaviourBlueprint> AbilityBehaviourBlueprints = new List<BehaviourBlueprint>();

    public BehaviourBlueprint GetBehaviourBlueprint(BehaviourData behaviourData)
    {
        Behaviour behaviour = null;

        if (behaviourData is MovementBehaviourData)
            return MovementBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id);
        else if (behaviourData is AttackBehaviourData)
            return AttackBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id);
        else if (behaviourData is DamageableBehaviourData)
            return DamageableBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id);
        else if (behaviourData is AbilityBehaviourData)
            return AbilityBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id);

        return null;
    }

}

[Serializable]
public class StatusEffectBlueprintsContainer
{
    public List<StatusEffectBlueprint> StunBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> DisarmBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> RootBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> DamageImmuneBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> ShieldBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> HealthModifierBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> DamageModifierBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> DodgeCastAttackBlueprints = new List<StatusEffectBlueprint>();
    public List<StatusEffectBlueprint> DodgeCastSpellBlueprints = new List<StatusEffectBlueprint>();

    public StatusEffectBlueprint GetStatusEffectBlueprint(StatusEffectData statusEffectData)
    {
        if (statusEffectData is StunData)
            return StunBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is DisarmData)
            return DisarmBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is RootData)
            return RootBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is DamageImmuneData)
            return DamageImmuneBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is ShieldData)
            return ShieldBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is HealthModifierData)
            return HealthModifierBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is DamageModifierData)
            return DamageModifierBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is DodgeCastAttackData)
            return DodgeCastAttackBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);
        if (statusEffectData is DodgeCastSpellData)
            return DodgeCastSpellBlueprints.FirstOrDefault(s => s.Id == statusEffectData.BlueprintId);

        return null;
    }
}




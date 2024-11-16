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

    public Behaviour GetBehaviour(BehaviourData behaviourData)
    {
        Behaviour behaviour = null;

        if(behaviourData is MovementBehaviourData)
            behaviour = MovementBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id)?.CreateBehaviour();
        else if(behaviourData is AttackBehaviourData)
            behaviour = AttackBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id)?.CreateBehaviour();
        else if(behaviourData is DamageableBehaviourData)
            behaviour = DamageableBehaviourBlueprints.FirstOrDefault(b => b.Id == behaviourData.Id)?.CreateBehaviour();

        return behaviour;
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

    public StatusEffect GetStatusEffect(StatusEffectData statusEffectData)
    {
        StatusEffect statusEffect = null;

        if(statusEffectData is StunData)
            statusEffect = StunBlueprints.FirstOrDefault(s => s.Id == statusEffectData.Id)?.CreateStatusEffect();
        if (statusEffectData is DisarmData)
            statusEffect = DisarmBlueprints.FirstOrDefault(s => s.Id == statusEffectData.Id)?.CreateStatusEffect();
        if (statusEffectData is RootData)
            statusEffect = RootBlueprints.FirstOrDefault(s => s.Id == statusEffectData.Id)?.CreateStatusEffect();
        if (statusEffectData is DamageImmuneData)
            statusEffect = DamageImmuneBlueprints.FirstOrDefault(s => s.Id == statusEffectData.Id)?.CreateStatusEffect();
        if (statusEffectData is ShieldData)
            statusEffect = ShieldBlueprints.FirstOrDefault(s => s.Id == statusEffectData.Id)?.CreateStatusEffect();

        return statusEffect;
    }
}




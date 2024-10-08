using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalBlueprints", menuName = "GlobalBlueprints")]
public class GlobalBlueprints : ScriptableObject
{
    public List<MapBlueprint> MapBlueprints = new List<MapBlueprint>();
    public List<FractionBlueprint> FractionBlueprints = new List<FractionBlueprint>();
    public BehaviourBlueprintsContainer BehaviourDatasContainer = new BehaviourBlueprintsContainer();
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

}



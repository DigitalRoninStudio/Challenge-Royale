using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "BehaviourData", menuName = "BehaviourData/BehaviourData")]
public abstract class BehaviourBlueprint : ScriptableObject
{
    public string Id;
    public string Name;
    public int EnergyCost = 1;
    public int MaxCast = 1;

    public BehaviourVisual Visual;
    public abstract Behaviour CreateBehaviour(Entity owner);
    public abstract Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner);
    public abstract Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner);
}

using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "BehaviourData", menuName = "BehaviourData/BehaviourData")]
public abstract class BehaviourBlueprint : ScriptableObject
{
    public string Id;
    public string Name;

    public BehaviourVisual Visual;
    public abstract Behaviour CreateBehaviour(Entity owner);
    public abstract Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner);
    public abstract Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner);
}

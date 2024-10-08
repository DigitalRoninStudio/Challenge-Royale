using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBlueprint : ScriptableObject
{
    public string Id;
    public string Name;
    public GameObject GameObject;

    public List<BehaviourBlueprint> BehaviourDatas = new List<BehaviourBlueprint>();

    //public abstract Entity CreateEntity(EntityData entityData, GameFactory factory);
    public abstract Entity CreateEntity();
}


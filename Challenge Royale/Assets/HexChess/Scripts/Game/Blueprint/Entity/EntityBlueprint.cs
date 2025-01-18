using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EntityBlueprint : ScriptableObject
{
    public string Id;
    public string Name;
    public bool IsBlockingMovement = true;
    public GameObject GameObject;

    public List<BehaviourBlueprint> BehaviourDatas = new List<BehaviourBlueprint>();

    public abstract Entity CreateEntity();
    public abstract Entity CreateEntity(RandomGenerator randomGenerator);
    public abstract Entity CreateEntity(EntityData entityData);
}


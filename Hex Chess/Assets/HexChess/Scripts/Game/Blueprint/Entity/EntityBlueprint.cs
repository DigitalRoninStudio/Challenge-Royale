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

    [Header("Visual Data")]
    public Sprite Icon;
    public bool Fliped;

    //public abstract Entity CreateEntity(EntityData entityData, GameFactory factory);
    public abstract Entity CreateEntity();
}


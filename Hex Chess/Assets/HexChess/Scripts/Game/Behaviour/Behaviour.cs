using System;
using UnityEngine;

public abstract class Behaviour
{
    public string guid;
    public string blueprintId;
    public string name;
    public Entity owner;
    protected float time;
    protected Map Map => owner.Owner.match.map;

    public Action<Behaviour> OnBehaviourStart;
    public Action<Behaviour> OnBehaviourEnd;

    public Behaviour() { }
    public Behaviour(BehaviourBlueprint blueprint)
    {
        guid = Guid.NewGuid().ToString();
        blueprintId = blueprint.Id;
        name = blueprint.Name;
    }
    public virtual void Enter() 
    { 
        time = Time.time;
    }
    public abstract void Execute();
    public virtual void Exit()
    {
        owner.ChangeBehaviour();
    }
    public abstract BehaviourData GetBehaviourData();
    public virtual void FillWithData(BehaviourData behaviourData)
    {
        guid = behaviourData.GUID;
    }
}

public interface ISerializableAction
{
    string SerializeAction();
    void DeserializeAction(string data);
}

public class BehaviourAction
{
}




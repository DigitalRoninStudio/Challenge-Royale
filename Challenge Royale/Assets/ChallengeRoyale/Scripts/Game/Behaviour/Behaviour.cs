using Newtonsoft.Json;
using System;
using UnityEngine;

public abstract class Behaviour : ISerializableBehaviour
{
    public string id;
    [JsonRequired] protected Entity entity;
    protected float time;

    public Action<Behaviour> OnBehaviourStart;
    public Action<Behaviour> OnBehaviourEnd;

    public Behaviour() : base() { }
    public Behaviour(Entity entity) { this.entity = entity; }
    public virtual void Enter() 
    { 
        time = Time.deltaTime;
        entity.OnBehaviourStart?.Invoke(this);
    }
    public abstract void Execute();
    public virtual void Exit()
    {
        entity.OnBehaviourEnd?.Invoke(this);
        entity.ChangeBehaviour();
    }
    public abstract string Serialize();
    public abstract void Deserialize(string data);
}

public interface ISerializableBehaviour
{
    string Serialize();
    void Deserialize(string data);
}

public class BehaviourData
{
}




using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Behaviour
{
    public string guid;
    public string name;
    public Entity Owner { get; private set; }
    protected float time;
    protected Map Map => Owner.Owner.match.map;
    public BehaviourBlueprint BehaviourBlueprint { get; private set; }
    public Behaviour() { }
    public Behaviour(BehaviourBlueprint blueprint)
    {
        guid = Guid.NewGuid().ToString();

        BehaviourBlueprint = blueprint;
        name = blueprint.Name;
    }

    public abstract BehaviourData GetBehaviourData();
    public virtual void FillWithData(BehaviourData behaviourData)
    {
        guid = behaviourData.GUID;
    }

    public virtual void SetOwner(Entity entity)
    {
        Owner = entity;

        if (BehaviourBlueprint.Visual != null)
        {//?
            BehaviourVisual visual = GameObject.Instantiate(BehaviourBlueprint.Visual, Owner.gameObject.transform);
            visual.Initialize(this);
        }
    }
}


public interface ITileSelection
{
    List<Tile> GetAvailableTiles();
    List<Tile> GetTiles();
    List<Tile> GetUnAvailableTilesInRange() => GetTiles().Except(GetAvailableTiles()).ToList();
}

public interface ILifecycleAction
{
    void Enter();
    void Execute();
    void Exit();

    Action OnActionStart { get; set; }
    Action OnActionExecuted { get; set; }
    Action OnActionEnd { get; set; }
}

public interface INetAction
{
    NetGameAction SerializeAction();
    void DeserializeAction(NetGameAction gameAction);
}




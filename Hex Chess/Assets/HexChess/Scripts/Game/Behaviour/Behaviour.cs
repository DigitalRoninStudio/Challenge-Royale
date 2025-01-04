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
    protected Behaviour() { }
    #region Builder
    public class Builder<T, TB, TD>
     where T : Behaviour
     where TB : BehaviourBlueprint
     where TD : BehaviourData
    {
        protected T _behaviour;

        public Builder(T behaviour, Entity owner)
        {
            _behaviour = behaviour;
            _behaviour.Owner = owner;
        }
        public virtual Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            _behaviour.BehaviourBlueprint = blueprint;
            _behaviour.name = blueprint.name;

            return this;
        }
        public Builder<T, TB, TD> WithGeneratedId()
        {
            _behaviour.guid = Guid.NewGuid().ToString();
            return this;
        }
        public virtual Builder<T, TB, TD> WithSyncGeneratedId(string guid)
        {
            _behaviour.guid = guid;
            return this;
        }

        public virtual Builder<T, TB, TD> WithData(TD behaviourData)
        {
            _behaviour.guid = behaviourData.GUID;
            return this;
        }

        public T Build() => _behaviour;
    }
    #endregion
    public abstract BehaviourData GetBehaviourData();

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
    ActionType ActionType { get; }
    string SerializeAction();
    void DeserializeAction(string action);
}


public class RoundActionData
{
    public bool EndRound;
    public bool SwitchInitiation;
}

public class BehaviourActionData
{
    public string EntityGUID;
    public string BehaviourGUID;
}

public class MovementActionData : BehaviourActionData
{
    public Vector2Int TileCoordinate;
}

public class AttackActionData : BehaviourActionData
{
    public string EnemyGUID;
    public string DamageableGUID;
}

public class SwordsmanSpecialActionData : BehaviourActionData
{
}






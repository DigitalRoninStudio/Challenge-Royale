using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEditor;
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

public static class BehaviourUtility
{
    public static void BroadcastActionToClients(Behaviour behaviour)
    {
        if (!Server.IsServer || behaviour is not ISerializableAction action)
            return;

        string serializedAction = action.SerializeAction();
        NetGameAction responess = new NetGameAction()
        {
            entityGUID = behaviour.Owner.guid,
            behaviourGUID = behaviour.guid,
            serializedBehaviour = serializedAction,
        };

        foreach (var player in behaviour.Owner.Owner.match.players)
        {
            NetworkConnection connection = GameManager.Instance.GetNetworkConnection(player.clientId);
            if (connection != null)
                Sender.ServerSendData(connection, responess, Pipeline.Reliable);
        }
    }
}


public interface IActionTileSelection
{
    List<Tile> GetAvailableTiles();
    List<Tile> GetTiles();
    List<Tile> GetUnAvailableTilesInRange() => GetTiles().Except(GetAvailableTiles()).ToList();
}

public interface IActionLifecycle
{
    void Enter();
    void Execute();
    void Exit();

    Action OnActionStart { get; set; }
    Action OnActionExecuted { get; set; }
    Action OnActionEnd { get; set; }
}

public interface ISerializableAction
{
    string SerializeAction();
    void DeserializeAction(string data);
}

public class BehaviourAction
{
}


public class MovementBehaviourAction : BehaviourAction
{
    public Vector2Int EndCoord { get; set; }
}

public class AttackBehaviourAction : BehaviourAction
{
    public string EnemyGUID { get; set; }
    public string DamageableGUID { get; set; }
}




using System;
using Unity.Networking.Transport;
using UnityEngine;

public abstract class Behaviour
{
    public string guid;
    public string blueprintId;
    public string name;
    public Entity Owner { get; private set; }
    protected float time;
    protected Map Map => Owner.Owner.match.map;

    public Action<Behaviour> OnBehaviourStart;
    //public Action<Behaviour> OnBehaviourExecute; ?
    public Action<Behaviour> OnBehaviourEnd;

    protected BehaviourBlueprint blueprint;
    public Behaviour() { }
    public Behaviour(BehaviourBlueprint blueprint)
    {
        guid = Guid.NewGuid().ToString();

        this.blueprint = blueprint;
        blueprintId = blueprint.Id;
        name = blueprint.Name;
    }
    public virtual void Enter() 
    { 
        time = Time.time;
        OnBehaviourStart?.Invoke(this);
        BroadcastActionToClients();
    }
    public abstract void Execute();
    public virtual void Exit()
    {
        OnBehaviourEnd?.Invoke(this);
        Owner.ChangeBehaviour();// move to entity and subscribe to on behaviour end ? 
    }
    public abstract BehaviourData GetBehaviourData();
    public virtual void FillWithData(BehaviourData behaviourData)
    {
        guid = behaviourData.GUID;
    }

    public virtual void SetOwner(Entity entity)
    {
        Owner = entity;

        if (blueprint.Visual != null)
        {
            BehaviourVisual visual = GameObject.Instantiate(blueprint.Visual, Owner.gameObject.transform);
            visual.Initialize(this);
        }
    }

    private void BroadcastActionToClients()
    {
        if (!Server.IsServer || this is not ISerializableAction action)
            return;

        string serializedAction = action.SerializeAction();
        NetGameAction response = new NetGameAction()
        {
            entityGUID = Owner.guid,
            behaviourGUID = guid,
            serializedBehaviour = serializedAction,
        };

        foreach (var player in Owner.Owner.match.players)
        {
            NetworkConnection connection = GameManager.Instance.GetNetworkConnection(player.clientId);
            if (connection != null)
                Sender.ServerSendData(connection, response, Pipeline.Reliable);
        }
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


public class MovementBehaviourAction : BehaviourAction
{
    public Vector2Int EndCoord { get; set; }
}

public class AttackBehaviourAction : BehaviourAction
{
    public string EnemyGUID { get; set; }
    public string DamageableGUID { get; set; }
}




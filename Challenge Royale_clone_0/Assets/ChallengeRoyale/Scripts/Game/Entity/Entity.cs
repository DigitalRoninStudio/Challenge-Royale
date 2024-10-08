using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public abstract class Entity : IDisposable
{
    public string id;
    public string name;
    [JsonIgnore] public GameObject GameObject => gameObject;
    private GameObject gameObject;
    public Visibility visibility;
    public Direction direction;
    [JsonIgnore] public Team Team => owner.team;
    [JsonIgnore] public Map Map => owner.match.map;
    public bool isBlockingMovement;

    public Action<Behaviour> OnBehaviourStart;
    public Action<Behaviour> OnBehaviourEnd;

    public List<Behaviour> Behaviours => behaviours;
    protected List<Behaviour> behaviours;
    protected Queue<Behaviour> pendingBehaviours;

    public Player owner;

    public Entity()
    {
        gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);//delete
        behaviours = new List<Behaviour>();
        pendingBehaviours = new Queue<Behaviour>();
        OnBehaviourStart += BehaviourStart;
    }

    public void Update()
    {
        if (pendingBehaviours.Count > 0)
            pendingBehaviours.Peek().Execute();
    }
    public void SetOwner(Player player) { owner = player; }
    public void ResetDirection()
    {
        if (Team == Team.GOOD_BOYS) direction = Direction.UP;
        else if (Team == Team.BAD_BOYS) direction = Direction.DOWN;
    }

    private void BehaviourStart(Behaviour behaviour)
    {
        if (Server.IsServer)
        {
            string serializedBehaviour = behaviour.Serialize();

            NetGameAction responess = new NetGameAction()
            {
                entityId = id,
                behaviourDataId = behaviour.id,
                serializedBehaviour = serializedBehaviour,
            };

            foreach (var player in owner.match.players)
            {
                NetworkConnection connection = GameManager.Instance.GetConnection(player);
                if (connection != null)
                    Sender.ServerSendData(connection, responess, Pipeline.Reliable);

            }
        }
    }
    public void AddBehaviourToWork(Behaviour behaviour)
    {
        if (behaviour != null)
        {
            if (pendingBehaviours.Count == 0)
            {
                pendingBehaviours.Enqueue(behaviour);
                behaviour.Enter();
            }
            else
                pendingBehaviours.Enqueue(behaviour);
        }
    }
    public void ChangeBehaviour()
    {
        if (pendingBehaviours.Count > 0)
            pendingBehaviours.Dequeue();

        if (pendingBehaviours.Count > 0)
            pendingBehaviours.Peek().Enter();
    }

    public void Dispose()
    {
        OnBehaviourStart -= BehaviourStart;
    }
}




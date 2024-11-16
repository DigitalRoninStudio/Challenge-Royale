using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Entity : IDisposable
{
    public string guid;
    public GameObject GameObject => gameObject;
    public GameObject gameObject;
    public Visibility visibility;
    public Direction direction;
    public Team Team => Owner.team;

    public bool isBlockingMovement;


    public List<Behaviour> Behaviours => behaviours;
    protected List<Behaviour> behaviours;
    protected Queue<Behaviour> pendingBehaviours;

    public StatusEffectController StatusEffectController => statusEffectController;
    private StatusEffectController statusEffectController;

    public EntityBlueprint EntityBlueprint { get; private set; }
    public Player Owner { get; private set; }

    public Entity()
    {
        behaviours = new List<Behaviour>();
        pendingBehaviours = new Queue<Behaviour>();
        statusEffectController = new StatusEffectController();
    }

    public Entity(EntityBlueprint blueprint)
    {
        behaviours = new List<Behaviour>();
        pendingBehaviours = new Queue<Behaviour>();
        statusEffectController = new StatusEffectController();

        guid = Guid.NewGuid().ToString();
        EntityBlueprint = blueprint;
        isBlockingMovement = blueprint.IsBlockingMovement;
        visibility = Visibility.BOTH;
        gameObject = GameObject.Instantiate(blueprint.GameObject);

        foreach (var behaviourData in blueprint.BehaviourDatas)
        {
            Behaviour behaviour = behaviourData.CreateBehaviour();
            AddBehaviour(behaviour);
        }

    }

    public void Update()
    {
        if (pendingBehaviours.Count > 0)
            pendingBehaviours.Peek().Execute();
    }
    public void SetOwner(Player player) => Owner = player; 
    public void ResetDirection()
    {
        if (Team == Team.GOOD_BOYS) direction = Direction.UP;
        else if (Team == Team.BAD_BOYS) direction = Direction.DOWN;
    }
    public void AddBehaviour(Behaviour behaviour)
    {
        behaviours.Add(behaviour);
        behaviour.SetOwner(this);
    }
    public T GetBehaviour<T>() where T : Behaviour
    {
        foreach (Behaviour behaviour in behaviours)
            if (behaviour is T b)
                return b;
        return null;
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
    public abstract EntityData GetEntityData();


    public void SetRotation()
    {
        gameObject.transform.eulerAngles = new Vector3(0, 0, Map.directionToRotation[direction]);
    }

    public virtual void FillWithData(EntityData entityData)
    {
        guid = entityData.GUID;
        visibility = entityData.Visibility;
        direction = entityData.Direction;
        SetRotation();
        SetUpBehaviours(entityData);
        SetUpStatusEffect(entityData.StatusEffectDatas);
    }

    public virtual void Dispose()
    {
        GameObject.Destroy(gameObject);
    }
    // HOT FIX
    private void SetUpBehaviours(EntityData entityData)
    {
        HashSet<string> behaviourDataIds = entityData.BehaviourDatas.Select(bd => bd.Id).ToHashSet();
        List<Behaviour> behavioursToDispose = behaviours.Where(b => !behaviourDataIds.Contains(b.BehaviourBlueprint.Id)).ToList();
        foreach (Behaviour behaviour in behavioursToDispose)
        {
            if(behaviour is IDisposable disposable)
                disposable.Dispose();

            behaviours.Remove(behaviour);
        }

        Dictionary<string, Behaviour> behaviourDict = behaviours.ToDictionary(b => b.BehaviourBlueprint.Id);

        GameFactory gameFactory = new GameFactory();
        foreach (BehaviourData behaviourData in entityData.BehaviourDatas)
        {
            if (behaviourDict.TryGetValue(behaviourData.Id, out Behaviour behaviour))
                behaviour.FillWithData(behaviourData);
            else
            {
                behaviour = gameFactory.CreateBehaviour(behaviourData);
                AddBehaviour(behaviour);
            }
        }
    }

    private void SetUpStatusEffect(List<StatusEffectData> statusEffectDatas)
    {
        GameFactory gameFactory = new GameFactory();
        foreach (var statusEffectData in statusEffectDatas)
        {
           StatusEffect statusEffect = gameFactory.CreateStatusEffect(statusEffectData);
           statusEffectController.AddStatusEffect(statusEffect); 
        }
    }
}




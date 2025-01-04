using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Entity : IDisposable
{
    public string guid;
    public GameObject GameObject => gameObject;
    public GameObject gameObject;
    public float rotation;
    public Visibility visibility;
   // public Direction direction;
    public Team Team => Owner.team;

    public bool isBlockingMovement;


    public List<Behaviour> Behaviours => behaviours;
    protected List<Behaviour> behaviours;

    public StatusEffectController StatusEffectController => statusEffectController;
    private StatusEffectController statusEffectController;

    public EntityBlueprint EntityBlueprint { get; private set; }
    public Player Owner { get; private set; }
    protected Entity()
    {
        behaviours = new List<Behaviour>();
        statusEffectController = new StatusEffectController();
    }

    #region Builder
    public class Builder<T, TB, TD>
       where T : Entity, new()
       where TB : EntityBlueprint
       where TD : EntityData
    {
        protected readonly T _entity;

        public Builder()
        {
            _entity = new T();
        }
        public virtual Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            _entity.EntityBlueprint = blueprint;
            _entity.isBlockingMovement = blueprint.IsBlockingMovement;
            _entity.visibility = Visibility.BOTH;
            _entity.gameObject = GameObject.Instantiate(blueprint.GameObject);
            return this;
        }
        public virtual Builder<T, TB, TD> WithBehaviours(TB blueprint)
        {
            foreach (var behaviourBlueprint in blueprint.BehaviourDatas)
            {
                Behaviour behaviour = behaviourBlueprint.CreateBehaviour(_entity);
                _entity.AddBehaviour(behaviour);
            }    

            return this;
        }
        public Builder<T, TB, TD> WithGeneratedId()
        {
            _entity.guid = Guid.NewGuid().ToString();
            return this;
        }
        public Builder<T, TB, TD> WithSyncGeneratedId(string guid)
        {
            _entity.guid = guid;
            return this;
        }

        public virtual Builder<T, TB, TD> WithData(TD entityData)
        {
            _entity.guid = entityData.GUID;
            _entity.visibility = entityData.Visibility;
            return this;
        }

        public T Build() => _entity;
    }
    #endregion

    public void SetOwner(Player player) => Owner = player;
   /* public void ResetDirection()
    {
        if (Team == Team.GOOD_BOYS) direction = Direction.UP;
        else if (Team == Team.BAD_BOYS) direction = Direction.DOWN;
    }*/
    public void AddBehaviour(Behaviour behaviour)
    {
        behaviours.Add(behaviour);
        behaviour.SetOwner(this);
    }
    public bool TryGetBehaviour<T>(out T behaviour) where T : Behaviour
    {
        behaviour = behaviours.OfType<T>().FirstOrDefault();
        return behaviour != null;
    }
    public bool HasBehaviour<T>() where T : Behaviour
    {
        return behaviours.Exists(b => b is T);
    }
    public T GetBehaviour<T>() where T : Behaviour
    {
        return behaviours.OfType<T>().FirstOrDefault();
    }
    public abstract EntityData GetEntityData();

    public virtual void Dispose()
    {
        GameObject.Destroy(gameObject);
    }
}




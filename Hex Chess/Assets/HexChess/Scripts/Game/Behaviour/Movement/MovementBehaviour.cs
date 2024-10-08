using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class MovementBehaviour : Behaviour, ISerializableAction
{
    public float speed;
    public int range;

    protected Queue<Tile> path;
    protected Tile currentTile;

    private Tile nextTile;
    private Vector3 direction;
    private Queue<float> steps;
    public MovementBehaviour() : base()
    {
        path = new Queue<Tile>();
        steps = new Queue<float>();
    }
    public MovementBehaviour(MovementBehaviourBlueprint blueprint) : base(blueprint)
    {
        path = new Queue<Tile>();
        steps = new Queue<float>();
        speed = blueprint.Speed;
        range = blueprint.Range;
    }

    public override void Execute()
    {
        if (nextTile == null && path.Count == 0)
        {
            currentTile = null;
            Exit();
            return;
        }

        if (nextTile == null && path.Count > 0)
        {
            nextTile = path.Dequeue();

            direction = (nextTile.GetPosition() - owner.GameObject.transform.position).normalized;
            float movePerFrame = speed * Time.fixedDeltaTime;
            float distance = Vector3.Distance(nextTile.GetPosition(), owner.GameObject.transform.position);
            int numberOfSteps = Mathf.FloorToInt(distance / movePerFrame);

            for (int i = 0; i < numberOfSteps; i++)
                steps.Enqueue(movePerFrame);
        }

        if (steps.Count == 0)
        {
            currentTile.RemoveEntity(owner);
            currentTile = nextTile;
            currentTile.AddEntity(owner);
            nextTile = null; 
        }
        else
            owner.GameObject.transform.position += direction * steps.Dequeue();

    }

    public abstract List<Tile> GetAvailableMoves(Tile tile);

    public virtual void SetPath(Tile start, Tile end)
    {
        currentTile = start;
    }
    public string SerializeAction()
    {
        MovementBehaviourAction movementAction = new MovementBehaviourAction
        {
            StartCoord = path.Peek().coordinate,
            EndCoord = path.Last().coordinate,
        };

        return JsonConvert.SerializeObject(movementAction);
    }
    public void DeserializeAction(string data)
    {
        MovementBehaviourAction movementAction = JsonConvert.DeserializeObject<MovementBehaviourAction>(data);

        Tile start = Map.GetTile(movementAction.StartCoord);
        Tile end = Map.GetTile(movementAction.EndCoord);
        SetPath(start, end);
    }

    public class MovementBehaviourAction : BehaviourAction
    {
        public Vector2Int StartCoord { get; set; }
        public Vector2Int EndCoord { get; set; }
    }
}
public abstract class AttackBehaviour : Behaviour
{
    protected int baseDamage;
    protected int attackRange;
    protected DamageType damageType;
    protected float timeToPerformAttack;

    public virtual int AttackDamage => baseDamage;
    public virtual int AttackRange => attackRange;
    public virtual float TimeToPerformAttack => timeToPerformAttack;

    protected DamageableBehaviour target;

    public Action<DamageableBehaviour> OnAttackPerformed;

    public AttackBehaviour() : base() { }
    public AttackBehaviour(AttackBehaviourBlueprint blueprint) : base(blueprint)
    {
        baseDamage = blueprint.BaseDamage;
        attackRange = blueprint.AttackRange;
        damageType = blueprint.DamageType;
        timeToPerformAttack = blueprint.TimeToPerformAttack;
    }
    public virtual void SetAttack(DamageableBehaviour target)
    {
        this.target = target;
    }
    public override void Execute()
    {
        if (Time.time >= time + TimeToPerformAttack)
        {
            target.ReceiveDamage(CreateDamage());
            OnAttackPerformed?.Invoke(target);
            Exit();
        }
    }
    public virtual List<Tile> GetAttackMoves(Tile tile)
    {
        List<Tile> attackMoves = new List<Tile>();

        List<Tile> tiles = Map.TilesInRange(tile, attackRange);

        foreach (var t in tiles)
        {
            List<Entity> entities = t.GetEntities();
            foreach (var enemy in entities)
                if (IsValidEnemyTarget(enemy))
                {
                    attackMoves.Add(t);
                    break;
                }
        }

        return attackMoves;
    }
    protected abstract Damage CreateDamage();

    public bool CanAttack(Entity enemy)
    {
        Tile entityTile = Map.GetTile(owner);
        Tile targetTile = Map.GetTile(enemy);

        if (entityTile == null || targetTile == null) return false;

        List<Tile> tiles = Map.TilesInRange(entityTile, attackRange);

        if(tiles.Contains(targetTile))
        {
            List<Entity> entities = targetTile.GetEntities();
            foreach (Entity e in entities) 
            {
                if(IsValidEnemyTarget(e))
                    return true;
            }
        }
        return false;
    }
    protected bool IsValidEnemyTarget(Entity enemy)
    {
        return enemy.Owner.team != owner.Owner.team &&
          enemy.GetBehaviour<DamageableBehaviour>() != null;
    }
}

public class DamageableBehaviour : Behaviour
{
    protected int currentHealth;
    protected int maxHealth;

    public DamageableBehaviour() : base() { }
    public DamageableBehaviour(DamageableBlueprint blueprint) : base(blueprint)
    {
        maxHealth = blueprint.MaxHealth;
        currentHealth = maxHealth;
    }

    public virtual int CurrentHealth => currentHealth;
    public virtual int MaxHealth => maxHealth;
    public virtual bool IsAlive => currentHealth > 0;
    public virtual bool CanReceiveDamage => IsAlive;

    public event Action<Damage> OnDamageReceived;
    public event Action OnDeath;

    public override void Execute()
    {
        Exit();
    }
    public override void FillWithData(BehaviourData behaviourData)
    {
        base.FillWithData(behaviourData);
        if(behaviourData is DamageableBehaviourData damageableData)
        {
            currentHealth = damageableData.CurrentHealth;
        }
    }
    public virtual void ReceiveDamage(Damage damage)
    {
        if (!CanReceiveDamage) return;

        int finalDamage = CalculateDamage(damage);
        currentHealth = Math.Max(0, currentHealth - finalDamage);
        OnDamageReceived?.Invoke(damage);

        if (!IsAlive)
        {
            OnDeath?.Invoke();
        }
    }

    protected virtual int CalculateDamage(Damage damage)
    {
        return damage.Amount;
    }
    public override BehaviourData GetBehaviourData() => new DamageableBehaviourData(this);
}

public class MeleeAttackBehaviour : AttackBehaviour
{
    public MeleeAttackBehaviour() : base() { }
    public MeleeAttackBehaviour(MeleeAttackBlueprint blueprint) : base(blueprint) { }
    public override BehaviourData GetBehaviourData() => new MeleeAttackData(this);
    protected override Damage CreateDamage() => new Damage(AttackDamage, DamageType.Physical);
}

public class RangedAttackBehaviour : AttackBehaviour
{
    public RangedAttackBehaviour() : base() { }
    public RangedAttackBehaviour(RangedAttackBlueprint blueprint) : base(blueprint) { }
    public override BehaviourData GetBehaviourData() => new RangedAttackData(this);
    protected override Damage CreateDamage() => new Damage(AttackDamage, DamageType.Physical);
}

public class Damage
{
    public int Amount { get; }
    public DamageType Type { get; }

    public Damage(int amount, DamageType type)
    {
        Amount = amount;
        Type = type;
    }
}

public enum DamageType
{
    Physical,
    Magic,
    Pure,
}







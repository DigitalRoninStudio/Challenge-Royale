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

    public Action<Tile, Tile> OnTileExit;
    public Action<Tile> OnTileEntered;

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

    public override void SetOwner(Entity entity)
    {
        base.SetOwner(entity);
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

            direction = (nextTile.GetPosition() - Owner.GameObject.transform.position).normalized;
            float movePerFrame = speed * Time.fixedDeltaTime;
            float distance = Vector3.Distance(nextTile.GetPosition(), Owner.GameObject.transform.position);
            int numberOfSteps = Mathf.FloorToInt(distance / movePerFrame);

            for (int i = 0; i < numberOfSteps; i++)
                steps.Enqueue(movePerFrame);

            OnTileExit?.Invoke(currentTile, nextTile);
        }

        if (steps.Count == 0)
        {
            currentTile.RemoveEntity(Owner);
            currentTile = nextTile;
            currentTile.AddEntity(Owner);
            nextTile = null;

            OnTileEntered?.Invoke(currentTile);
        }
        else
            Owner.GameObject.transform.position += direction * steps.Dequeue();

    }

    public abstract List<Tile> GetAvailableMoves();

    public bool CanMove(Tile tile)
    {
        if (Owner.StatusEffectController.HasStatusEffect<Stun>())
        {
            Debug.Log("CANNOT PERFORM ATTACK, ENTITY IS STUNED");
            return false;
        }
        else if (Owner.StatusEffectController.HasStatusEffect<Root>())
        {
            Debug.Log("CANNOT PERFORM ATTACK, ENTITY IS ROOTED");
            return false;
        }

        if (tile == null) return false;

        if(!GetAvailableMoves().Contains(tile)) return false;
               
        return true;
    }

    public virtual void SetPath(Tile end)
    {
        currentTile = Map.GetTile(Owner);
    }
    public string SerializeAction()
    {
        MovementBehaviourAction movementAction = new MovementBehaviourAction
        {
            EndCoord = path.Last().coordinate,
        };

        return JsonConvert.SerializeObject(movementAction);
    }
    public void DeserializeAction(string data)
    {
        MovementBehaviourAction movementAction = JsonConvert.DeserializeObject<MovementBehaviourAction>(data);

        Tile end = Map.GetTile(movementAction.EndCoord);
        SetPath(end);
    }
}
public abstract class AttackBehaviour : Behaviour, ISerializableAction
{
    protected int baseDamage;
    protected int attackRange;
    protected DamageType damageType;
    protected float timeToPerformAttack;

    public virtual int AttackDamage => baseDamage;
    public virtual int AttackRange => attackRange;
    public virtual float TimeToPerformAttack => timeToPerformAttack;

    protected DamageableBehaviour target;

    public Action<DamageableBehaviour, Damage> OnAttackPerformed;

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
            Damage damage = CreateDamage();
            target.ReceiveDamage(damage);
            OnAttackPerformed?.Invoke(target, damage);
            Exit();
        }
    }
    public virtual List<Tile> GetAttackMoves()
    {
        List<Tile> attackMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return attackMoves;

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
        if(Owner.StatusEffectController.HasStatusEffect<Stun>())
        {
            Debug.Log("CANNOT PERFORM ATTACK, ENTITY IS STUNED");
            return false;
        }else if(Owner.StatusEffectController.HasStatusEffect<Disarm>())
        {
            Debug.Log("CANNOT PERFORM ATTACK, ENTITY IS DISARMED");
            return false;
        }

        Tile targetTile = Map.GetTile(enemy);

        if (targetTile == null) return false;

        if(!GetAttackMoves().Contains(targetTile)) return false;       
        
        return true;
    }
    protected bool IsValidEnemyTarget(Entity enemy)
    {
        return enemy.Owner.team != Owner.Owner.team &&
          enemy.GetBehaviour<DamageableBehaviour>() != null;
    }

    public string SerializeAction()
    {
        AttackBehaviourAction attackAction = new AttackBehaviourAction
        {
            EnemyGUID = target.Owner.guid,
            DamageableGUID = target.guid
        };

        return JsonConvert.SerializeObject(attackAction);
    }
    public void DeserializeAction(string data)
    {
        var attackAction = JsonConvert.DeserializeObject<AttackBehaviourAction>(data);

        var damageable = Owner.Owner.match.GetAllEntities()
            .FirstOrDefault(e => e.guid == attackAction.EnemyGUID)?
            .Behaviours
            .FirstOrDefault(b => b.guid == attackAction.DamageableGUID) as DamageableBehaviour;

        if (damageable != null)
            SetAttack(damageable);
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

    public Action<int,int> OnDamageReceived;
    public Action OnDeath;

    public override void Execute() => Exit();
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

        OnDamageReceived?.Invoke(currentHealth, finalDamage);

        if (!IsAlive)
        {
            OnDeath?.Invoke();
            Map.GetTile(Owner).RemoveEntity(Owner);
            Owner.gameObject.SetActive(false); // ?
        }
    }

    protected virtual int CalculateDamage(Damage damage)
    {
        if(Owner.StatusEffectController.HasStatusEffect<DamageImmune>())
        {
            List<DamageImmune> damageImmunes = Owner.StatusEffectController.GetStatusEffectsOfType<DamageImmune>();
            foreach (var damageImmune in damageImmunes)
            {
                if ((damageImmune.DamageType & damage.Type) != 0)
                {
                    damage.SetAmount(0);
                    return damage.Amount;
                }
            }
        }

        if(Owner.StatusEffectController.HasStatusEffect<Shield>())
        {
            List<Shield> shields = Owner.StatusEffectController.GetStatusEffectsOfType<Shield>();
            for (int i = shields.Count - 1; i >= 0; i--)
            {
                var shield = shields[i];
                if ((shield.DamageType & damage.Type) != 0)
                {
                    shield.ReceiveDamage(damage);

                    if (!shield.IsShieldActive())
                    {
                        Owner.StatusEffectController.RemoveStatusEffect(shield);
                        shields.RemoveAt(i); 
                    }

                    if (damage.Amount == 0)
                        break;
                }
            }
        }

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
    public int Amount { get; private set; }
    public DamageType Type { get; private set; }

    public Damage(int amount, DamageType type)
    {
        Amount = amount;
        Type = type;
    }

    public void SetAmount(int amount)
    {
        Amount = amount;
    }
}

[Flags]
public enum DamageType
{
    Physical = 0,
    Magic = 1 << 0,
    True = 1 << 1
}
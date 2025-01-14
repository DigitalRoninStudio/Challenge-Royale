using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEngine;

public abstract class MovementBehaviour : Behaviour, ILifecycleAction ,INetAction, ITileSelection
{
    public float speed;
    public int range;
    public Direction direction;
    

    public Action<Tile, Tile> OnTileExit;
    public Action<Tile> OnTileEntered;
    public Tile Destination => path.Count > 0 ? path.Last() : null;

    protected Queue<Tile> path;
    protected Tile currentTile;

    private Tile nextTile;
    private Vector3 lookDirection;
    private Queue<float> steps;
    public ActionType ActionType => ActionType.BEHAVIOUR;

    public Action OnActionStart { get; set; } = () => { };
    public Action OnActionExecuted { get; set; } = () => { };
    public Action OnActionEnd { get; set; } = () => { };
    protected MovementBehaviour() : base()
    {
        path = new Queue<Tile>();
        steps = new Queue<float>();
    }
    #region Builder
    public new abstract class Builder<T, TB, TD> : Behaviour.Builder<T, TB, TD>
        where T : MovementBehaviour
        where TB : MovementBehaviourBlueprint
        where TD : MovementBehaviourData
    {
        public Builder(T behaviour, Entity owner) : base(behaviour, owner) { }

        public new Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            base.WithBlueprint(blueprint);
            _behaviour.speed = blueprint.Speed;
            _behaviour.range = blueprint.Range;
            return this;
        }
        public new Builder<T, TB, TD> WithData(TD behaviourData)
        {
            base.WithData(behaviourData);
            _behaviour.direction = behaviourData.Direction;

            Vector2Int dir = HexagonMap.directionToCoordinate[behaviourData.Direction];
            Vector3 direction = new Vector3(dir.x, dir.y, 0).normalized;
            _behaviour.Owner.GameObject.transform.GetChild(0).rotation = Quaternion.FromToRotation(Vector3.up, direction);
            return this;
        }
    }
    #endregion

    public override void SetOwner(Entity entity)
    {
        base.SetOwner(entity);
    }

    public virtual void Enter()
    {
        time = Time.time;
        OnActionStart?.Invoke();
    }
    public bool CanBeExecuted()
    {
        if(Owner.TryGetBehaviour<DamageableBehaviour>(out var damageableBehaviour))
            if (!damageableBehaviour.IsAlive)
                return false;

        if (Owner.StatusEffectController.HasStatusEffect<Stun>() || Owner.StatusEffectController.HasStatusEffect<Root>())
            return false;

        return true;
    }
    public virtual void Execute()
    {

        if (nextTile == null && path.Count == 0)
        {
            currentTile = null;
            OnActionExecuted?.Invoke();
            Exit();
            return;
        }

        if (nextTile == null && path.Count > 0)
        {
            if(path.Peek() == currentTile)
            {
                path.Dequeue();
                return;
            }

            nextTile = path.Dequeue();

            var cooridnateDirection = nextTile.coordinate - currentTile.coordinate;
            var unitCoordinate = HexagonMap.TransformCoordinatesToUnitCoordinates(cooridnateDirection);
            direction = HexagonMap.coordinateToDirection[unitCoordinate];

            lookDirection = (nextTile.GetPosition() - Owner.GameObject.transform.position).normalized;
            float movePerFrame = speed * Time.fixedDeltaTime;
            float distance = Vector3.Distance(nextTile.GetPosition(), Owner.GameObject.transform.position);
            int numberOfSteps = Mathf.FloorToInt(distance / movePerFrame);

            for (int i = 0; i < numberOfSteps; i++)
                steps.Enqueue(movePerFrame);

            if (cooridnateDirection != Vector2Int.zero)
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
        {
            Owner.GameObject.transform.position += lookDirection * steps.Dequeue();
            Owner.GameObject.transform.GetChild(0).rotation = Quaternion.FromToRotation(Vector3.up, lookDirection);
        }

    }

    public virtual void Exit()
    {
        OnActionEnd?.Invoke();
    }

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

        if(!GetAvailableTiles().Contains(tile)) return false;
               
        return true;
    }

    public virtual void SetPath(Tile end)
    {
        currentTile = Map.GetTile(Owner);
        nextTile = null;
        path.Clear();
    }

    public virtual int GetEnergyCost(Tile tile) => energyCost;
    public string SerializeAction()
    {
        MovementActionData actionData = new MovementActionData()
        {
            EntityGUID = Owner.guid,
            BehaviourGUID = guid,
            TileCoordinate = new Vector2Data(path.Last().coordinate)
        };

        ExecutedAction executedAction = new ExecutedAction()
        {
            playerGuid = Owner.Owner.clientId,
            actionData = actionData
        };
        Owner.Owner.match.executedClientsActions.Add(executedAction);

        return JsonConvert.SerializeObject(actionData);
    }
    
    public void DeserializeAction(string actionJson)
    {
        MovementActionData actionData = JsonConvert.DeserializeObject<MovementActionData>(actionJson);

        ExecutedAction executedAction = new ExecutedAction()
        {
            playerGuid = Owner.Owner.clientId,
            actionData = actionData
        };
        Owner.Owner.match.executedClientsActions.Add(executedAction);

        Tile end = Map.GetTile(new Vector2Int(actionData.TileCoordinate.X, actionData.TileCoordinate.Y));
        SetPath(end);
    }

    public abstract List<Tile> GetAvailableTiles();
    public abstract List<Tile> GetTiles();
}

public abstract class AbilityBehaviour : Behaviour
{
    protected AbilityBehaviour() : base() { }
    #region Builder
    public new abstract class Builder<T, TB, TD> : Behaviour.Builder<T, TB, TD>
        where T : AbilityBehaviour
        where TB : AbilityBlueprint
        where TD : AbilityBehaviourData
    {
        public Builder(T behaviour, Entity owner) : base(behaviour, owner) { }
    }
    #endregion

}

public abstract class PassiveAbility : AbilityBehaviour
{
    protected PassiveAbility() : base() { }
    #region Builder
    public new abstract class Builder<T, TB, TD> : AbilityBehaviour.Builder<T, TB, TD>
        where T : PassiveAbility
        where TB : PassiveAbilityBlueprint
        where TD : PassiveAbilityBehaviourData
    {
        public Builder(T behaviour, Entity owner) : base(behaviour, owner) { }
    }
    #endregion

    public abstract void Apply();
    public abstract void Remove();

}


public abstract class ActiveAbility : AbilityBehaviour, ILifecycleAction, INetAction, ITileSelection
{
    protected int maxTargets;
    protected int radius;
    protected int maxCooldown;
    protected int currentCooldown;
    public virtual int MaxTargets => maxTargets;
    public virtual int Radius => radius;
    public virtual int MaxCooldown => maxCooldown;
    public virtual int CurrentCooldow => currentCooldown;
    public Action OnActionStart { get; set; } = () => { };
    public Action OnActionExecuted { get; set; } = () => { };
    public Action OnActionEnd { get; set; } = () => { };
    public ActionType ActionType => ActionType.BEHAVIOUR;

    protected ActiveAbility() : base() { }
    #region Builder
    public new abstract class Builder<T, TB, TD> : AbilityBehaviour.Builder<T, TB, TD>
        where T : ActiveAbility
        where TB : ActiveAbilityBlueprint
        where TD : ActiveAbilityBehaviourData
    {
        public Builder(T behaviour, Entity owner) : base(behaviour, owner) { }

        public new Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            base.WithBlueprint(blueprint);
            _behaviour.maxTargets = blueprint.MaxTargets;
            _behaviour.radius = blueprint.Radius;
            _behaviour.maxCooldown = blueprint.MaxCooldown;

            return this;
        }

        public new Builder<T, TB, TD> WithData(TD behaviourData)
        {
            base.WithData(behaviourData);
            _behaviour.currentCooldown = behaviourData.CurrentCooldown;

            return this;
        }


    }
    #endregion

    public virtual void Enter()
    {
        time = Time.time;
        OnActionStart?.Invoke();
    }
    public virtual bool CanBeExecuted() => true;
    public abstract void Execute();
    public virtual void Exit()
    {
        OnActionEnd?.Invoke();
    }

    public abstract List<Tile> GetAvailableTiles();
    public abstract List<Tile> GetTiles();

    public void ResetCooldown() => currentCooldown = maxCooldown;
    public bool IsOnCooldown() => currentCooldown > 0;
    public void TickCooldown() => currentCooldown = Math.Max(0, currentCooldown - 1);
    public virtual bool CanUseAbility()
    {
        if (currentCooldown > 0)
            return false;

        return true;
    }

    public abstract string SerializeAction();
    public abstract void DeserializeAction(string action);
}

public class SwordsmanSpecial : ActiveAbility, IToggleable
{
    public bool IsToogle => toggle;

    private bool toggle = false;

    public DamageModifierBlueprint damageModifierBlueprint;
    public string damageModifierInstanceId = "";
    public HealthModifierBlueprint healthModifierBlueprint;
    public string healthModifierInstanceId = "";
    public DodgeCastAttackBlueprint dodgeCastAttackBlueprint;

    private HashSet<Tile> subscribedTiles = new HashSet<Tile>();


    #region Builder
    public class Builder : Builder<SwordsmanSpecial, SwordsmanSpecialBlueprint, SwordsmanSpecialData>
    {
        public Builder(Entity owner) : base(new SwordsmanSpecial(), owner) { }

        public new Builder WithGeneratedId()
        {
            base.WithGeneratedId();

            // we add a damage modifier to the creation of the swordsman special
            if (_behaviour.Owner.HasBehaviour<AttackBehaviour>())
            {
                DamageModifier damageModifier = _behaviour.damageModifierBlueprint.CreateStatusEffect(_behaviour, _behaviour.Owner) as DamageModifier;
                _behaviour.Owner.StatusEffectController.AddStatusEffect(damageModifier);
                _behaviour.damageModifierInstanceId = damageModifier.guid;
            }

            return this;
        }
        public new Builder WithSyncGeneratedId(string guid)
        {
            base.WithSyncGeneratedId(guid);

            // we add a damage modifier to the creation of the swordsman special
            if (_behaviour.Owner.HasBehaviour<AttackBehaviour>())
            {
                DamageModifier damageModifier = _behaviour.damageModifierBlueprint.CreateStatusEffect(_behaviour.Owner.Owner.match.randomGenerator, _behaviour, _behaviour.Owner) as DamageModifier;
                _behaviour.Owner.StatusEffectController.AddStatusEffect(damageModifier);
                _behaviour.damageModifierInstanceId = damageModifier.guid;
            }

            return this;
        }


        public new Builder WithBlueprint(SwordsmanSpecialBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _behaviour.healthModifierBlueprint = blueprint.HealthBlueprint;
            _behaviour.damageModifierBlueprint = blueprint.DamageBlueprint;
            _behaviour.dodgeCastAttackBlueprint = blueprint.DodgeCastAttackBlueprint;

            return this;
        }

        public new Builder WithData(SwordsmanSpecialData behaviourData)
        {
            base.WithData(behaviourData);
            _behaviour.toggle = behaviourData.Toggle;
            _behaviour.damageModifierInstanceId = behaviourData.DamageModifierInstanceId;
            _behaviour.healthModifierInstanceId = behaviourData.HealthModifierInstanceId;

            return this;
        }

        public Builder WithSubscription()
        {
           // if (Server.IsServer)
                if (_behaviour.Owner.TryGetBehaviour<MovementBehaviour>(out var movementBehaviour))
                {
                    _behaviour.Owner.OnPlaced += _behaviour.OnSwordsmanPlacedOnTile;
                    movementBehaviour.OnTileEntered += _behaviour.OnSwordsmanEnterTile;
                    movementBehaviour.OnTileExit += _behaviour.OnSwordsmanExitTile;
                }

            return this;
        }
    }

    #endregion

    public override void Execute()
    {
        Toggle();
        Exit();
    }

    public override List<Tile> GetAvailableTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return availableMoves;

        availableMoves.Add(tile);

        return availableMoves;
    }

    public override BehaviourData GetBehaviourData() => new SwordsmanSpecialData(this);

    public override List<Tile> GetTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return availableMoves;

        availableMoves.Add(tile);

        return availableMoves;
    }

    public override string SerializeAction()
    {
        SwordsmanSpecialActionData actionData = new SwordsmanSpecialActionData()
        {
            EntityGUID = Owner.guid,
            BehaviourGUID = guid,
        };
        return JsonConvert.SerializeObject(actionData); //HOTFIX
    }
    public override void DeserializeAction(string actionJson)
    {
    }
    public bool CanToggle()
    {
        if(Owner.StatusEffectController.HasStatusEffect<Stun>())
            return false;

        return true;
    }

    public void Toggle()
    {
        if(toggle)
            Deactivated();
        else
            Activated();
    }

    public void Activated()
    {
        //Remove Damage Modifier
        if(!string.IsNullOrEmpty(damageModifierInstanceId))
        {
            var damageModifier = Owner.StatusEffectController
               .GetStatusEffectsOfType<DamageModifier>()
               .FirstOrDefault(d => d.guid == damageModifierInstanceId);

            if (damageModifier != null)
                Owner.StatusEffectController.RemoveStatusEffect(damageModifier);
        }

        //Add Health Modifier
        if(Owner.HasBehaviour<DamageableBehaviour>())
        {
            HealthModifier healthModifier = healthModifierBlueprint.CreateStatusEffect(Owner.Owner.match.randomGenerator, this, Owner) as HealthModifier;
            Owner.StatusEffectController.AddStatusEffect(healthModifier);
            healthModifierInstanceId = healthModifier.guid;
        }

        if (Owner.TryGetBehaviour<MovementBehaviour>(out var movementBehaviour))
        {
            Tile tile = Map.GetTile(Owner);
            if (tile != null)
            {
                var unitDirection = Map.DirectionToCoordinate(movementBehaviour.direction);
                SubscribeToTiles(tile, unitDirection);
            }
        }

        DodgeCastAttack dodgeCastAttack = dodgeCastAttackBlueprint.CreateStatusEffect(Owner.Owner.match.randomGenerator, this, Owner) as DodgeCastAttack;
        Owner.StatusEffectController.AddStatusEffect(dodgeCastAttack);

        toggle = true;
    }

    public void Deactivated()
    {
        //Remove Health Modifier
        if (!string.IsNullOrEmpty(healthModifierInstanceId))
        {
            var healthModifier = Owner.StatusEffectController
               .GetStatusEffectsOfType<HealthModifier>()
               .FirstOrDefault(d => d.guid == healthModifierInstanceId);

            if (healthModifier != null)
                Owner.StatusEffectController.RemoveStatusEffect(healthModifier);
        }

        //Add Damage Modifier
        if (Owner.HasBehaviour<AttackBehaviour>())
        {
            DamageModifier damageModifier = damageModifierBlueprint.CreateStatusEffect(Owner.Owner.match.randomGenerator, this, Owner) as DamageModifier;
            Owner.StatusEffectController.AddStatusEffect(damageModifier);
            damageModifierInstanceId = damageModifier.guid;
        }

        UnsubscribeFromTiles();

        if (Owner.StatusEffectController.TryGetStatusEffect<DodgeCastAttack>(out var dodgeCastAttack))
            Owner.StatusEffectController.RemoveStatusEffect(dodgeCastAttack);

        toggle = false;
    }


    private void OnSwordsmanEnterTile(Tile tile)
    {
        if (toggle) // defense
        {
            // subs on 3 facing fields
            if (Owner.TryGetBehaviour<MovementBehaviour>(out var movementBehaviour))
            {
                var unitDirection = Map.DirectionToCoordinate(movementBehaviour.direction);
                SubscribeToTiles(tile, unitDirection);
            }
        }
        else // attack
        {
            // try to attack face field
            if (Server.IsServer && Owner.TryGetBehaviour<MovementBehaviour>(out var movementBehaviour))
            {
                if (HexagonMap.directionToCoordinate.TryGetValue(movementBehaviour.direction, out var unitCoordinate))
                {
                    Tile faceTile = Map.GetTile(tile.coordinate + unitCoordinate);
                    if(faceTile != null)
                    {
                        foreach (var entity in faceTile.GetEntities())
                        {
                            if(entity.Team != Owner.Team && 
                                Owner.TryGetBehaviour<AttackBehaviour>(out var attackBehaviour) && 
                                entity.TryGetBehaviour<DamageableBehaviour>(out var damageableBehaviour))
                            {
                                attackBehaviour.SetAttack(damageableBehaviour);
                                Owner.Owner.match.actionController.AddActionToWork(attackBehaviour);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnSwordsmanPlacedOnTile(Tile tile)
    {
        if(toggle)
        {
            if (Owner.TryGetBehaviour<MovementBehaviour>(out var movementBehaviour))
            {
                var unitDirection = Map.DirectionToCoordinate(movementBehaviour.direction);
                SubscribeToTiles(tile, unitDirection);
            }
        }
    }

    private void OnFacedTileBeenOccupied(Entity entity)
    {
        if (entity.Team != Owner.Team &&
            Owner.TryGetBehaviour<AttackBehaviour>(out var attackBehaviour) &&
            entity.TryGetBehaviour<DamageableBehaviour>(out var damageableBehaviour))
        {
            if(Server.IsServer)
            {
                attackBehaviour.SetAttack(damageableBehaviour);
                Owner.Owner.match.actionController.AddActionToWork(attackBehaviour);
            }

            if (entity.TryGetBehaviour<MovementBehaviour>(out var enemyMovementBehaviour))
            {
                Tile destination = enemyMovementBehaviour.Destination;
               
                if (destination != null)
                {
                    enemyMovementBehaviour.Exit(); 
                    if (Server.IsServer)
                    {
                        enemyMovementBehaviour.SetPath(destination);
                        entity.Owner.match.actionController.AddActionToWork(enemyMovementBehaviour);
                    }
                }
            }
        }
    }


    private void OnSwordsmanExitTile(Tile previous, Tile current)
    {
        UnsubscribeFromTiles();
    }

    private void SubscribeToTiles(Tile tile, Vector2Int unitCoordinate)
    {
        var directionCoordinate = Map.CoordinateToDirection(unitCoordinate);
        (Vector2Int left, Vector2Int right) leftAndRightUnitCoordinate = HexagonMap.GetLeftAndRightCoordinate(directionCoordinate);

        SubscribeToTile(Map.GetTile(tile.coordinate + unitCoordinate));
        SubscribeToTile(Map.GetTile(tile.coordinate + leftAndRightUnitCoordinate.left));
        SubscribeToTile(Map.GetTile(tile.coordinate + leftAndRightUnitCoordinate.right));
    }


    private void SubscribeToTile(Tile tile)
    {
        if (tile != null && !subscribedTiles.Contains(tile))
        {
            tile.OnOccupied += OnFacedTileBeenOccupied;
            subscribedTiles.Add(tile);
        }
    }

    private void UnsubscribeFromTiles()
    {
        foreach (var tile in subscribedTiles)
            tile.OnOccupied -= OnFacedTileBeenOccupied;

        subscribedTiles.Clear();
    }

}

public interface IToggleable
{
    bool IsToogle { get; }
    bool CanToggle();
    void Toggle();
    void Activated();
    void Deactivated();
}

public abstract class AttackBehaviour : Behaviour, INetAction, ITileSelection, ILifecycleAction
{
    protected int baseDamage;
    protected int attackRange;
    protected DamageType damageType;
    protected float timeToPerformAttack;

    public virtual int AttackDamage => baseDamage;
    public virtual int AttackRange => attackRange;
    public virtual float TimeToPerformAttack => timeToPerformAttack;
    public ActionType ActionType => ActionType.BEHAVIOUR;

    public Action OnActionStart { get; set; } = () => { };
    public Action OnActionExecuted { get; set; } = () => { };
    public Action OnActionEnd { get; set; } = () => { };


    protected DamageableBehaviour target;

    public Action<DamageableBehaviour, Damage> OnAttackPerformed;

    protected AttackBehaviour() : base() { }
    #region Builder
    public new abstract class Builder<T, TB, TD> : Behaviour.Builder<T, TB, TD>
        where T : AttackBehaviour
        where TB : AttackBehaviourBlueprint
        where TD : AttackBehaviourData
    {
        public Builder(T behaviour, Entity owner) : base(behaviour, owner) { }

        public new Builder<T, TB, TD> WithBlueprint(TB blueprint)
        {
            base.WithBlueprint(blueprint);
            _behaviour.baseDamage = blueprint.BaseDamage;
            _behaviour.attackRange = blueprint.AttackRange;
            _behaviour.damageType = blueprint.DamageType;
            _behaviour.timeToPerformAttack = blueprint.TimeToPerformAttack;
            return this;
        }

        public new Builder<T, TB, TD> WithData(TD behaviourData)
        {
            base.WithData(behaviourData);
            _behaviour.baseDamage = behaviourData.BaseDamage;
            return this;
        }
    }
    #endregion
    public virtual void SetAttack(DamageableBehaviour target)
    {
        this.target = target;
    }


    public virtual void Enter()
    {
        time = Time.time;
        OnActionStart?.Invoke();
    }
    public bool CanBeExecuted()
    {
        if (Owner.TryGetBehaviour<DamageableBehaviour>(out var damageableBehaviour))
            if (!damageableBehaviour.IsAlive)
                return false;

        if (Owner.StatusEffectController.HasStatusEffect<Stun>() || Owner.StatusEffectController.HasStatusEffect<Disarm>())
            return false;

        return true;
    }
    public virtual void Execute()
    {
        if (Time.time >= time + TimeToPerformAttack)
        {
            Damage damage = CreateDamage();
            target.ReceiveDamage(damage);
            OnAttackPerformed?.Invoke(target, damage);
            OnActionExecuted?.Invoke();
            Exit();
        }
    }
    public virtual void Exit()
    {
        OnActionEnd?.Invoke();
    }
    public virtual List<Tile> GetAvailableTiles()
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
    public virtual List<Tile> GetTiles()
    {

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return new List<Tile>();

        return Map.TilesInRange(tile, attackRange);
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

        if(!GetAvailableTiles().Contains(targetTile)) return false;       
        
        return true;
    }
    protected bool IsValidEnemyTarget(Entity enemy)
    {
        return enemy.Owner.team != Owner.Owner.team &&
          enemy.GetBehaviour<DamageableBehaviour>() != null;
    }
    public virtual int GetEnergyCost(Entity enemy) => energyCost;
    public string SerializeAction()
    {
        AttackActionData actionData = new AttackActionData()
        {
            EntityGUID = Owner.guid,
            BehaviourGUID = guid,
            EnemyGUID = target.Owner.guid,
            DamageableGUID = target.guid
        };

        ExecutedAction executedAction = new ExecutedAction()
        {
            playerGuid = Owner.Owner.clientId,
            actionData = actionData
        };
        Owner.Owner.match.executedClientsActions.Add(executedAction);

        return JsonConvert.SerializeObject(actionData);
    }
    public void DeserializeAction(string actionJson)
    {
        AttackActionData actionData = JsonConvert.DeserializeObject<AttackActionData>(actionJson);

        ExecutedAction executedAction = new ExecutedAction()
        {
            playerGuid = Owner.Owner.clientId,
            actionData = actionData
        };
        Owner.Owner.match.executedClientsActions.Add(executedAction);

        var damageable = Owner.Owner.match.GetAllEntities()
            .FirstOrDefault(e => e.guid == actionData.EnemyGUID)?
            .Behaviours
            .FirstOrDefault(b => b.guid == actionData.DamageableGUID) as DamageableBehaviour;

        if (damageable != null)
            SetAttack(damageable);
    }

    public void ModifyDamage(int value)
    {
        baseDamage += value;
    }
}

public class DamageableBehaviour : Behaviour
{
    protected int currentHealth;
    protected int maxHealth;

    protected DamageableBehaviour() : base() { }
    #region Builder
    public class Builder : Builder<DamageableBehaviour, DamageableBlueprint, DamageableBehaviourData>
    {
        public Builder(Entity owner) : base(new DamageableBehaviour(), owner) { }
        public new Builder WithBlueprint(DamageableBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _behaviour.maxHealth = blueprint.MaxHealth;
            _behaviour.currentHealth = blueprint.MaxHealth;

            return this;
        }
        public new Builder WithData(DamageableBehaviourData behaviourData)
        {
            base.WithData(behaviourData);
            _behaviour.maxHealth = behaviourData.MaxHealth;
            _behaviour.currentHealth = behaviourData.CurrentHealth;

            return this;
        }
    }
    #endregion

    public virtual int CurrentHealth => currentHealth;
    public virtual int MaxHealth => maxHealth;
    public virtual bool IsAlive => currentHealth > 0;
    public virtual bool CanReceiveDamage => IsAlive;

    public Action<int,int> OnDamageReceived;
    public Action OnDeath;
    public virtual async void ReceiveDamage(Damage damage, bool damageReturn = false)
    {
        if (!CanReceiveDamage) return;

        if (Owner.StatusEffectController.TryGetStatusEffect<DodgeCastAttack>(out var dodgeCastAttack))
        {
            bool dodged = await dodgeCastAttack.TryToDodge(Owner.Owner.match);
            if (dodged) return;
        }

        if (Owner.StatusEffectController.TryGetStatusEffect<DodgeCastSpell>(out var dodgeCastSpell))
        {
            bool dodged = await dodgeCastSpell.TryToDodge(Owner.Owner.match);
            if (dodged) return;
        }

        int finalDamage = CalculateDamage(damage);
        currentHealth = Math.Max(0, currentHealth - finalDamage);

        OnDamageReceived?.Invoke(currentHealth, finalDamage);

        TryToReturnDamage(damage, damageReturn, finalDamage);

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

    private void TryToReturnDamage(Damage damage, bool damageReturn, int finalDamage)
    {
        if (!damageReturn && finalDamage > 0 && damage.Source != null)
        {
            DamageableBehaviour damageableBehaviour = damage.Source.GetBehaviour<DamageableBehaviour>();
            if (Owner.StatusEffectController.HasStatusEffect<DamageReturn>() && damageableBehaviour != null && damageableBehaviour.CanReceiveDamage)
            {
                DamageReturn damageReturnEffect = Owner.StatusEffectController.GetStatusEffect<DamageReturn>();
                int reflectedDamage = Mathf.CeilToInt(finalDamage * damageReturnEffect.ReturnPercentage);

                damageableBehaviour.ReceiveDamage(new Damage(reflectedDamage, damage.Type, Owner), damageReturn: true);
            }
        }
    }

    public void IncreaseCurrentHealth(int value)
    {
        currentHealth += value;

        if(currentHealth < 1)
            currentHealth = 1;
    }

    public void IncreaseMaxHealth(int valuse)
    {
        maxHealth += valuse;

        if (maxHealth < 1)
            maxHealth = 1;
    }

    public void SetCurrentHealth(int currentHealth)
    {
        this.currentHealth = currentHealth;

        if (this.currentHealth < 1)
            this.currentHealth = 1;
    }

    public void SetMaxHealth(int maxHealth)
    {
        this.maxHealth = maxHealth;

        if(this.maxHealth < 1)
            this.maxHealth = 1;
    }
    public override BehaviourData GetBehaviourData() => new DamageableBehaviourData(this);
}

public class MeleeAttackBehaviour : AttackBehaviour
{
    protected MeleeAttackBehaviour() : base() { }
    #region Builder
    public class Builder : Builder<MeleeAttackBehaviour, MeleeAttackBlueprint, MeleeAttackData>
    {
        public Builder(Entity owner) : base(new MeleeAttackBehaviour(), owner) { }
    }
    #endregion
    public override BehaviourData GetBehaviourData() => new MeleeAttackData(this);
    protected override Damage CreateDamage() => new Damage(AttackDamage, DamageType.Physical, Owner);
}

public class RangedAttackBehaviour : AttackBehaviour
{
    protected RangedAttackBehaviour() : base() { }
    #region Builder
    public class Builder : Builder<RangedAttackBehaviour, RangedAttackBlueprint, RangedAttackData>
    {
        public Builder(Entity owner) : base(new RangedAttackBehaviour(), owner) { }
    }
    #endregion
    public override BehaviourData GetBehaviourData() => new RangedAttackData(this);
    protected override Damage CreateDamage() => new Damage(AttackDamage, DamageType.Physical, Owner);
}

public class Damage
{
    public int Amount { get; private set; }
    public DamageType Type { get; private set; }
    public Entity Source { get; private set; }

    public Damage(int amount, DamageType type, Entity source)
    {
        Amount = amount;
        Type = type;
        Source = source;
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
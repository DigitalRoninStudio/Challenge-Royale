using System.Collections.Generic;
using System.Linq;

public class Player
{
    public string clientId;
    public Team team;
    public PlayerState playerState;
    public Game match;
    public List<Entity> entities;
    public EnergyController energyController;

    public Player()
    {
        entities = new List<Entity>();
        energyController = new EnergyController();
    }
    public Player(PlayerData playerData)
    {
        clientId = playerData.Id;
        team = playerData.Team;
        playerState = playerData.PlayerState;
        energyController = new EnergyController(playerData.EnergyData);

        entities = new List<Entity>();

        // CREATE ENTITY
        foreach (var entityData in playerData.EntityData)
        {
            Entity entity = GameFactory.CreateEntity(entityData);
            AddEntity(entity);
        }

        // CREATE BEHAVIOUR
        foreach (var entityData in playerData.EntityData)
        {
            var entity = entities.FirstOrDefault(e => e.guid == entityData.GUID);
            if (entity != null)
            {
                foreach (BehaviourData behaviourData in entityData.BehaviourDatas)
                {
                    var blueprint = GameFactory.FindBehaviourBlueprint(behaviourData);
                    Behaviour behaviour = blueprint?.CreateBehaviour(behaviourData); 
                    entity.AddBehaviour(behaviour);
                }
            }
        }

        // CREATE STATUS EFFECT
        foreach (var entityData in playerData.EntityData)
        {
            var entity = entities.FirstOrDefault(e => e.guid == entityData.GUID);
            if (entity != null)
            {
                foreach (var statusEffectData in entityData.StatusEffectDatas)
                {
                    StatusEffect statusEffect = GameFactory.CreateStatusEffect(statusEffectData, entities);
                    entity.StatusEffectController.StatusEffects.Add(statusEffect);
                }
            }
        }

        // CREATE MODFIERS
        foreach (var entityData in playerData.EntityData)
        {
            var entity = entities.FirstOrDefault(e => e.guid == entityData.GUID);
            if (entity != null)
            {
                foreach (var modifierData in entityData.ModifierSourceDatas)
                {
                    Modifier modifier = GameFactory.FindModifier(modifierData, entities);
                    entity.ModifierController.Modifiers.Add(modifier);
                }
            }
        }
    }
    public void AddEntity(Entity entity)
    {
        entity.SetOwner(this);
        entities.Add(entity);
    }

    public PlayerData GetPlayerData() => new PlayerData(this);

    public void SetPlayerStateDependOnInitiation(Team teamWithInitiation)
    {
        if (teamWithInitiation == team)
            playerState = PlayerState.PLAYING;
        else
            playerState = PlayerState.IDLE;

    }
  
}

public enum PlayerState
{
    IDLE, PLAYING
}






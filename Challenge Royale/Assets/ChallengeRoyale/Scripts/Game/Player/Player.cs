using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Player
{
    public string clientId;
    public Team team;
    public PlayerState playerState;
    public Game match;
    public List<Entity> entities;
    public EnergyController energyController;
    public Dictionary<string, int> actionCastCounter;

    public Player()
    {
        entities = new List<Entity>();
        energyController = new EnergyController();
        actionCastCounter = new Dictionary<string, int>();
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
                    Behaviour behaviour = blueprint?.CreateBehaviour(behaviourData, entity); 
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
                    StatusEffect statusEffect = GameFactory.CreateStatusEffect(statusEffectData, entities, entity);
                    entity.StatusEffectController.StatusEffects.Add(statusEffect);
                }
            }
        }

        actionCastCounter = playerData.actionCastCounter;
    }
    public void SetMatch(Game game)
    {
        match = game;
        match.roundController.OnChangeInitiation += OnChangeInitiation;
    }

    private void OnChangeInitiation(Team teamWithInitiation)
    {
        if (teamWithInitiation == team)
        {
            playerState = PlayerState.PLAYING;
            actionCastCounter.Clear();
        }
        else
            playerState = PlayerState.IDLE;
    }

    public void AddEntity(Entity entity)
    {
        entity.SetOwner(this);
        entities.Add(entity);
    }
    public void AddCastAction(string guid) => actionCastCounter[guid] = actionCastCounter.GetValueOrDefault(guid, 0) + 1;
    public bool CanCastAction(Behaviour behaviour) => !HasCastAction(behaviour.guid) || GetActionCastCount(behaviour.guid) < behaviour.MaxCast;
    public bool HasCastAction(string guid) => actionCastCounter.ContainsKey(guid);
    public int GetActionCastCount(string guid) => actionCastCounter.GetValueOrDefault(guid, 0);

    public PlayerData GetPlayerData() => new PlayerData(this);
  
}

public enum PlayerState
{
    IDLE, PLAYING
}






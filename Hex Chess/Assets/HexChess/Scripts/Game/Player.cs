using Newtonsoft.Json;
using System.Collections.Generic;

public class Player
{
    public string clientId;
    public Team team;
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
        energyController = new EnergyController(playerData.EnergyData);

        entities = new List<Entity>();
        foreach (var entityData in playerData.EntityData)
        {
            Entity entity = GameFactory.CreateEntity(entityData);
            AddEntity(entity);
        }
    }
    public void AddEntity(Entity entity)
    {
        entity.SetOwner(this);
        entities.Add(entity);
    }

    public PlayerData GetPlayerData() => new PlayerData(this);
}





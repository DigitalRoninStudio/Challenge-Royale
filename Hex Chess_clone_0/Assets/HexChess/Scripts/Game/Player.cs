using Newtonsoft.Json;
using System.Collections.Generic;

public class Player
{
    public string clientId;
    public Team team;
    public Game match;
    public List<Entity> entities;

    public Player()
    {
        entities = new List<Entity>();
    }

    public void AddEntity(Entity entity)
    {
        entity.SetOwner(this);
        entities.Add(entity);
    }
}




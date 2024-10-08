using Newtonsoft.Json;
using System.Collections.Generic;

public class Player
{
    public string clientId;
    public Team team;
    public Game match;
    [JsonConverter(typeof(CustomConverters.EntityConvertor))] public Entity entity1;
    [JsonIgnore]public List<Entity> entities;
}




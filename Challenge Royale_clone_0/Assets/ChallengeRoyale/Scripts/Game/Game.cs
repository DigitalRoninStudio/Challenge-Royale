using Newtonsoft.Json;
using System.Collections.Generic;

public class Game
{
    public string id;
    [JsonIgnore] public Map map;
    [JsonIgnore] public List<Player> players;

    public void AddPlayer(Player player)
    {
        player.match = this;
        players.Add(player);
    }
    public void Start()
    {
    }

    public void Update()
    {

    }

    public void End()
    {

    }


}




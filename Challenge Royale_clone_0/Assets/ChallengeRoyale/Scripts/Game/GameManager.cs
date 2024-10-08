using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class GameManager : Singleton<GameManager>
{
    public Dictionary<string, Game> matches = new Dictionary<string, Game>();

    public Dictionary<string, Player> playerIds = new Dictionary<string, Player>();
    public Dictionary<Player, string> players = new Dictionary<Player, string>();

    public Dictionary<NetworkConnection, Player> connections = new Dictionary<NetworkConnection, Player>();
    public Dictionary<Player, NetworkConnection> playerConnections = new Dictionary<Player, NetworkConnection>();
    Game game;
    Player player;
    public void CreateMatch(string matchId)
    {
        Game game = new Game();
        matches.Add(matchId, game);
    }

    public void AddPlayer(string matchId, string clientId, NetworkConnection connection)
    {
        if (!matches.ContainsKey(matchId))
            throw new ArgumentException("Match ID not found", nameof(matchId));

        Player player = new Player() { clientId = clientId };

        matches[matchId].AddPlayer(player);
        playerIds.Add(clientId, player);
        players.Add(player, clientId);
        connections.Add(connection, player);
        playerConnections.Add(player, connection);
    }

    public Game GetMatch(string matchId)
    {
        if (matches.TryGetValue(matchId, out var game))
            return game;

        throw new ArgumentException("Match ID not found", nameof(matchId));
    }

    public string GetPlayerId(Player player)
    {
        if (players.TryGetValue(player, out var clientId))
            return clientId;

        throw new ArgumentException("Player not found", nameof(player));
    }

    public Player GetPlayer(string clientId)
    {
        if (playerIds.TryGetValue(clientId, out var player))
            return player;

        throw new ArgumentException("Player ID not found", nameof(clientId));
    }

    public Player GetPlayer(NetworkConnection connection)
    {
        if (connections.TryGetValue(connection, out var player))
            return player;

        throw new ArgumentException("Player not found", nameof(connection));
    }

    public NetworkConnection GetConnection(Player player)
    {
        if (playerConnections.TryGetValue(player, out var connection))
            return connection;

        throw new ArgumentException("Connection not found", nameof(player));
    }

    private void Start()
    {
        player = new Player() { clientId = "JOVAN JEBAC" };
        game = new Game() { id = "asdqwezxc" };
        game.players = new List<Player>() { (player) };
        game.Start();

        game.map = new HexagonMap(4, 4, 1.05f, 0.5f, MapEditor.Instance.hex);

        player.match = game;
        player.team = Team.GOOD_BOYS;

        matches.Add(game.id, game);
        playerIds.Add(player.clientId, player);

        Figure figure = new Figure();
        figure.id = "Knight 123";
        figure.name = "Knight";
        figure.owner = player;
        figure.direction = Direction.UP;
        figure.visibility = Visibility.BOTH;
        figure.GameObject.transform.eulerAngles = new Vector3(90, 0, 0);
        figure.GameObject.transform.localScale = Vector3.one * 0.5f;
        figure.GameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        game.map.GetTile(0, 3).AddEntity(figure);

        player.entity1 = figure;
    }

    public void Update()
    {
        if (game == null) return;
        if (Input.GetMouseButtonDown(1))
        {
            Tile desiredTile = game.map.OnHoverMapGetTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if(desiredTile != null)
            {
                Debug.Log("USO");
                NormalMovement movement = new NormalMovement(player.entity1, 0.1f, 5);
                movement.SetPath(game.map.GetTile(player.entity1), desiredTile);
                player.entity1.AddBehaviourToWork(movement);
            }



            /*string json = CustomConverters.Serialize(game);
            Game g = CustomConverters.Deserialize<Game>(json);
            string secondJson = CustomConverters.Serialize(g);
            Debug.Log(secondJson);*/
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            foreach (var tile in game.map.Tiles)
            {
                if(!tile.Walkable)
                    tile.GameObject.GetComponent<SpriteRenderer>().material.color = Color.red;
            }
        }
        player.entity1?.Update();
    }
}

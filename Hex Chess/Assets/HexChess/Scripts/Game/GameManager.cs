using System;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public string gameJson;
    public GlobalBlueprints GlobalData;
    public Dictionary<string, Game> matches = new Dictionary<string, Game>();

    public Dictionary<string, Player> playerIds = new Dictionary<string, Player>();
    public Dictionary<Player, string> players = new Dictionary<Player, string>();

    public Dictionary<NetworkConnection, Player> connections = new Dictionary<NetworkConnection, Player>();
    public Dictionary<Player, NetworkConnection> playerConnections = new Dictionary<Player, NetworkConnection>();


    public Game game = null;


    public InputReader inputReader;
    private PlayerController playerController;

    public void CreateMatch(string matchId)
    {
        /* Game game = new Game();
         matches.Add(matchId, game);*/

        GameData gameData = GameStateConverter.Deserialize<GameData>(gameJson);
        GameFactory gameFactory = new GameFactory();
        game = gameFactory.CreateGame(gameData);
        playerController = new PlayerController(inputReader, game);
    }

    public void AddPlayer(string matchId, string clientId, NetworkConnection connection)
    {
        if (!matches.ContainsKey(matchId))
            throw new ArgumentException("Match ID not found", nameof(matchId));

        Player player = new Player() { id = clientId };

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
        CreateMatch("");
    }

    private void Update()
    {

        if (game != null)
        {
            foreach (var player in game.players)
            {
                foreach (var entity in player.entities)
                {
                    entity.Update();
                }
            }
        }
    }
}

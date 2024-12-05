using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Networking.Transport;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public string gameJson;
    public GlobalBlueprints GlobalData;

    private Dictionary<string, Game> matches = new Dictionary<string, Game>();

    private Dictionary<string, NetworkConnection> playersByID = new Dictionary<string, NetworkConnection>();
    private Dictionary<NetworkConnection, string> playersByConnection = new Dictionary<NetworkConnection, string>();

    public InputReader inputReader;
    private PlayerController playerController;
    public bool isServer = false;
 

    private void Start()
    {
        if(isServer)
        {
            NetworkManager.Instance.StartServer();
            CreateMatch(gameJson);

           // Server.OnClientConnected += OnClientConnected;
            Server.OnClientDisconnected += OnClientDisconnected;
        }
        else
        {
            NetworkManager.Instance.ConnectToServer();
           /* Client.OnClientConnected += OnConnected;
            Client.OnClientDisconnected += OnDisconnected;*/
        }
    }

    //SERVER
    private void OnClientDisconnected(NetworkConnection connection)
    {
        if (playersByConnection.TryGetValue(connection, out string playerID))
        {
            playersByID.Remove(playerID);
            playersByConnection.Remove(connection);
            NetworkLogger.Log($"Player {playerID} disconnected and removed from dictionary");
        }
    }

    private void OnClientConnected(NetworkConnection connection)
    {
        
    }
    public void AddPlayer(string id, NetworkConnection connection)
    {
        playersByID[id] = connection;
        playersByConnection[connection] = id;
    }

    public string GetPlayerId(NetworkConnection connection)
    {
        return playersByConnection.TryGetValue(connection, out var id) ? id : "";
    }

    public NetworkConnection GetNetworkConnection(string clientId)
    {
        return playersByID.TryGetValue(clientId, out var connection) ? connection : default;
    }
    //CLIENT
    private void OnDisconnected(NetworkConnection connection)
    {
        throw new NotImplementedException();
    }

    private void OnConnected(NetworkConnection connection)
    {
        throw new NotImplementedException();
    }

    public void CreateMatch(string json, bool isClient = false)
    {
        GameData gameData = GameStateConverter.Deserialize<GameData>(json);

        Game game = new Game(gameData);

        matches.Add(game.GUID, game);

        if(isClient)
            playerController = new PlayerController(inputReader, game);
    }

    public string GetMatchJson(Game game)
    {
        return GameStateConverter.Serialize(game.GetGameData());
    }
    private void Update()
    {
        foreach(var game in matches)
        {
            game.Value.Update();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(GameManager.Instance.GetMatchJson(GetFirstMatch()));
        }
    }

    private void OnDestroy()
    {
        Server.OnClientConnected -= OnClientConnected;
        Server.OnClientDisconnected -= OnClientDisconnected;
        Client.OnClientConnected -= OnConnected;
        Client.OnClientDisconnected -= OnDisconnected;

    }

    public Game GetMatch(string matchId)
    {
        return matches.TryGetValue(matchId, out var match) ? match : null;
    }

    public Game GetFirstMatch()
    {
        return matches.First().Value;
    }
}

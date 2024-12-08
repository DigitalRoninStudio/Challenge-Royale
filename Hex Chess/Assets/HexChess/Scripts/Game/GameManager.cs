using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEngine;

public class LocalPlayer : Singleton<LocalPlayer>
{
    public string LocalClientID;

    public void PaintTilesOnSelectEntity(Entity selectedEntity)
    {
        Game game = GameManager.Instance.GetFirstMatch();
        MovementBehaviour movementBehaviour = selectedEntity.GetBehaviour<MovementBehaviour>();
        AttackBehaviour attackBehaviour = selectedEntity.GetBehaviour<AttackBehaviour>();
        Tile tile = game.map.GetTile(selectedEntity);

        if (tile == null) return;

        if(IsLocalClientOwner(selectedEntity) && CanLocalPlayerDoAction())
        {
            if (movementBehaviour != null && movementBehaviour is IActionTileSelection movementTiles)
            {
                foreach (var availableTile in movementTiles.GetAvailableTiles())
                    availableTile.SetColor(Color.green);

                Color DarkGreen = new Color(0, 0.5f, 0f);
                foreach (var unAvailableTile in movementTiles.GetUnAvailableTilesInRange())
                    unAvailableTile.SetColor(DarkGreen);
            }

            if (attackBehaviour != null && attackBehaviour is IActionTileSelection attackTiles)
                foreach (var availableTile in attackTiles.GetAvailableTiles())
                    availableTile.SetColor(Color.red);
        }
        else
        {

            Color DarkGreen = new Color(0, 0.5f, 0f);
            if (movementBehaviour != null && movementBehaviour is IActionTileSelection movementTiles)
                foreach (var tiles in movementTiles.GetTiles())
                    tiles.SetColor(DarkGreen);

            Color DarkRed = new Color(0.5f, 0f, 0f);
            if (attackBehaviour != null && attackBehaviour is IActionTileSelection attackTiles)
                foreach (var availableTile in attackTiles.GetAvailableTiles())
                    availableTile.SetColor(DarkRed);
        }
    }

    public void EndRound()
    {
        Game game = GameManager.Instance.GetFirstMatch();
        Player player = game.GetPlayer(LocalClientID);
        if (player == null) return;

        if (game.IsPlayerHasInitiation(player))
        {
            Sender.ClientSendData(new NetEndRound() { MatchId = game.GUID }, Pipeline.Reliable);
            Debug.Log("END ROUND");
        }
    }

    public void HandOverTheInitiative()
    {
        Game game = GameManager.Instance.GetFirstMatch();
        Player player = game.GetPlayer(LocalClientID);
        if (player == null) return;

        if (game.IsPlayerHasInitiation(player))
        {
            Sender.ClientSendData(new NetHandOverTheInitiative() { MatchId = game.GUID }, Pipeline.Reliable);
            Debug.Log("END INITIATION");
        }
    }

    public void ClearTiles()
    {
        foreach (var tile in GameManager.Instance.GetFirstMatch().map.Tiles) { tile.RefreshColor(); }
    }

    public bool IsLocalClientOwner(Entity entity)
    {
        return entity.Owner.clientId == LocalClientID;
    }

    public bool CanLocalPlayerDoAction()
    {
        Game game = GameManager.Instance.GetFirstMatch();
        Player player = game.GetPlayer(LocalClientID);
        if(player == null) return false;

        return player.team == game.roundController.teamWithInitiation && player.playerState == PlayerState.PLAYING;
    }
}


public class GameManager : Singleton<GameManager>
{
    public GlobalBlueprints GlobalData;

    private Dictionary<string, Game> matches = new Dictionary<string, Game>();

    private Dictionary<string, NetworkConnection> playersByID = new Dictionary<string, NetworkConnection>();
    private Dictionary<NetworkConnection, string> playersByConnection = new Dictionary<NetworkConnection, string>();

    public InputReader inputReader;
    private PlayerController playerController;

    [Space(50)]
    public string gameJson;
    public string LocalClient = "PLAYER_1";
    public string GameID = "GAME";
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
            Client.OnClientConnected += OnConnected;
            Client.OnClientDisconnected += OnDisconnected;
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
        // Destroy(gameObject.GetComponent<LocalPlayer>());
    }

    private void OnConnected(NetworkConnection connection)
    {
        gameObject.AddComponent<LocalPlayer>().LocalClientID = LocalClient;
    }

    public void CreateMatch(string json, bool isClient = false)
    {
        GameData gameData = GameStateConverter.Deserialize<GameData>(json);

        Game game = new Game(gameData);

        matches.Add(game.GUID, game);

        if(isClient)
            playerController = new PlayerController(inputReader, game);
    }

    public string GetGameJson(Game game)
    {
        return GameStateConverter.Serialize(game.GetGameData());
    }
    private void Update()
    {
        foreach(var game in matches)
        {
            game.Value.Update();
        }
      /*  if(Input.GetKeyDown(KeyCode.Space))
        {
            Game g = new Game();
            Player player1 = new Player()
            {
                clientId = "PLAYER_1",
                team = Team.GOOD_BOYS,
                playerState = PlayerState.IDLE
            };
            Player player2 = new Player()
            {
                clientId = "PLAYER_2",
                team = Team.BAD_BOYS,
                playerState = PlayerState.IDLE
            };
            g.AddPlayer(player1);
            g.AddPlayer(player2);

            g.roundController.SetFirstRound();

            MapEditor.Instance.LoadMap();
            g.map = MapEditor.Instance.Map;

            foreach(var spawnPosition in g.map.SpawnPositions)
            {                
                Entity entity = GameManager.Instance.GlobalData.FractionBlueprints
                   .SelectMany(f => f.EntityBlueprints)
                   .FirstOrDefault(e => e is FigureBlueprint figure && figure.FigureType == spawnPosition.FigureType && 
                   ((figure.FractionType == FractionType.LIGHT && spawnPosition.Team == Team.GOOD_BOYS) || 
                   (figure.FractionType == FractionType.DARK && spawnPosition.Team == Team.BAD_BOYS)))
                   ?.CreateEntity();

                if(spawnPosition.Team == Team.GOOD_BOYS)
                    player1.AddEntity(entity);
                else
                    player2.AddEntity(entity);

                g.map.GetTile(spawnPosition.Coordinate).AddEntity(entity);
            }

            string json = GameManager.Instance.GetGameJson(g);
            Debug.Log(json);
            GameData gameData = GameStateConverter.Deserialize<GameData>(json);
            //GameData gameData = GameStateConverter.Deserialize<GameData>(json);
        }*/

        if (Input.GetKeyDown(KeyCode.R) && LocalPlayer.Instance.CanLocalPlayerDoAction())
        {
            LocalPlayer.Instance.EndRound();
        }
        if (Input.GetKeyDown(KeyCode.S) && LocalPlayer.Instance.CanLocalPlayerDoAction())
        {
            LocalPlayer.Instance.HandOverTheInitiative();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            string json = GameManager.Instance.GetGameJson(GetFirstMatch());
            Debug.Log(json);
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

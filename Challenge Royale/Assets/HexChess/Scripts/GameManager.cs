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
            if (movementBehaviour != null && movementBehaviour is ITileSelection movementTiles)
            {
                foreach (var availableTile in movementTiles.GetAvailableTiles())
                    availableTile.SetColor(Color.green);

                Color DarkGreen = new Color(0, 0.5f, 0f);
                foreach (var unAvailableTile in movementTiles.GetUnAvailableTilesInRange())
                    unAvailableTile.SetColor(DarkGreen);
            }

            if (attackBehaviour != null && attackBehaviour is ITileSelection attackTiles)
                foreach (var availableTile in attackTiles.GetAvailableTiles())
                    availableTile.SetColor(Color.red);
        }
        else
        {

            Color DarkGreen = new Color(0, 0.5f, 0f);
            if (movementBehaviour != null && movementBehaviour is ITileSelection movementTiles)
                foreach (var tiles in movementTiles.GetTiles())
                    tiles.SetColor(DarkGreen);

            Color DarkRed = new Color(0.5f, 0f, 0f);
            if (attackBehaviour != null && attackBehaviour is ITileSelection attackTiles)
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

[RequireComponent(typeof(InputReader))]
public class GameManager : Singleton<GameManager>
{
    public GlobalBlueprints GlobalData;
    public Transform MapContainer;

    private Dictionary<string, Game> matches = new Dictionary<string, Game>();

    private Dictionary<string, NetworkConnection> playersByID = new Dictionary<string, NetworkConnection>();
    private Dictionary<NetworkConnection, string> playersByConnection = new Dictionary<NetworkConnection, string>();

    private PlayerInputController playerInputController;
    public List<Pallete> palletes = new List<Pallete>();

    [Space(50)]
    public string gameJson;
    public string LocalClient = "PLAYER_1";
    public string GameID = "GAME";
    public bool isServer = false;

    private void Start()
    {
        if (isServer)
        {
            NetworkManager.Instance.StartServer();
            CreateGame();

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
    public void CreateGame()
    {
        Game game = new Game();
        game.GUID = GameID;

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
        game.AddPlayer(player1);
        game.AddPlayer(player2);

        game.roundController.SetFirstRound();

        game.map = GameFactory.CreateMap();

        game.AddEntity(FractionType.LIGHT, FigureType.King, 0, -5, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Tank, -1, -4, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Tank, 1, -5, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Jester, -2, -3, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Wizard, 2, -5, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Queen, 0, -4, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Archer, 1, -4, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Archer, -1, -3, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Knight, -3, -2, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Knight, 3, -5, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Swordsman, 2, -4, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Swordsman, 0, -3, player1, game.map);
        game.AddEntity(FractionType.LIGHT, FigureType.Swordsman, -2, -2, player1, game.map);

        game.AddEntity(FractionType.DARK, FigureType.King, 0, 5, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Tank, 1, 4, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Tank, -1, 5, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Jester, 2, 3, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Wizard, -2, 5, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Queen, 0, 4, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Archer, -1, 4, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Archer, 1, 3, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Knight, 3, 2, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Knight, -3, 5, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Swordsman, -2, 4, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Swordsman, 0, 3, player2, game.map);
        game.AddEntity(FractionType.DARK, FigureType.Swordsman, 2, 2, player2, game.map);
        //remove
        UIManager.Instance.OnGameCreated?.Invoke(game);
        matches.Add(game.GUID, game);

    }
    public void CreateMatch(string json, bool isClient = false)
    {
        GameData gameData = GameStateConverter.Deserialize<GameData>(json);

        Game game = new Game(gameData);

        matches.Add(game.GUID, game);

        if(isClient)
        {
            playerInputController = new PlayerInputController(GetComponent<InputReader>(), game);
        }
    }

    public string GetGameJson(Game game)
    {
        return GameStateConverter.Serialize(game.GetGameData());
    }
   // Game g;
    private void Update()
    {
        foreach(var game in matches)
        {
            game.Value.Update();
        }
        /* if(Input.GetKeyDown(KeyCode.Space))
         {
             g = new Game();
             g.GUID = "GAME";
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

                 Tile tile = g.map.GetTile(spawnPosition.Coordinate);
                 tile.AddEntity(entity);
                 entity.OnPlaced?.Invoke(tile);
             }
             string json = GameManager.Instance.GetGameJson(g);
             string path = "C:/Users/jovan/Desktop/gamejson.txt";
             System.IO.File.WriteAllText(path, json);

             //Debug.Log(json);
             GameData gameData = GameStateConverter.Deserialize<GameData>(json);
             //GameData gameData = GameStateConverter.Deserialize<GameData>(json);
         }*/

     
        if (Input.GetKeyDown(KeyCode.T))
        {
            Game game = GetFirstMatch();

            Entity entity1 = game.map.GetTile(0, -2).GetEntities().First();
            game.actionController.AddActionToWork(entity1.GetBehaviour<SwordsmanSpecial>());
            Entity entity2 = game.map.GetTile(2, -3).GetEntities().First();
            game.actionController.AddActionToWork(entity2.GetBehaviour<SwordsmanSpecial>());
            // game.Update();

            string json = GameManager.Instance.GetGameJson(GetFirstMatch());
            string path = "C:/Users/jovan/Desktop/gamejson.txt";
            System.IO.File.WriteAllText(path, json);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            string json = GameManager.Instance.GetGameJson(GetFirstMatch());
            string path = "C:/Users/jovan/Desktop/gamejson.txt";
            System.IO.File.WriteAllText(path, json);
            Debug.Log("JSON SAVED");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Game game = GetFirstMatch();
            foreach (var entity in game.GetAllEntities())
            {
                if(entity.StatusEffectController.StatusEffects.Count > 0)
                {
                    foreach (var se in entity.StatusEffectController.StatusEffects)
                    {
                        Debug.Log(se.GetType());
                    }
                }
            }
            string json = GameManager.Instance.GetGameJson(GetFirstMatch());
            string path = "C:/Users/jovan/Desktop/gamejson.txt";
            System.IO.File.WriteAllText(path, json);
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

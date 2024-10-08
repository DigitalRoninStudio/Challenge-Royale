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
    
    public void CreateMatch(string matchId)
    {
        Game game = new Game();
        matches.Add(matchId, game);
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

    public void Start()
    {
    }
    Game game = null;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
           /* GameFactory gameFactory = new GameFactory();
            Entity entity = gameFactory.CreateFigure(FractionType.LIGHT, FigureType.Swordsman);
            EntityData data = entity.GetEntityData();
            string json = GameStateConverter.Serialize(data);
            Debug.Log(json);
            entity.Dispose();
            FigureData figureData = GameStateConverter.Deserialize<FigureData>(json);
            Entity newEntity = gameFactory.CreateEntity(figureData);
            Debug.Log("Entity name: " + newEntity.name);
            Debug.Log("Entity guid: " + newEntity.guid);
            Debug.Log("Entity id: " + newEntity.id);
            Debug.Log("Behaviour name: " + newEntity.Behaviours[0].name);
            Debug.Log("Behaviour guid: " + newEntity.Behaviours[0].guid);
            Debug.Log("Behaviour id: " + newEntity.Behaviours[0].id);*/
             /*   game = new Game();
               game.GUID = Guid.NewGuid().ToString();
               game.map = MapEditor.Instance.Map;

               Player player1 = new Player();
               player1.id = "Player 1";
               player1.team = Team.GOOD_BOYS;

               Player player2 = new Player();
               player2.id = "Player 2";
               player2.team = Team.BAD_BOYS;

            GameFactory gameFactory = new GameFactory();*/
            /*Figure figure1 = gameFactory.CreateFigure(FractionType.LIGHT, FigureType.Swordsman);
            Figure figure2 = gameFactory.CreateFigure(FractionType.DARK, FigureType.Swordsman);

            figure1.direction = Direction.UP;
            figure1.visibility = Visibility.BOTH;

            figure2.direction = Direction.DOWN;
            figure2.visibility = Visibility.BOTH;*/

            /* DamageableBehaviour behaviour = gameFactory.CreateBehaviourByBlueprintId("DAMAGAEBLE") as DamageableBehaviour;
             figure.AddBehaviour(behaviour);
             Damage damage = new Damage(1, DamageType.Physical);
             behaviour.ReceiveDamage(damage);*/
            /*player1.AddEntity(figure1);
            player2.AddEntity(figure2);

            game.AddPlayer(player1);
            game.AddPlayer(player2);

            game.map.GetTile(0, 0).AddEntity(figure1);
            game.map.GetTile(0, 2).AddEntity(figure2);

            AttackBehaviour attack = figure1.GetBehaviour<AttackBehaviour>();
            if (attack.CanAttack(figure2))
            {
                attack.SetAttack(figure2.GetBehaviour<DamageableBehaviour>());
                figure1.AddBehaviourToWork(attack);
            }*/
            /* if(behaviour.GetAvailableMoves(game.map.GetTile(0, 0)).Contains(game.map.GetTile(3,0)))
           {
               behaviour.SetPath(game.map.GetTile(0, 0), game.map.GetTile(3,0));
               figure.AddBehaviourToWork(behaviour);
           }*/
            //figure.AddBehaviourToWork(knightMovement);

           /* game.random.Next();
            game.random.Next();
            game.random.Next();
            game.random.Next();
            game.random.Next();
            game.random.Next();
            game.random.Next();
            GameData gameData = gameFactory.CreateGameData(game);
             string json = GameStateConverter.Serialize(gameData);
             Debug.Log(json);
            Debug.Log("RANDOM: " + game.random.Next());*/

              GameData gameData = GameStateConverter.Deserialize<GameData>(gameJson);
              GameFactory gameFactory = new GameFactory();
              game = gameFactory.CreateGame(gameData);
              Debug.Log("RANDOM: " + game.randomGenerator.Next());
            /* Debug.Log(game.players[0].entities[0].Behaviours[0].name);
             Debug.Log(((DamageableBehaviour)game.players[0].entities[0].Behaviours[1]).name
                 + " " + ((DamageableBehaviour)game.players[0].entities[0].Behaviours[1]).CurrentHealth + " " + 
                 ((DamageableBehaviour)game.players[0].entities[0].Behaviours[1]).MaxHealth);*/
            /* KnightMovement knightMovement = game.players[0].entities[0].Behaviours[0] as KnightMovement;
             knightMovement.SetPath(game.map.GetTile(2, 0), game.map.GetTile(1, -1));
             game.players[0].entities[0].AddBehaviourToWork(knightMovement);
             //Debug.Log(game.players[0].entities[0].Behaviours[0].id);*/

            //0.164219787615403
        }

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

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEngine;

public class ActionController
{
    protected Game game;
    protected Queue<ILifecycleAction> pendingActions;

    public ActionController(Game game)
    {
        this.game = game;
        pendingActions = new Queue<ILifecycleAction>();
    }

    public void Update()
    {
        if (pendingActions.Count > 0)
            pendingActions.Peek().Execute();
    }

    public void AddActionToWork(ILifecycleAction action)
    {
        if (action != null)
        {
            if (pendingActions.Count == 0)
            {
                pendingActions.Enqueue(action);
                StartAction(action);
            }
            else
                pendingActions.Enqueue(action);

        }
    }

    private void ChangeAction()
    {
        if (pendingActions.Count > 0)
            pendingActions.Dequeue();

        if (pendingActions.Count > 0)
            StartAction(pendingActions.Peek());
    }

    private void StartAction(ILifecycleAction actionLifecycle)
    {
        void OnActionEndHandler()
        {
            actionLifecycle.OnActionEnd -= OnActionEndHandler;
            ChangeAction();
        }

        actionLifecycle.OnActionEnd += OnActionEndHandler;
        actionLifecycle.Enter();

        if (Server.IsServer && actionLifecycle is INetAction serializable)
            BroadcastActionToClients(game, serializable);
    }

    public static void BroadcastActionToClients(Game game, INetAction serializable)
    {
        NetGameAction responess = serializable.SerializeAction();

        foreach (var player in game.players)
        {
            NetworkConnection connection = GameManager.Instance.GetNetworkConnection(player.clientId);
            if (connection != null)
                Sender.ServerSendData(connection, responess, Pipeline.Reliable);
        }
    }
}
public class Game
{
    public string GUID;
    public Map map;
    public List<Player> players;
    public RandomGenerator randomGenerator;
    public RoundController roundController;
    public ActionController actionController;

    public Game()
    {
        GUID = Guid.NewGuid().ToString();
        players = new List<Player>();
        randomGenerator = new RandomGenerator();
        roundController = new RoundController(this);
        actionController = new ActionController(this);
    }

    public Game(GameData gameData)
    {
        GUID = gameData.GUID;

        players = new List<Player>();
        randomGenerator = new RandomGenerator(gameData.RandomState);
        roundController = new RoundController(this, gameData.RoundData);
        actionController = new ActionController(this);//TODO serialize and deserialize

        foreach (var playerData in gameData.PlayersData)
            AddPlayer(new Player(playerData));

        map = GameFactory.CreateMap(gameData.MapData);

        GameFactory.PlaceEntitiesOnTiles(gameData.MapData.entityPositions, this);
    }

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
        actionController.Update();
    }

    public void End()
    {

    }
    
    public List<Entity> GetAllEntities()
    {
        return players.SelectMany(player => player.entities).ToList();
    }

    public GameData GetGameData() => new GameData(this);

    public void SendMessageToPlayers(NetMessage responess)
    {
        foreach (var player in players)
        {
            NetworkConnection connection = GameManager.Instance.GetNetworkConnection(player.clientId);
            if (connection != null)
                Sender.ServerSendData(connection, responess, Pipeline.Reliable);
        }
    }

    public Player GetPlayer(string clientId)
    {
        foreach (var player in players)
            if (player.clientId == clientId)
                return player;

        return null;
    }

    public bool IsPlayerHasInitiation(Player player)
    {
        foreach (var p in players)
            if (p == player)
                if (player.team == roundController.teamWithInitiation)
                    return true;

        return false;
    }

}

public class RandomGenerator
{
    private uint seed;
    private uint initialSeed;
    private long sequencePosition;

    public RandomGenerator()
    {
        initialSeed = (uint)DateTime.Now.Ticks;
        Reset();
    }

    public RandomGenerator(RandomGeneratorState state)
    {
        SetState(state);
    }
    public void Reset()
    {
        seed = initialSeed;
        sequencePosition = 0;
    }

    public RandomGeneratorState GetState()
    {
        return new RandomGeneratorState
        {
            InitialSeed = initialSeed,
            CurrentPosition = sequencePosition
        };
    }
    private void SetState(RandomGeneratorState state)
    {
        initialSeed = state.InitialSeed;
        seed = initialSeed;
        sequencePosition = 0;

        for (long i = 0; i < state.CurrentPosition; i++) 
             Next();
    }

    public double Next()
    {
        const uint a = 1664525;
        const uint c = 1013904223;
        const uint m = 0xffffffff;

        seed = (a * seed + c) & m;
        sequencePosition++;

        return seed / (double)uint.MaxValue;
    }

    public int NextInt(int min = 0, int max = 100)
    {
        double value = Next();
        return (int)(value * (max - min + 1)) + min;
    }
    public float NextFloat()
    {
        return (float)Next();
    }

    public uint GetSeed() => initialSeed;
}


public static class GameFactory
{
    public static Entity CreateEntity(EntityData entityData)
    {
        Entity entity = GameManager.Instance.GlobalData.FractionBlueprints
            .SelectMany(f => f.EntityBlueprints)
            .FirstOrDefault(e => e.Id == entityData.Id)
            ?.CreateEntity();
        entity.FillWithData(entityData);
        return entity;
    }

    public static Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        Behaviour behaviour = GameManager.Instance.GlobalData.BehaviourDatasContainer.GetBehaviour(behaviourData);
        behaviour.FillWithData(behaviourData);
        return behaviour;
    }

    public static StatusEffect CreateStatusEffect(StatusEffectData statusEffectData)
    {
        StatusEffect statusEffect = GameManager.Instance.GlobalData.StatusEffectBlueprintsContainer.GetStatusEffect(statusEffectData);
        statusEffect.FillWithData(statusEffectData);
        return statusEffect;
    }

    public static Map CreateMap(MapData mapData)
    {
        return GameManager.Instance.GlobalData.MapBlueprints
               .FirstOrDefault(data => data.Id == mapData.Id)
               ?.CreateMap();
    }

    public static MapData CreateMapData(Game game)
    {
        MapData mapData = new();
        mapData.Id = game.map.MapId;

        foreach (var tile in game.map.Tiles)
        {
            if(tile.GetEntities().Count > 0)
            {
                mapData.entityPositions.Add(tile.coordinate, new List<string>());
                foreach (var entity in tile.GetEntities())
                    mapData.entityPositions[tile.coordinate].Add(entity.guid);
            }
        }

        return mapData;
    }
    public static void PlaceEntitiesOnTiles(Dictionary<Vector2Int, List<string>> entityPositions, Game game)
    {
        foreach (var pair in entityPositions)
        {
            foreach (var player in game.players)
                foreach (var entity in player.entities)
                    if (pair.Value.Contains(entity.guid))
                        game.map.GetTile(pair.Key).AddEntity(entity);
        }
    }
}
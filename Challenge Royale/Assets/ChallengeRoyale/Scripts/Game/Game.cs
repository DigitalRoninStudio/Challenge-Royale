using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Networking.Transport;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ActionController
{
    public ILifecycleAction CurrentAction => pendingActions.Count > 0 ? pendingActions.Last() : null;
    protected Game game;
    protected Queue<ILifecycleAction> pendingActions;
    private bool isWaiting;

    public bool IsWaiting => isWaiting;

    public ActionController(Game game)
    {
        this.game = game;
        pendingActions = new Queue<ILifecycleAction>();
    }

    public void Update()
    {
        if (isWaiting) return;

        if (pendingActions.Count > 0)
        {
            if (pendingActions.Peek().CanBeExecuted())
                pendingActions.Peek().Execute();
            else
                pendingActions.Peek().Exit();
        }
       
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

        if (Server.IsServer && actionLifecycle is INetAction netAction)
            BroadcastActionToClients(game, netAction);
    }

    public static void BroadcastActionToClients(Game game, INetAction netAction)
    {
        NetAction responess = new NetAction()
        {
            ActionType = netAction.ActionType,
            Action = netAction.SerializeAction()
        };

        game.SendMessageToPlayers(responess);
    }

    public void Wait()
    {
        isWaiting = true;
    }

    public void StopWaiting()
    {
        isWaiting = false;
    }
}

public class Dice
{
    public int NumberOfSides => numberOfSides;
    private int numberOfSides;
    private ActionController actionController;
    private RandomGenerator randomGenerator;

    public Dice(int numberOfSides, ActionController actionController, RandomGenerator randomGenerator)
    {
        this.numberOfSides = numberOfSides;
        this.actionController = actionController;
        this.randomGenerator = randomGenerator;
    }
    public async Task<int> RollAndGetNumber(float animationDuration = 2f)
    {
        actionController.Wait();

        int rolledNumber = randomGenerator.NextInt(1, numberOfSides); 
        await Task.Delay(TimeSpan.FromSeconds(animationDuration)); 

        actionController.StopWaiting();
        return rolledNumber;
    }
    public async Task<bool> RollAndCheck(int threshold, float animationDuration = 2f)
    {
        actionController.Wait();
        Debug.Log("ROLL");
        int rolledNumber = randomGenerator.NextInt(1, numberOfSides);
        await Task.Delay(TimeSpan.FromSeconds(animationDuration));

        Debug.Log("ROLL RESULT: " + rolledNumber);
        actionController.StopWaiting();
        return rolledNumber >= threshold;
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
    public List<ExecutedAction> executedClientsActions;
    public Dice dice;
    public Game()
    {
        GUID = Guid.NewGuid().ToString();
        players = new List<Player>();
        randomGenerator = new RandomGenerator();
        roundController = new RoundController(this);
        actionController = new ActionController(this);
        executedClientsActions = new List<ExecutedAction>();
        dice = new Dice(12, actionController, randomGenerator);
    }

    public Game(GameData gameData)
    {
        GUID = gameData.GUID;

        players = new List<Player>();
        randomGenerator = new RandomGenerator(gameData.RandomState);
        roundController = new RoundController(this, gameData.RoundData);
        actionController = new ActionController(this);
        executedClientsActions = gameData.executedActions;
        dice = new Dice(12, actionController, randomGenerator);

        foreach (var playerData in gameData.PlayersData)
            AddPlayer(new Player(playerData));

        map = GameFactory.CreateMap();

        GameFactory.PlaceEntitiesOnTiles(gameData.MapData.entityPositions, this);

        //REMOVE
        UIManager.Instance.OnGameCreated?.Invoke(this);
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);
        player.SetMatch(this);
    }

    public void AddEntity(FractionType fractionType, FigureType figureType, int column, int row, Player player, Map map)
    {
        Entity entity = GameManager.Instance.GlobalData.FractionBlueprints
                   .SelectMany(f => f.EntityBlueprints)
                   .FirstOrDefault(e => e is FigureBlueprint figure &&
                   figure.FigureType == figureType &&
                   figure.FractionType == fractionType)
                   ?.CreateEntity();

        player.AddEntity(entity);

        Tile tile = map.GetTile(column, row);
        tile.AddEntity(entity);
        entity.OnPlaced?.Invoke(tile);
    }
    public void Start()
    {
    }

    public void Update()
    {
        if (!actionController.IsWaiting)
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
    public string NextGuid()
    {
        byte[] guidBytes = new byte[16];
        byte[] doubleBytes = BitConverter.GetBytes(Next());

        for (int i = 0; i < 8; i++)
        {
            guidBytes[i] = doubleBytes[i];
            guidBytes[i + 8] = doubleBytes[i];
        }

        return Convert.ToBase64String(guidBytes);
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
            ?.CreateEntity(entityData);
        return entity;
    }

    public static BehaviourBlueprint FindBehaviourBlueprint(BehaviourData behaviourData)
    {
        return GameManager.Instance.GlobalData.BehaviourDatasContainer.GetBehaviourBlueprint(behaviourData);
    }
   
    public static StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, List<Entity> entities, Entity target)
    {
        return entities
        .Where(entity => entity.guid == statusEffectData.EntityGuid)
        .SelectMany(entity => entity.Behaviours
            .Where(behaviour => behaviour.guid == statusEffectData.BehaviourGuid)
            .Select(matchedBehaviour =>
            {
                var statusEffectBlueprint = GameManager.Instance.GlobalData.StatusEffectBlueprintsContainer.GetStatusEffectBlueprint(statusEffectData);
                return statusEffectBlueprint?.CreateStatusEffect(statusEffectData, matchedBehaviour, target);
            }))
        .FirstOrDefault();
    }

    public static Map CreateMap()
    {
        return GameManager.Instance.GlobalData.MapBlueprints[0].CreateMap();
    }

    public static MapData CreateMapData(Game game)
    {
        MapData mapData = new();

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
                    {
                        Tile tile = game.map.GetTile(pair.Key);
                        tile.AddEntity(entity);
                        entity.OnPlaced?.Invoke(tile);
                    }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game
{
    public string GUID;
    public Map map;
    public List<Player> players;
    public RandomGenerator randomGenerator;

    public Game()
    {
        players = new List<Player>();
        randomGenerator = new RandomGenerator();
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
        foreach (Player player in players)
        {
            foreach (var entity in player.entities)
            {
                entity.Update();
            }
        }
    }

    public void End()
    {

    }

    public List<Entity> GetAllEntities()
    {
        return players.SelectMany(player => player.entities).ToList();
    }

}

public class RandomGenerator
{
    private uint seed;
    private uint initialSeed;
    private long sequencePosition;

    const int Max = 100;
    const int Min = 1;

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

    public int NextInt()
    {
        double value = Next();
        return (int)(value * (Max - Min + 1)) + Min;
    }
    public float NextFloat()
    {
        return (float)Next();
    }

    public uint GetSeed() => initialSeed;
}

public class RandomGeneratorState
{
    public uint InitialSeed { get; set; }
    public long CurrentPosition { get; set; }
}


public class GameFactory
{
    // Create Game from Game State Date
    public Game CreateGame(GameData gameData)
    {
        Game game = new Game();
        game.GUID = gameData.GUID;
        game.randomGenerator = new RandomGenerator(gameData.randomState);

        foreach (var playerData in gameData.PlayersData)
        {
            Player player = CreatePlayer(playerData);
            game.AddPlayer(player);
        }

        game.map = CreateMap(gameData.MapData);

        PlaceEntitiesOnTiles(gameData.MapData.entityPositions, game);

        return game;
    }
    public Player CreatePlayer(PlayerData playerData)
    {
        Player player = new Player();
        player.clientId = playerData.Id;
        player.team = playerData.Team;

        foreach (var entityData in playerData.EntityData)
        {
            Entity entity = CreateEntity(entityData);
            player.AddEntity(entity);
        }

        return player;
    }

    public Entity CreateEntity(EntityData entityData)
    {
        Entity entity = GameManager.Instance.GlobalData.FractionBlueprints
            .SelectMany(f => f.EntityBlueprints)
            .FirstOrDefault(e => e.Id == entityData.Id)
            ?.CreateEntity();
        entity.FillWithData(entityData);
        return entity;
    }

    public Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        Behaviour behaviour = CreateBehaviourByBlueprintId(behaviourData.Id);
        behaviour.FillWithData(behaviourData);
        return behaviour;
    }

    public Map CreateMap(MapData mapData)
    {
        return GameManager.Instance.GlobalData.MapBlueprints
               .FirstOrDefault(data => data.Id == mapData.Id)
               ?.CreateMap();
    }

    // Create Game State Datu from Game
    public GameData CreateGameData(Game game)
    {
        GameData gameData = new();
        gameData.GUID = game.GUID;
        gameData.randomState = game.randomGenerator.GetState();

        gameData.MapData = CreateMapData(game);

        foreach (var player in game.players)
            gameData.PlayersData.Add(CreatePlayerData(player));

        return gameData;
    }

    public MapData CreateMapData(Game game)
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

    public PlayerData CreatePlayerData(Player player)
    {
        PlayerData playerData = new()
        {
            Id = player.clientId,
            Team = player.team,
        };

        foreach (var entity in player.entities)
            playerData.EntityData.Add(entity.GetEntityData());

        return playerData;
    }

    private void PlaceEntitiesOnTiles(Dictionary<Vector2Int, List<string>> entityPositions, Game game)
    {
        foreach (var pair in entityPositions)
        {
            foreach (var player in game.players)
                foreach (var entity in player.entities)
                    if (pair.Value.Contains(entity.guid))
                        game.map.GetTile(pair.Key).AddEntity(entity);
        }
    }

    // Create Entities from Entity Scriptable Objects (THIS WILL WORK UNLESS WE DOSENT HAVE ONLY ONE FIGURE TYPE PER FRACTION)

    public Figure CreateFigure(FractionType fractionType, FigureType figureType)
    {
        var fractionData = GameManager.Instance.GlobalData.FractionBlueprints
       .FirstOrDefault(f => f.FractionType == fractionType);

        if (fractionData == null) return null;

        var figureData =  fractionData.EntityBlueprints.Cast<FigureBlueprint>()
       .FirstOrDefault(e => e.FigureType == figureType);

        if (figureData == null) return null;

        return figureData.CreateEntity() as Figure;
    }

    public Behaviour CreateBehaviourByBlueprintId(string blueprintId)
    {
        BehaviourBlueprintsContainer behaviourContainer = GameManager.Instance.GlobalData.BehaviourDatasContainer;
        Behaviour behaviour = behaviourContainer
            .MovementBehaviourBlueprints
            .Concat(behaviourContainer.AttackBehaviourBlueprints)
            .Concat(behaviourContainer.DamageableBehaviourBlueprints)
            .Concat(behaviourContainer.AbilityBehaviourBlueprints)
            .FirstOrDefault(b => b.Id == blueprintId)
            ?.CreateBehaviour();
        return behaviour;
    }
}






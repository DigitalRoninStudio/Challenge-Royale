using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : INetworkService
{
    public static bool IsServer { get; private set; } = false;
    public NativeList<NetworkConnection> Connections => connections;

    private float keepAlive;
    private float lastKeepAlive;
    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private NetworkSettings settings;
    private NetworkPipelineService pipelineService;

    public Server(NetworkSettings settings, float keepAlive)
    {
        this.settings = settings;
        this.keepAlive = keepAlive;
    }
    public void StartServer(ushort port, ushort maxNumberOfClients = 16)
    {
        driver = NetworkDriver.Create(settings);
        pipelineService = new NetworkPipelineService(driver);

        NetworkEndpoint endPoint = NetworkEndpoint.AnyIpv4;
        endPoint.Port = port;

        if(driver.Bind(endPoint) != 0)
        {
            NetworkLogger.Log("Unable to bind on port: " + endPoint.Port);
        }
        else
        {
            driver.Listen();
            connections = new NativeList<NetworkConnection>(maxNumberOfClients, Allocator.Persistent);

            IsServer = true;

            NetworkLogger.Log("Currently listening on port: " + endPoint.Port);
        }
    }
    public void Update()
    {
        if (!IsServer) return;

        driver.ScheduleUpdate().Complete();

        AcceptNewConnections();
        CleanupConnections();
        UpdateMessagePump();
        KeepAlive();
    }

    public void Shutdown()
    {
        if (driver.IsCreated)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i].IsCreated)
                {
                    connections[i].Disconnect(driver);
                    connections[i] = default;
                }
            }
            driver.Dispose();
            connections.Dispose();
            NetworkLogger.Log("Server has shut down");
        }
    }

    public void Dispose()
    {
        Shutdown();
    }

    public NetworkPipeline GetPipeline(Pipeline pipeline)
    {
        switch (pipeline)
        {
            case Pipeline.Reliable:
                return pipelineService.Reliable;
            case Pipeline.Unreliable:
                return pipelineService.Unreliable;
            case Pipeline.Fragmentation:
                return pipelineService.Fragmentation;
        }
        return default;
    }

    public NetworkDriver GetDriver()
    {
        return driver;
    }
    private void AcceptNewConnections()
    {
        NetworkConnection connection;
        while ((connection = driver.Accept()) != default)
        {
            connections.Add(connection);

            Sender.ServerSendData(connection, new NetWelcome(), Pipeline.Reliable);

            NetworkLogger.Log("Client connected on server");
        }
    }
    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    private void UpdateMessagePump()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    // Handle data received from client
                    OnDataReceived(stream, connections[i]);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    connections[i] = default;
                    NetworkLogger.Log("Client disconnected");
                }
            }
        }
    }
    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive > keepAlive)
        {
            lastKeepAlive = Time.time;
            Sender.ServerSendDataToAll(new NetKeepAlive(), Pipeline.Reliable);
        }
    }
    private static void OnDataReceived(DataStreamReader reader, NetworkConnection connection)
    {
        NetMessage msg = null;
        OpCode opCode = (OpCode)reader.ReadByte();

        switch (opCode)
        {
            case OpCode.ON_KEEP_ALIVE:
                msg = new NetKeepAlive(reader);
                break;
            case OpCode.ON_WELCOME:
                msg = new NetWelcome(reader);
                break;
            default:
                break;
        }

        if (msg != null)
            msg.ReceivedOnServer(connection);
    }
}

public class Match
{
    public readonly string id;
    public List<Player> players;
    public Map map;

    public Match()
    {
        this.id = GameManager.GenerateId();
    }

}

public class Player
{
    public readonly string id;
    public List<Unit> units;
    private readonly string matchId;

    public Match Match => GameManager.Instance.GetMatchById(matchId);

    public Player(string matchId)
    {
        this.matchId = matchId;
    }
}
public enum ClassType
{
    None = 0,
    Light = 1,
    Dark = 2,
}

public enum UnitType
{
    Swordsman = 0,
    Knight = 1,
    Tank = 2,
    Archer = 3,
    Wizard = 4,
    Jester = 5,
    Queen = 6,
    King = 7,

}
public class Unit
{
    public readonly string id;
    private readonly string matchId;
    private readonly string ownerId;

    public Player Owner => GameManager.Instance.GetPlayerInMatch(matchId, ownerId);

    public Unit(string matchId, string ownerId, Tile tile, ClassType class_type, UnitType unit_type)
    {
        this.id = GameManager.GenerateId();
        this.matchId = matchId;
        this.ownerId = ownerId;
    }
}

public abstract class Entity
{
    public readonly string id;
    public bool IsBlockingMovement;
}

public abstract class Map
{
    protected int numberOfColumns;
    protected int numberOfRows;
    protected float offset;
    protected float tileSize;
    protected Dictionary<Vector2Int, Tile> tiles;
    public Map(int column, int row, float offset, float tileSize)
    {
        numberOfColumns = column;
        numberOfRows = row;
        this.offset = offset;
        this.tileSize = tileSize;
        tiles = new Dictionary<Vector2Int, Tile>();
        CreateMap(column, row);
    }

    public Tile GetTile(Vector2Int coordinate)
    {
        return tiles.ContainsKey(coordinate) ? tiles[coordinate] : null;
    }

    public Tile GetTile(int column, int row)
    {
        Vector2Int coordinate = new Vector2Int(column, row);
        return tiles.ContainsKey(coordinate) ? tiles[coordinate] : null;
    }

    public Tile GetTile(Entity entity)
    {
        foreach (var tile in tiles.Values) 
            if(tile.GetEntities().Contains(entity))
                return tile;

        return null;
    }

    public abstract void CreateMap(int column, int row);

    public abstract Tile OnHoverMapGetTile(Vector2 mousePosition);
    public abstract List<Vector2Int> GetNeighborsVectors();
    public abstract List<Vector2Int> GetDiagonalsNeighborsVectors();
    public abstract List<Tile> TilesInRange(Tile tile, int range);
    public List<Tile> GetTilesInDirection(Direction direction, Tile tile, int range, bool includeUnwalkableTiles = true)
    {
        List<Tile> directionTiles = new List<Tile>();
        Vector2Int directionCoordinate = DirectionToCoordinate(direction);

        for (int i = 1; i < range + 1; i++)
        {
            Tile tileInDirection = GetTile(
                i * directionCoordinate.x + tile.coordinate.x,
                i * directionCoordinate.y + tile.coordinate.y);

            if (tileInDirection != null)
                if (includeUnwalkableTiles || tileInDirection.IsWalkable())
                    directionTiles.Add(tileInDirection);
        }
        return directionTiles;
    }
    public abstract Vector2Int DirectionToCoordinate(Direction direction);
    public abstract Direction CoordinateToDirection(Vector2Int coordinate);

    public void Draw()
    {
        foreach (var tile in tiles)
            tile.Value.Draw(tileSize);
    }
}

public abstract class Tile
{
    public readonly Vector2Int coordinate;
    protected List<Entity> entities;
    protected List<Tile> neighbors;
    protected Vector3 position;

    public Tile(Vector2Int coordinate, Vector3 position)
    {
        this.coordinate = coordinate;
        entities = new List<Entity>();
        neighbors = new List<Tile>();
        this.position = position;
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }

    public void RemoveEntity(Entity entity) 
    {
        entities.Remove(entity);
    }

    public T TryToGetEntityOfType<T>() where T : Entity
    {
        foreach (var entity in entities)
            if (entity is T type)
                return type;

        return null;
    }

    public bool IsWalkable()
    {
        foreach(var entity in entities)
            if(entity.IsBlockingMovement)
                return false;

        return true;
    }
    public List<Entity> GetEntities() { return entities; }

    public void SetNeighbors(Map map)
    {
        foreach (Vector2Int neigborVector in map.GetNeighborsVectors())
        {
            Tile _neigbor_tile = map.GetTile(coordinate.x + neigborVector.x, coordinate.y + neigborVector.y);
            if (_neigbor_tile != null)
                neighbors.Add(_neigbor_tile);
        }
    }

    public List<Tile> GetNeighbors() { return neighbors; }

    public void Draw(float tileSize)
    {
        Vector3[] corners = Corners(tileSize);
        for (int i = 0; i < corners.Length - 1; i++)
            Debug.DrawLine(corners[i], corners[i + 1], Color.red);
        Debug.DrawLine(corners[corners.Length - 1], corners[0], Color.red);
    }
    public abstract Vector3[] Corners(float tileSize);
    protected abstract Vector3 Corner(int index, float tileSize);
}
public class Hex : Tile
{
    public int S { set; get; }
    public Hex(Vector2Int coordinate, Vector3 position) : base(coordinate, position)
    {
        S = -coordinate.x - coordinate.y;
    }
    public static float OuterRadius(float size) { return size; }
    public static float InnerRadius(float size) { return size * Mathf.Sqrt(3) / 2f; }
    public static float Width(float size) { return size * 2f; }
    public static float Height(float size) { return size * Mathf.Sqrt(3); }
    public override Vector3[] Corners(float size)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < corners.Length; i++)
            corners[i] = Corner(i, size);
        return corners;
    }
    protected override Vector3 Corner(int index, float size)
    {
        float angle = 60f * index;
        Vector3 corner = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) * OuterRadius(size) + position;
        return corner;
    }
}
public class Square : Tile
{
    public Square(Vector2Int coordinate, Vector3 position) : base(coordinate, position) { }

    public static float OuterRadius(float size) { return Mathf.Sqrt(2 * Mathf.Pow(size, 2)); }
    public static float InnerRadius(float size) { return size / 2f; }
    public static float Width(float size) { return size * 2; }
    public static float Height(float size) { return size * 2; }
    public override Vector3[] Corners(float size)
    {
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < corners.Length; i++)
            corners[i] = Corner(i, size);
        return corners;
    }

    protected override Vector3 Corner(int index, float size)
    {
        float angle = 90f * index + 45f;
        Vector3 corner = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) *
             OuterRadius(size) + position;
        return corner;
    }
}
public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    UPPER_RIGHT,
    UPPER_LEFT,
    LOWER_RIGHT,
    LOWER_LEFT
}

public class HexagonMap : Map
{
    public static readonly List<Vector2Int> neighborsVectors = new List<Vector2Int> {
        new Vector2Int(0, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, -1)};

    public static readonly List<Vector2Int> diagonalsNeighborsVectors = new List<Vector2Int>() {
        new Vector2Int(2, -1),
        new Vector2Int(1, -2),
        new Vector2Int(-1, -1),
        new Vector2Int(-2, 1),
        new Vector2Int(-1, 2),
        new Vector2Int(1, 1)};

    public static readonly Dictionary<Vector2Int, Direction> coordinateToDirection = new Dictionary<Vector2Int, Direction>
    {
        { new Vector2Int(0, 1), Direction.UP },
        { new Vector2Int(0, -1), Direction.DOWN },
        { new Vector2Int(1, -1), Direction.UPPER_RIGHT },
        { new Vector2Int(-1, 0), Direction.UPPER_LEFT },
        { new Vector2Int(1, 0), Direction.LOWER_RIGHT },
        { new Vector2Int(-1, 1), Direction.LOWER_LEFT }
    };

    public static readonly Dictionary<Direction, Vector2Int> directionToCoordinate = new Dictionary<Direction, Vector2Int>
    {
        { Direction.UP, new Vector2Int(0, 1) },
        { Direction.DOWN, new Vector2Int(0, -1) },
        { Direction.UPPER_RIGHT, new Vector2Int(1, -1) },
        { Direction.UPPER_LEFT, new Vector2Int(-1, 0) },
        { Direction.LOWER_RIGHT, new Vector2Int(1, 0) },
        { Direction.LOWER_LEFT, new Vector2Int(-1, 1) }
    };

    public HexagonMap(int column, int row, float offset, float tileSize) : base(column, row, offset, tileSize) { }

    public override void CreateMap(int column, int row)
    {
        Vector3 pos = Vector3.zero;

        for (int c = -column; c <= column; c++)
        {
            int r1 = Mathf.Max(-column, -c - column);
            int r2 = Mathf.Min(column, -c + column);

            for (int r = r1; r <= r2; r++)
            {
                pos.x = 3f/4f * Hex.Width(tileSize) * c * offset;
                pos.y = Hex.Height(tileSize) * (r + c / 2f) * offset;

                Hex hex = new Hex(new Vector2Int(c, r), pos);
                tiles.Add(new Vector2Int(c, r), hex);
            }
        }

        foreach (Tile tile in tiles.Values)
            tile.SetNeighbors(this);
    }

    public override Direction CoordinateToDirection(Vector2Int coordinate)
    {
        if (coordinateToDirection.TryGetValue(coordinate, out Direction direction))
            return direction;

        throw new ArgumentException("Invalid coordinates");
    }
    public override Vector2Int DirectionToCoordinate(Direction direction)
    {
        if (directionToCoordinate.TryGetValue(direction, out Vector2Int coordinate))
            return coordinate;

        throw new ArgumentException("Invalid direction");
    }

    public override List<Vector2Int> GetDiagonalsNeighborsVectors()
    {
        return diagonalsNeighborsVectors;
    }

    public override List<Vector2Int> GetNeighborsVectors()
    {
        return neighborsVectors;
    }

    public override Tile OnHoverMapGetTile(Vector2 mousePosition)
    {
        return null;
    }
    public override List<Tile> TilesInRange(Tile tile, int range)
    {
        List<Tile> listOfTiles = new List<Tile>();

        for (int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
            {
                Vector2Int neighborOffset = new Vector2Int(q, r);
                Tile neighborInRange = GetTile(tile.coordinate + neighborOffset);
                if (neighborInRange != null)
                    listOfTiles.Add(neighborInRange);
            }
        }

        return listOfTiles;
    }
}

public class SquareMap : Map
{
    public static readonly List<Vector2Int> neighborsVectors = new List<Vector2Int>
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    public static readonly List<Vector2Int> diagonalsNeighborsVectors = new List<Vector2Int>
    {
        new Vector2Int(1, 1), 
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, -1) 
    };

    public static readonly Dictionary<Vector2Int, Direction> coordinateToDirection = new Dictionary<Vector2Int, Direction>
    {
        { new Vector2Int(0, 1), Direction.UP },
        { new Vector2Int(0, -1), Direction.DOWN },
        { new Vector2Int(1, 0), Direction.RIGHT },
        { new Vector2Int(-1, 0), Direction.LEFT },
        { new Vector2Int(1, 1), Direction.UPPER_RIGHT },
        { new Vector2Int(-1, 1), Direction.UPPER_LEFT },
        { new Vector2Int(1, -1), Direction.LOWER_RIGHT },
        { new Vector2Int(-1, -1), Direction.LOWER_LEFT }
    };

    public static readonly Dictionary<Direction, Vector2Int> directionToCoordinate = new Dictionary<Direction, Vector2Int>
    {
        { Direction.UP, new Vector2Int(0, 1) },
        { Direction.DOWN, new Vector2Int(0, -1) },
        { Direction.RIGHT, new Vector2Int(1, 0) },
        { Direction.LEFT, new Vector2Int(-1, 0) },
        { Direction.UPPER_RIGHT, new Vector2Int(1, 1) },
        { Direction.UPPER_LEFT, new Vector2Int(-1, 1) },
        { Direction.LOWER_RIGHT, new Vector2Int(1, -1) },
        { Direction.LOWER_LEFT, new Vector2Int(-1, -1) }
    };

    public SquareMap(int column, int row, float offset, float tileSize) : base(column, row, offset, tileSize) { }

    public override void CreateMap(int column, int row)
    {
        Vector3 pos = Vector3.zero;
        Vector2Int center = new Vector2Int(column / 2, row / 2);

        for (int c = -center.x; c <= center.x; c++)
        {
            for (int r = -center.y; r <= center.y; r++)
            {
                pos.x = (center.x + c) * Square.Width(tileSize) * offset;
                pos.y = (center.y + r) * Square.Height(tileSize) * offset;

                Square square = new Square(new Vector2Int(center.x + c, center.y + r), pos);
                tiles.Add(new Vector2Int(center.x + c, center.y + r), square);
            }
        }

        foreach (Square tile in tiles.Values)
            tile.SetNeighbors(this);
    }

    public override Direction CoordinateToDirection(Vector2Int coordinate)
    {
        if (coordinateToDirection.TryGetValue(coordinate, out Direction direction))
            return direction;

        throw new ArgumentException("Invalid coordinates");
    }

    public override Vector2Int DirectionToCoordinate(Direction direction)
    {
        if (directionToCoordinate.TryGetValue(direction, out Vector2Int coordinate))
            return coordinate;

        throw new ArgumentException("Invalid direction");
    }

    public override List<Vector2Int> GetDiagonalsNeighborsVectors()
    {
        return diagonalsNeighborsVectors;
    }

    public override List<Vector2Int> GetNeighborsVectors()
    {
        return neighborsVectors;
    }
    public override Tile OnHoverMapGetTile(Vector2 mousePosition)
    {
        return null;
    }
    public override List<Tile> TilesInRange(Tile tile, int range)
    {
        List<Tile> listOfTiles = new List<Tile>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (Mathf.Abs(dx) + Mathf.Abs(dy) <= range)
                {
                    Vector2Int neighborOffset = new Vector2Int(dx, dy);
                    Tile neighborInRange = GetTile(tile.coordinate + neighborOffset);
                    if (neighborInRange != null)
                        listOfTiles.Add(neighborInRange);
                }
            }
        }

        return listOfTiles;
    }
}
public class GameManager : Singleton<GameManager>
{
    public Dictionary<string, Match> matches = new Dictionary<string, Match>();

    public Match GetMatchById(string matchId)
    {
        return matches.ContainsKey(matchId) ? matches[matchId] : null;
    }
    public Player GetPlayerInMatch(string matchId, string playerId)
    {
        var match = GetMatchById(matchId);
        return match?.players.FirstOrDefault(p => p.id == playerId);
    }

    public Map GetMapFromMatch(string matchId)
    {
        return GetMatchById(matchId).map;
    }
    public static string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}



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
/*
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
}*/



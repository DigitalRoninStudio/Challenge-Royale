using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : INetworkService
{
    public static bool IsServer { get; private set; } = false;

    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private NetworkSettings settings;

    public Server(NetworkSettings settings)
    {
        this.settings = settings;
    }
    public void StartServer(ushort port, ushort maxNumberOfClients = 16)
    {
        driver = NetworkDriver.Create(settings);

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
        //keep alive
        AcceptNewConnections();
        CleanupConnections();
        UpdateMessagePump();
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
    private void AcceptNewConnections()
    {
        NetworkConnection connection;
        while ((connection = driver.Accept()) != default)
        {
            connections.Add(connection);
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
                    //OnDataReceived(stream, connections[i]);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    connections[i] = default;
                    NetworkLogger.Log("Client disconnected");
                }
            }
        }
    }
}

public class NetworkPipelineService
{
    public NetworkPipeline UDP { get; private set; }
    public NetworkPipeline TCP { get; private set; }
    public NetworkPipeline Fragmentation { get; private set; }

    public NetworkPipelineService(NetworkDriver driver)
    {
        UDP = driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
        TCP = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        Fragmentation = driver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
    }
}

public interface INetworkPipelineService
{
    public NetworkPipeline GetUnreliablePipeline();
    public NetworkPipeline GetReliablePipeline();
    public NetworkPipeline GetFragmentationPipeline();
}

using Unity.Collections;
using Unity.Networking.Transport;

public class Server : INetworkService
{
    public static bool IsServer { get; private set; } = false;
    public NativeList<NetworkConnection> Connections => connections;

    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private NetworkSettings settings;
    private NetworkPipelineService pipelineService;

    public Server(NetworkSettings settings)
    {
        this.settings = settings;
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




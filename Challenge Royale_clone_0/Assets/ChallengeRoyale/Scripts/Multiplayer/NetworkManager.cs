using Unity.Networking.Transport;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{    
    [SerializeField] private NetworkConfigParameter configParameter;

    [Space(25)]
    [SerializeField] private string ip = "127.0.0.1";
    [SerializeField] private ushort port = 9000;
    [SerializeField] private ushort maxNumberOfClients = 16;
    [SerializeField] private float keepAlive = 5f;
    [SerializeField] private bool activeNetworkLogger = true;

    public INetworkService NetworkService => networkService;
    private INetworkService networkService;

    public void StartServer()
    {
        if (!configParameter.Validate()) return;

        NetworkSettings settings = CreateNetworkSettings();
        Server server = new Server(settings, keepAlive);
        server.StartServer(port, maxNumberOfClients);
        networkService = server;

        NetworkLogger.IsDebugEnabled = activeNetworkLogger;
    }

    public void ConnectToServer()
    {
        if (!configParameter.Validate()) return;

        NetworkSettings settings = CreateNetworkSettings();
        Client client = new Client(settings);
        client.ConnectToServer(ip, port);
        networkService = client;

        NetworkLogger.IsDebugEnabled = activeNetworkLogger;
    }
    
    private void Update()
    {
        networkService?.Update();
    }

    private void OnDestroy()
    {
        networkService?.Dispose();
    }
    private NetworkSettings CreateNetworkSettings()
    {
        var settings = new NetworkSettings();
        settings.WithNetworkConfigParameters(
            connectTimeoutMS: configParameter.connectTimeoutMS,
            maxConnectAttempts: configParameter.maxConnectAttempts,
            disconnectTimeoutMS: configParameter.disconnectTimeoutMS,
            heartbeatTimeoutMS: configParameter.heartbeatTimeoutMS,
            reconnectionTimeoutMS: configParameter.reconnectionTimeoutMS,
            maxFrameTimeMS: configParameter.maxFrameTimeMS,
            fixedFrameTimeMS: configParameter.fixedFrameTimeMS,
            receiveQueueCapacity: configParameter.receiveQueueCapacity,
            sendQueueCapacity: configParameter.sendQueueCapacity,
            maxMessageSize: configParameter.maxMessageSize
        );
        return settings;
    }
}

using Unity.Networking.Transport;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{    
    [SerializeField] private NetworkConfigParameter configParameter;

    [Space(25)]
    [SerializeField] private string ip = "127.0.0.1";
    [SerializeField] private ushort port = 9000;
    [SerializeField] private ushort maxNumberOfClients = 16;
    [SerializeField] private bool activeNetworkLogger = true;

    private INetworkService NetworkService;

    [ContextMenu("Start Server")]
    public void StartServer()
    {
        if (!configParameter.Validate()) return;

        NetworkSettings settings = CreateNetworkSettings();
        Server server = new Server(settings);
        server.StartServer(port, maxNumberOfClients);
        NetworkService = server;

        NetworkLogger.IsDebugEnabled = activeNetworkLogger;
    }

    [ContextMenu("Connect to Server")]
    public void ConnectToServer()
    {
        if (!configParameter.Validate()) return;

        NetworkSettings settings = CreateNetworkSettings();
        Client client = new Client(settings);
        client.ConnectToServer(ip, port);
        NetworkService = client;

        NetworkLogger.IsDebugEnabled = activeNetworkLogger;
    }

    private void Update()
    {
        NetworkService?.Update();
    }

    private void OnDestroy()
    {
        NetworkService?.Dispose();
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

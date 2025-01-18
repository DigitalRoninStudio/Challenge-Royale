using System.Diagnostics;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

public static class Sender
{    
    #region Client
    public static void ClientSendData(NetMessage msg, Pipeline pipeline)
    {
        INetworkService networkService = NetworkManager.Instance.NetworkService;

        if (networkService == null) return;

        if (Client.IsClient && networkService is Client client)
            SendDataByClient(networkService.GetDriver(), networkService.GetPipeline(pipeline), client.Connection, msg);
        else
            NetworkLogger.Log("Client trying to send a message to the Server but the Client has not been created");

    }
    private static void SendDataByClient(NetworkDriver driver, NetworkPipeline pipeline, NetworkConnection connection, NetMessage msg)
    {
        driver.BeginSend(pipeline, connection, out DataStreamWriter writer);
        writer.WriteByte((byte)msg.Code);
        msg.Serialize(ref writer);
        StatusCode status = (StatusCode)driver.EndSend(writer);
        if (status == StatusCode.NetworkSendQueueFull)
            NetworkLogger.Log("ERROR: Copy your message to a queue, and try resending later");
    }
    #endregion
    #region Server
    public static void ServerSendData(NetworkConnection connection, NetMessage msg, Pipeline pipeline)
    {
        INetworkService networkService = NetworkManager.Instance.NetworkService;

        if (networkService == null) return;

        if (Server.IsServer)
            SendDataToClientByServer(networkService.GetDriver(), networkService.GetPipeline(pipeline), connection, msg);
        else
            NetworkLogger.Log("Server trying to send a message to the Client but the Server has not been created");
    }
    public static void ServerSendDataToAll(NetMessage msg, Pipeline pipeline)
    {
        INetworkService networkService = NetworkManager.Instance.NetworkService;

        if (networkService == null) return;

        if (Server.IsServer && networkService is Server server)
            SendDataToAllClientsByServer(networkService.GetDriver(), networkService.GetPipeline(pipeline), server.Connections, msg);
        else
            NetworkLogger.Log("Server trying to send a message to the All Clients but the Server has not been created");
    }
    public static void ServerSendDataToAllExceptToGivenOne(NetworkConnection excluded, NetMessage msg, Pipeline pipeline)
    {
        INetworkService networkService = NetworkManager.Instance.NetworkService;

        if (networkService == null) return;

        if (Server.IsServer && networkService is Server server)
            SendDataToAllClientsExceptToGivenOneByServer(networkService.GetDriver(), networkService.GetPipeline(pipeline), excluded, server.Connections, msg);
        else
            NetworkLogger.Log("Server trying to send a message to the All Clients except one Client but the Server has not been created");
    }
    private static void SendDataToClientByServer(NetworkDriver driver, NetworkPipeline pipeline, NetworkConnection connection, NetMessage msg)
    {
        driver.BeginSend(pipeline, connection, out DataStreamWriter writer);
        if (!writer.IsCreated) return;
        writer.WriteByte((byte)msg.Code);
        msg.Serialize(ref writer);
        StatusCode status = (StatusCode)driver.EndSend(writer);
        if (status == StatusCode.NetworkSendQueueFull)
            NetworkLogger.Log("ERROR: Copy your message to a queue, and try resending later");
    }
    private static void SendDataToAllClientsByServer(NetworkDriver driver, NetworkPipeline pipeline, NativeList<NetworkConnection> connections, NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
            if (connections[i].IsCreated)
                SendDataToClientByServer(driver, pipeline, connections[i], msg);
    }
    private static void SendDataToAllClientsExceptToGivenOneByServer(NetworkDriver driver, NetworkPipeline pipeline, NetworkConnection excluded, NativeList<NetworkConnection> connections, NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
            if (connections[i].IsCreated && connections[i] != excluded)
                SendDataToClientByServer(driver, pipeline, connections[i], msg);
    }
    #endregion
}


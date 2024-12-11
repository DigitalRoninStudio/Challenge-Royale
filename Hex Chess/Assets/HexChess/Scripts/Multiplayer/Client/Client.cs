using System;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : INetworkService
{
    public static bool IsClient { get; private set; } = false;
    public static Action<NetworkConnection> OnClientConnected;
    public static Action<NetworkConnection> OnClientDisconnected;
    public NetworkConnection Connection => connection;

    private NetworkDriver driver;
    private NetworkConnection connection;
    private NetworkSettings settings;
    private NetworkPipelineService pipelineService;

    private Receiver receiver;

    public Client(NetworkSettings settings)
    {
        this.settings = settings;
    }
    public void ConnectToServer(string ip, ushort port)
    {
        driver = NetworkDriver.Create(settings);
        pipelineService = new NetworkPipelineService(driver);

        NetworkEndpoint endPoint = NetworkEndpoint.Parse(ip, port);
        connection = driver.Connect(endPoint);

        receiver = new Receiver();
        receiver.Subscribe();

        NetworkLogger.Log("Attempting to connect to server at " + ip + ":" + port);
    }

    public void Update()
    {
        driver.ScheduleUpdate().Complete();

        UpdateMessagePump();
    }

    private void UpdateMessagePump()
    {
        if (!connection.IsCreated) return;

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out reader)) != NetworkEvent.Type.Empty)
        {
            if (!connection.IsCreated) break;

            if (cmd == NetworkEvent.Type.Connect)
            {
                IsClient = true;
                OnClientConnected?.Invoke(connection);
                NetworkLogger.Log("Connected to server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnDataReceived(reader);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                IsClient = false;
                OnClientDisconnected?.Invoke(connection);
                connection = default;

                NetworkLogger.Log("Disconnected from server \nREASON: " +
                   (Unity.Networking.Transport.Error.DisconnectReason)reader.ReadByte());
                NetworkLogger.Log("Client got disconnected from server");

            }
        }
    }
    public void Disconnect()
    {
        if (connection.IsCreated)
        {
            connection.Disconnect(driver);
            connection = default;
            NetworkLogger.Log("Disconnected from server");
        }
    }
    public void Dispose()
    {
        IsClient = false;
        driver.Dispose();
        receiver.Unsubscribe();
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
    private static void OnDataReceived(DataStreamReader reader)
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
            case OpCode.ON_GAME_ACTION:
                msg = new NetGameAction(reader);
                break;
            case OpCode.ON_SYNC_GAME:
                msg = new NetSyncGame(reader);
                break;
            default:
                break;
        }

        if (msg != null)
            msg.ReceivedOnClient();
    }

    public class Receiver
    {
        public void Subscribe()
        {
            C_ON_KEEP_ALIVE_RESPONESS += OnKeepAliveResponess;
            C_ON_WELCOME_RESPONESS += OnWelcomeResponess;
            C_ON_GAME_ACTION_RESPONESS += OnGameActionResponess;
            C_ON_SYNC_GAME_RESPONESS += OnGameSyncResponess;

        }

        public void Unsubscribe()
        {
            C_ON_KEEP_ALIVE_RESPONESS -= OnKeepAliveResponess;
            C_ON_WELCOME_RESPONESS -= OnWelcomeResponess;
            C_ON_GAME_ACTION_RESPONESS -= OnGameActionResponess;
            C_ON_SYNC_GAME_RESPONESS -= OnGameSyncResponess;

        }

        private void OnGameSyncResponess(NetMessage message)
        {
            NetSyncGame responess = message as NetSyncGame;
            GameManager.Instance.CreateMatch(responess.gameData, true);
            Debug.Log(GameManager.Instance.GetGameJson(GameManager.Instance.GetFirstMatch()));
            NetworkLogger.Log("CLIENT RECEIVER GAME");
        }

        private void OnWelcomeResponess(NetMessage message)
        {
            NetSyncGame request = new NetSyncGame()
            {
                playerId = LocalPlayer.Instance.LocalClientID,
                matchId = GameManager.Instance.GameID
            };
            Sender.ClientSendData(request, Pipeline.Reliable);
        }

        private void OnKeepAliveResponess(NetMessage message)
        {
            Sender.ClientSendData(message, Pipeline.Reliable);
        }

        private void OnGameActionResponess(NetMessage message)
        {
            if (message is not NetGameAction responess)
            {
                Debug.LogWarning("Invalid message received.");
                return;
            }

            var match = GameManager.Instance.GetFirstMatch();
            if (match == null)
            {
                Debug.LogWarning("No active match found.");
                return;
            }

            switch (responess.ActionType)
            {
                case GameActionType.ON_BEHAVIOUR_ACTION:
                    HandleBehaviourAction(match, responess as NetBehaviourAction);
                    break;

                case GameActionType.ON_ROUND_ACTION:
                    HandleRoundAction(match, responess as NetRoundAction);
                    break;

                default:
                    Debug.LogWarning($"Unknown ActionType: {responess.ActionType}");
                    break;
            }
        }
        private void HandleBehaviourAction(Game game, NetBehaviourAction responess)
        {
            if (responess == null)
            {
                Debug.LogWarning("Invalid behaviour action received.");
                return;
            }

            var entity = game.players
                  .SelectMany(player => player.entities)
                  .FirstOrDefault(e => e.guid == responess.EntityGUID);

            if (entity == null)
            {
                Debug.LogWarning($"Entity with GUID {responess.EntityGUID} not found.");
                return;
            }

            var behaviour = entity.Behaviours
                .FirstOrDefault(b => b.guid == responess.BehaviourGUID);

            if (behaviour is INetAction action)
            {
                action.DeserializeAction(responess);
                game.actionController.AddActionToWork(behaviour as ILifecycleAction);
            }
        }

        private void HandleRoundAction(Game game, NetRoundAction roundAction)
        {
            if (roundAction == null)
            {
                Debug.LogWarning("Invalid round action received.");
                return;
            }

            RoundAction action = new RoundAction();
            action.DeserializeAction(roundAction);

            game.actionController.AddActionToWork(roundAction as ILifecycleAction);
        }        

        #region NetMessages Events
        public static Action<NetMessage> C_ON_KEEP_ALIVE_RESPONESS;
        public static Action<NetMessage> C_ON_WELCOME_RESPONESS;
        public static Action<NetMessage> C_ON_GAME_ACTION_RESPONESS;
        public static Action<NetMessage> C_ON_SYNC_GAME_RESPONESS;
        #endregion
    }

}

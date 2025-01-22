using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            case OpCode.ON_ACTION:
                msg = new NetAction(reader);
                break;
            case OpCode.ON_SYNC_GAME:
                msg = new NetSyncGame(reader);
                break;
            case OpCode.ON_CAST_ACTION:
                msg = new NetCastAction(reader);
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
            C_ON_ACTION_RESPONESS += OnActionResponess;
            C_ON_SYNC_GAME_RESPONESS += OnGameSyncResponess;
            C_ON_CAST_ACTION_RESPONESS += OnCastActionResponess;

        }

        public void Unsubscribe()
        {
            C_ON_KEEP_ALIVE_RESPONESS -= OnKeepAliveResponess;
            C_ON_WELCOME_RESPONESS -= OnWelcomeResponess;
            C_ON_ACTION_RESPONESS -= OnActionResponess;
            C_ON_SYNC_GAME_RESPONESS -= OnGameSyncResponess;
            C_ON_CAST_ACTION_RESPONESS -= OnCastActionResponess;

        }

        private void OnGameSyncResponess(NetMessage message)
        {
            NetSyncGame responess = message as NetSyncGame;
            GameManager.Instance.CreateMatch(responess.gameData, true);
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

        private void OnCastActionResponess(NetMessage message)
        {
            NetCastAction responess = message as NetCastAction;
            var match = GameManager.Instance.GetFirstMatch();
            foreach (var player in match.players)
            {
                if(player.clientId == responess.ClientId)
                {
                    player.AddCastAction(responess.BehaviourGUID);
                    player.energyController.DecreaseEnergy(responess.Amount);
                    break;

                }
            }
        }

        private void OnActionResponess(NetMessage message)
        {
            if (message is not NetAction responess)
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
                case ActionType.BEHAVIOUR:
                    HandleBehaviourAction(match, responess.Action);
                    break;

                case ActionType.ROUND:
                    HandleRoundAction(match, responess.Action);
                    break;

                default:
                    Debug.LogWarning($"Unknown ActionType: {responess.ActionType}");
                    break;
            }
        }
        private void HandleBehaviourAction(Game game, string actionJson)
        {
            BehaviourActionData actionData = JsonConvert.DeserializeObject<BehaviourActionData>(actionJson);
            if (actionData == null)
            {
                Debug.LogWarning("Invalid behaviour action received.");
                return;
            }

            var entity = game.players
                  .SelectMany(player => player.entities)
                  .FirstOrDefault(e => e.guid == actionData.EntityGUID);

            if (entity == null)
            {
                Debug.LogWarning($"Entity with GUID {actionData.EntityGUID} not found.");
                return;
            }

            var behaviour = entity.Behaviours
                .FirstOrDefault(b => b.guid == actionData.BehaviourGUID);

            if (behaviour is INetAction action)
            {
                action.DeserializeAction(actionJson);
                game.actionController.AddActionToWork(behaviour as ILifecycleAction);
            }
        }

        private void HandleRoundAction(Game game, string actionJson)
        {
            RoundActionData actionData = JsonConvert.DeserializeObject<RoundActionData>(actionJson);
            if (actionData == null)
            {
                Debug.LogWarning("Invalid round action received.");
                return;
            }

            RoundAction action = new RoundAction(game);
            action.DeserializeAction(actionJson);
            game.actionController.AddActionToWork(action);
        }        

        #region NetMessages Events
        public static Action<NetMessage> C_ON_KEEP_ALIVE_RESPONESS;
        public static Action<NetMessage> C_ON_WELCOME_RESPONESS;
        public static Action<NetMessage> C_ON_ACTION_RESPONESS; 
        public static Action<NetMessage> C_ON_SYNC_GAME_RESPONESS;
        public static Action<NetMessage> C_ON_CAST_ACTION_RESPONESS;
        #endregion
    }

}

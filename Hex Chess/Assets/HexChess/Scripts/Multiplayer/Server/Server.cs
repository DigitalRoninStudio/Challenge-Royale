using System;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class Server : INetworkService
{
    public static bool IsServer { get; private set; } = false;
    public static Action<NetworkConnection> OnClientConnected;
    public static Action<NetworkConnection> OnClientDisconnected;
    public NativeList<NetworkConnection> Connections => connections;

    private float keepAlive;
    private float lastKeepAlive;
    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private NetworkSettings settings;
    private NetworkPipelineService pipelineService;

    private Receiver receiver;

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

            receiver = new Receiver();
            receiver.Subscribe();

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
            receiver.Unsubscribe();
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

            OnClientConnected?.Invoke(connection);

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
                    OnClientDisconnected?.Invoke(connections[i]);
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
            case OpCode.ON_SYNC_GAME:
                msg = new NetSyncGame(reader);
                break;
            case OpCode.ON_MOVE:
                msg = new NetMovement(reader);
                break;
            case OpCode.ON_ATTACK:
                msg = new NetAttack(reader);
                break;
            case OpCode.ON_END_ROUND:
                msg = new NetEndRound(reader);
                break;
            case OpCode.ON_HAND_OVER_THE_INITIATIVE:
                msg = new NetHandOverTheInitiative(reader);
                break;
            default:
                break;
        }

        if (msg != null)
            msg.ReceivedOnServer(connection);
    }

    public class Receiver
    {
        public void Subscribe()
        {
            S_ON_SYNC_GAME_REQUEST += OnGameStateRequest;
            S_ON_MOVE_REQUEST += OnMoveRequest;
            S_ON_ATTACK_REQUEST += OnAttackRequest;
            S_ON_END_ROUND_REQUEST += OnEndRoundRequest;
            S_ON_HAND_OVER_THE_INITIATIVE_REQUEST += OnHandOverTheInitiativeRequest;
        }

        public void Unsubscribe()
        {
            S_ON_SYNC_GAME_REQUEST -= OnGameStateRequest;
            S_ON_MOVE_REQUEST -= OnMoveRequest;
            S_ON_ATTACK_REQUEST -= OnAttackRequest;
            S_ON_END_ROUND_REQUEST -= OnEndRoundRequest;
            S_ON_HAND_OVER_THE_INITIATIVE_REQUEST -= OnHandOverTheInitiativeRequest;
        }

        private void OnHandOverTheInitiativeRequest(NetMessage message, NetworkConnection connection)
        {
            NetHandOverTheInitiative request = message as NetHandOverTheInitiative;
            Game game = GameManager.Instance.GetMatch(request.MatchId);

            if (game == null)
            {
                NetworkLogger.Log("Client tried to end turn but GAME is null");
                return;
            }

            var clientId = GameManager.Instance.GetPlayerId(connection);
            if (string.IsNullOrEmpty(clientId))
            {
                NetworkLogger.Log("Client tried to end turn but CLIENTID is empty");
                return;
            }

            Player player = game.GetPlayer(clientId);

            if (player == null) return;

            if (!game.IsPlayerHasInitiation(player)) return;

            if (game.actionController.IsActionQueueEmpty())
                HandOverInitiationHandler();
            else
            {
                game.SendMessageToPlayers(new NetChangePlayerState() { ClientId = player.clientId, PlayerState = PlayerState.IDLE });
                game.actionController.OnActionQueueEmpty += HandOverInitiationHandler;
            }

            void HandOverInitiationHandler()
            {
                game.actionController.OnActionQueueEmpty -= HandOverInitiationHandler;
                game.roundController.SwitchInitiation();
                game.SendMessageToPlayers(new NetHandOverTheInitiative() { EndTurn = false });
            }
        }

        private void OnEndRoundRequest(NetMessage message, NetworkConnection connection)
        {
            NetEndRound request = message as NetEndRound;
            Game game = GameManager.Instance.GetMatch(request.MatchId);

            if (game == null)
            {
                NetworkLogger.Log("Client tried to end turn but GAME is null");
                return;
            }

            var clientId = GameManager.Instance.GetPlayerId(connection);
            if (string.IsNullOrEmpty(clientId))
            {
                NetworkLogger.Log("Client tried to end turn but CLIENTID is empty");
                return;
            }

            Player player = game.GetPlayer(clientId); 

            if (player == null) return;

            if (!game.IsPlayerHasInitiation(player)) return;

            if (game.actionController.IsActionQueueEmpty())
                EndRoundHandler();
            else
            {
                game.SendMessageToPlayers(new NetChangePlayerState() { ClientId = player.clientId, PlayerState = PlayerState.IDLE });
                game.actionController.OnActionQueueEmpty += EndRoundHandler;
            }

            void EndRoundHandler()
            {
                game.actionController.OnActionQueueEmpty -= EndRoundHandler;
                if (game.roundController.endTurnCalled)
                {
                    game.roundController.EndRound();
                    game.SendMessageToPlayers(new NetEndRound());
                }
                else
                {
                    game.roundController.EndRoundAndSwitchInitiation();
                    game.SendMessageToPlayers(new NetHandOverTheInitiative() { EndTurn = true });

                }
            }
        }

        private void OnAttackRequest(NetMessage message, NetworkConnection connection)
        {
            NetAttack request = message as NetAttack;
            Game game = GameManager.Instance.GetMatch(request.MatchId);

            if (game == null)
            {
                NetworkLogger.Log("Client tried to attack but GAME is null");
                return;
            }

            var clientId = GameManager.Instance.GetPlayerId(connection);
            if (string.IsNullOrEmpty(clientId))
            {
                NetworkLogger.Log("Client tried to attack but CLIENTID is empty");
                return;
            }

            var attackEntity = game.GetAllEntities().FirstOrDefault(e => e.guid == request.AttackerEntityId);
            if(attackEntity == null)
            {
                NetworkLogger.Log("Client tried to attack but ATTACKER ENTITY is null");
                return;
            }

            var attackBehaviour = attackEntity.Behaviours
                .OfType<AttackBehaviour>()
                .FirstOrDefault(b => b.guid == request.AttackBehaviourId);
            if(attackBehaviour == null)
            {
                NetworkLogger.Log("Entity tried to attack but ATTACK BEHAVIOUR is null");
                return;
            }

            var damagableEntity = game.GetAllEntities().FirstOrDefault(e => e.guid == request.DamagableEntityId);
            if (damagableEntity == null)
            {
                NetworkLogger.Log("Client tried to attack but DAMAGABLE ENTITY is null");
                return;
            }

            var damagableBehaviour = damagableEntity.Behaviours
                .OfType<DamageableBehaviour>()
                .FirstOrDefault(b => b.guid == request.DamagableBehaviourId);
            if (damagableBehaviour == null)
            {
                NetworkLogger.Log("Entity tried to attack but DAMAGABLE BEHAVIOUR is null");
                return;
            }

            if (attackBehaviour.CanAttack(damagableEntity))
            {
                attackBehaviour.SetAttack(damagableBehaviour);
                game.actionController.AddActionToWork(attackBehaviour);
            }
        }
        private void OnMoveRequest(NetMessage message, NetworkConnection connection)
        {
            NetMovement request = message as NetMovement;
            Game game = GameManager.Instance.GetMatch(request.MatchId);

            if (game == null)
            {
                NetworkLogger.Log("Client tried to move entity but GAME is null");
                return;
            }

            var clientId = GameManager.Instance.GetPlayerId(connection);
            if (string.IsNullOrEmpty(clientId))
            {
                NetworkLogger.Log("Client tried to move entity but CLIENTID is empty");
                return;
            }

            var entity = game.GetAllEntities().FirstOrDefault(e => e.guid == request.EntityId);
            if (entity == null)
            {
                NetworkLogger.Log("Client tried to move entity but ENTITY is null");
                return;
            }

            var movementBehaviour = entity.Behaviours
                 .OfType<MovementBehaviour>()
                 .FirstOrDefault(b => b.guid == request.MovementBehaviourId);
            if (movementBehaviour == null)
            {
                NetworkLogger.Log("Entity tried to move but MOVEMENT BEHAVIOUR is null");
                return;
            }

            var tile = game.map.GetTile(request.TileCoordinate);
            if(tile == null)
            {
                NetworkLogger.Log("Entity tried to move but TILE is null");
                return;
            }
            if (movementBehaviour.CanMove(tile))
            {
                movementBehaviour.SetPath(tile);
                game.actionController.AddActionToWork(movementBehaviour);
            }
        }
        private void OnGameStateRequest(NetMessage message, NetworkConnection connection)
        {
            NetSyncGame request = message as NetSyncGame;

            Game game = GameManager.Instance.GetMatch(request.matchId);

            if (game != null) 
            {
                //check here if player is created in game if its not add
                //serialize and send to the client
                bool playerExist = false;
                foreach (var player in game.players)
                {
                    if(player.clientId == request.playerId)
                    {
                        playerExist = true;
                        break;
                    }
                }
                
                GameManager.Instance.AddPlayer(request.playerId, connection);

                NetSyncGame responess = new NetSyncGame()
                {
                    gameData = GameManager.Instance.GetGameJson(game)
                };

                Sender.ServerSendData(connection, responess, Pipeline.Fragmentation);
            }
            else
            {
                NetworkLogger.Log($"PLAYER DOSENT HAVE MATCH");
            }

        }



        #region NetMessages Events
        public static Action<NetMessage, NetworkConnection> S_ON_SYNC_GAME_REQUEST;
        public static Action<NetMessage, NetworkConnection> S_ON_MOVE_REQUEST;
        public static Action<NetMessage, NetworkConnection> S_ON_ATTACK_REQUEST;
        public static Action<NetMessage, NetworkConnection> S_ON_END_ROUND_REQUEST;
        public static Action<NetMessage, NetworkConnection> S_ON_HAND_OVER_THE_INITIATIVE_REQUEST;
        #endregion
    }
}



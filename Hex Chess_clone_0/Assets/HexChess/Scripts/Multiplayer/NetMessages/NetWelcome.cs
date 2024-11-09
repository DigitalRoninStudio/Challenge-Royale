using Mono.Cecil.Cil;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetWelcome : NetMessage
{
    public NetWelcome()
    {
        Code = OpCode.ON_WELCOME;
    }
    public NetWelcome(DataStreamReader reader)
    {
        Code = OpCode.ON_WELCOME;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
    }
    public override void Deserialize(DataStreamReader reader)
    {
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_WELCOME_RESPONESS?.Invoke(this);
    }
}

public class NetGameAction : NetMessage
{
    public string entityGUID;
    public string behaviourGUID;
    public string serializedBehaviour;
    public NetGameAction()
    {
        Code = OpCode.ON_GAME_ACTION;
    }
    public NetGameAction(DataStreamReader reader)
    {
        Code = OpCode.ON_GAME_ACTION;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        WriteString(ref writer, entityGUID);
        WriteString(ref writer, behaviourGUID);

        WriteString(ref writer, serializedBehaviour);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        entityGUID = ReadString(ref reader);
        behaviourGUID = ReadString(ref reader);

        serializedBehaviour = ReadString(ref reader);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_GAME_ACTION_RESPONESS?.Invoke(this);
    }
}

public class NetSyncGame : NetMessage
{
    public string playerId;
    public string matchId;

    public string gameData;
    public NetSyncGame()
    {
        Code = OpCode.ON_SYNC_GAME;
    }
    public NetSyncGame(DataStreamReader reader)
    {
        Code = OpCode.ON_SYNC_GAME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        if(Client.IsClient)
        {
            WriteString(ref writer, playerId);
            WriteString(ref writer, matchId);
        }
        if(Server.IsServer)
        {
            WriteString(ref writer, gameData);
        }
    }
    public override void Deserialize(DataStreamReader reader)
    {
        if(Server.IsServer)
        {
            playerId = ReadString(ref reader);
            matchId = ReadString(ref reader);
        }
        if(Client.IsClient)
        {
            gameData = ReadString(ref reader);
        }
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_SYNC_GAME_RESPONESS?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
        Server.Receiver.S_ON_SYNC_GAME_REQUEST?.Invoke(this, connection);
    }
}

public class NetMovement : NetMessage
{
    public string MatchId;
    public string EntityId;
    public string MovementBehaviourId;
    public Vector2Int TileCoordinate;

    public NetMovement()
    {
        Code = OpCode.ON_MOVE;
    }
    public NetMovement(DataStreamReader reader)
    {
        Code = OpCode.ON_MOVE;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        WriteString(ref writer, MatchId);
        WriteString(ref writer, EntityId);
        WriteString(ref writer, MovementBehaviourId);
        writer.WriteInt(TileCoordinate.x);
        writer.WriteInt(TileCoordinate.y);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        MatchId = ReadString(ref reader);
        EntityId = ReadString(ref reader);
        MovementBehaviourId = ReadString(ref reader);
        TileCoordinate = new Vector2Int(reader.ReadInt(), reader.ReadInt());
    }

    public override void ReceivedOnServer(NetworkConnection connection)
    {
        Server.Receiver.S_ON_MOVE_REQUEST?.Invoke(this, connection);
    }
}

public class NetAttack : NetMessage
{
    public string MatchId;
    public string AttackerEntityId;
    public string AttackBehaviourId;
    public string DamagableEntityId;
    public string DamagableBehaviourId;

    public NetAttack()
    {
        Code = OpCode.ON_ATTACK;
    }
    public NetAttack(DataStreamReader reader)
    {
        Code = OpCode.ON_ATTACK;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        WriteString(ref writer, MatchId);
        WriteString(ref writer, AttackerEntityId);
        WriteString(ref writer, AttackBehaviourId);
        WriteString(ref writer, DamagableEntityId);
        WriteString(ref writer, DamagableBehaviourId);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        MatchId = ReadString(ref reader);
        AttackerEntityId = ReadString(ref reader);
        AttackBehaviourId = ReadString(ref reader);
        DamagableEntityId = ReadString(ref reader);
        DamagableBehaviourId = ReadString(ref reader);
    }

    public override void ReceivedOnServer(NetworkConnection connection)
    {
        Server.Receiver.S_ON_ATTACK_REQUEST?.Invoke(this, connection);
    }
}



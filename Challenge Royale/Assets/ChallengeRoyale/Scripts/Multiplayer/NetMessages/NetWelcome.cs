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

public class NetEndRound : NetMessage
{
    public string MatchId;
    public NetEndRound()
    {
        Code = OpCode.ON_END_ROUND;
    }
    public NetEndRound(DataStreamReader reader)
    {
        Code = OpCode.ON_END_ROUND;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        WriteString(ref writer, MatchId);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        MatchId = ReadString(ref reader);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
        Server.Receiver.S_ON_END_ROUND_REQUEST?.Invoke(this, connection);
    }
}

public class NetHandOverTheInitiative : NetMessage
{
    public string MatchId;
    public NetHandOverTheInitiative()
    {
        Code = OpCode.ON_HAND_OVER_THE_INITIATIVE;
    }
    public NetHandOverTheInitiative(DataStreamReader reader)
    {
        Code = OpCode.ON_HAND_OVER_THE_INITIATIVE;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        WriteString(ref writer, MatchId);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        MatchId = ReadString(ref reader);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
        Server.Receiver.S_ON_HAND_OVER_THE_INITIATIVE_REQUEST?.Invoke(this, connection);
    }
}


public class NetAction : NetMessage
{
    public ActionType ActionType;
    public string Action;
    public NetAction()
    {
        Code = OpCode.ON_ACTION;
    }
    public NetAction(DataStreamReader reader) : base()
    {
        Code = OpCode.ON_ACTION;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)ActionType);
        WriteString(ref writer, Action);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        ActionType = (ActionType)reader.ReadByte();
        Action = ReadString(ref reader);
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_ACTION_RESPONESS?.Invoke(this);
    }
}

public class NetDecreaseEnergy : NetMessage
{
    public string ClientId;
    public int Amount;
    public NetDecreaseEnergy()
    {
        Code = OpCode.ON_DECREASE_ENERGY;
    }
    public NetDecreaseEnergy(DataStreamReader reader) : base()
    {
        Code = OpCode.ON_DECREASE_ENERGY;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        WriteString(ref writer, ClientId);
        writer.WriteInt(Amount);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        ClientId = ReadString(ref reader);
        Amount = reader.ReadInt();
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_DECREASE_ENERGY_RESPONESS?.Invoke(this);
    }
}

public enum ActionType
{
    BEHAVIOUR, ROUND
}





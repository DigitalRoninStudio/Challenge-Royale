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


public class NetGameAction : NetMessage
{
    public GameActionType ActionType;
    public NetGameAction()
    {
        Code = OpCode.ON_GAME_ACTION;
    }
    public NetGameAction(DataStreamReader reader) : base()
    {
        Code = OpCode.ON_GAME_ACTION;
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)ActionType);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        ActionType = (GameActionType)reader.ReadByte();
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_GAME_ACTION_RESPONESS?.Invoke(this);
    }
}

public enum GameActionType
{
    ON_BEHAVIOUR_ACTION, ON_ROUND_ACTION
}


public class NetBehaviourAction : NetGameAction
{
    public string EntityGUID;
    public string BehaviourGUID;
    public NetBehaviourAction() : base()
    {
        ActionType = GameActionType.ON_BEHAVIOUR_ACTION;
    }

    public NetBehaviourAction(DataStreamReader reader) : base()
    {
        ActionType = GameActionType.ON_BEHAVIOUR_ACTION;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        base.Serialize(ref writer);
        WriteString(ref writer, EntityGUID);
        WriteString(ref writer, BehaviourGUID);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        base.Deserialize(reader);
        EntityGUID = ReadString(ref reader);
        BehaviourGUID = ReadString(ref reader);
    }
}

public class NetMovementAction : NetBehaviourAction
{
    public Vector2Int TileCoordinate;
    public NetMovementAction() : base() { }

    public NetMovementAction(DataStreamReader reader) : base() { }

    public override void Serialize(ref DataStreamWriter writer)
    {
        base.Serialize(ref writer);
        writer.WriteInt(TileCoordinate.x);
        writer.WriteInt(TileCoordinate.y);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        base.Deserialize(reader);
        TileCoordinate = new Vector2Int(reader.ReadInt(), reader.ReadInt());
    }
}

public class NetAttackAction : NetBehaviourAction
{
    public string EnemyGUID;
    public string DamageableGUID;
    public NetAttackAction() : base() { }

    public NetAttackAction(DataStreamReader reader) : base() { }

    public override void Serialize(ref DataStreamWriter writer)
    {
        base.Serialize(ref writer);
        WriteString(ref writer, EnemyGUID);
        WriteString(ref writer, DamageableGUID);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        base.Deserialize(reader);
        EnemyGUID = ReadString(ref reader);
        DamageableGUID = ReadString(ref reader);
    }
}

public class NetRoundAction : NetGameAction
{
    public bool EndRound;
    public bool SwitchInitiation;
    public NetRoundAction() : base()
    {
        ActionType = GameActionType.ON_ROUND_ACTION;
    }

    public NetRoundAction(DataStreamReader reader) : base()
    {
        ActionType = GameActionType.ON_ROUND_ACTION;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        base.Serialize(ref writer);
        writer.WriteByte(EndRound ? (byte)1 : (byte)0);
        writer.WriteByte(SwitchInitiation ? (byte)1 : (byte)0);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        base.Deserialize(reader);
        EndRound = reader.ReadByte() == 1;
        SwitchInitiation = reader.ReadByte() == 1;
    }
}





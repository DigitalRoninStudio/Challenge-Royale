using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

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
    public string entityId;
    public string behaviourDataId;
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
        writer.WriteFixedString32(entityId);
        writer.WriteFixedString32(behaviourDataId);

        List<byte> byteList = StringToBytes(serializedBehaviour);
        writer.WriteInt(byteList.Count);

        foreach (byte b in byteList)
            writer.WriteByte(b);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        entityId = reader.ReadFixedString32().ToString();
        behaviourDataId = reader.ReadFixedString32().ToString();

        int length = reader.ReadInt();
        byte[] byteArray = new byte[length];

        for (int i = 0; i < length; i++)
            byteArray[i] = reader.ReadByte();

        serializedBehaviour = BytesToString(new List<byte>(byteArray));
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
    }
    public override void ReceivedOnClient()
    {
        Client.Receiver.C_ON_GAME_ACTION_RESPONESS?.Invoke(this);
    }

    private List<byte> StringToBytes(string str)
    {
        return new List<byte>(System.Text.Encoding.UTF8.GetBytes(str));
    }
    private string BytesToString(List<byte> bytes)
    {
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }
}


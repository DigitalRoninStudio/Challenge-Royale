using Unity.Collections;
using Unity.Networking.Transport;

public class NetMessage
{
    public OpCode Code { set; get; }

    public virtual void Serialize(ref DataStreamWriter writer)
    {
    }
    public virtual void Deserialize(DataStreamReader reader)
    {
    }
    public virtual void ReceivedOnServer(NetworkConnection connection)
    {

    }
    public virtual void ReceivedOnClient()
    {

    }
}

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive()
    {
        Code = OpCode.ON_KEEP_ALIVE;
    }
    public NetKeepAlive(DataStreamReader reader)
    {
        Code = OpCode.ON_KEEP_ALIVE;
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
        NetworkLogger.Log("SERVER RECEIVED ON KEEP ALIVE MESSAGE");
    }
    public override void ReceivedOnClient()
    {
        NetworkLogger.Log("CLIENT RECEIVED ON KEEP ALIVE MESSAGE");
    }
}

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
        NetworkLogger.Log("SERVER RECEIVED ON WELCOME MESSAGE");
    }
    public override void ReceivedOnClient()
    {
        NetworkLogger.Log("CLIENT RECEIVED ON WELCOME MESSAGE");
    }
}


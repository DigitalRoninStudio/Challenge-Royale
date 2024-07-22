using Unity.Collections;
using Unity.Networking.Transport;

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
    }
    public override void ReceivedOnClient()
    {
    }
}


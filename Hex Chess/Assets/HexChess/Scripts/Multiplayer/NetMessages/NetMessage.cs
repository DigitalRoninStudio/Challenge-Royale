using System.Diagnostics;
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


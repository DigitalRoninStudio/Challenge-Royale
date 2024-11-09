using System.Collections.Generic;
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
    #region String Writer
    protected void WriteString(ref DataStreamWriter writer, string str)
    {
        List<byte> byteList = StringToBytes(str);
        writer.WriteInt(byteList.Count);

        foreach (byte b in byteList)
            writer.WriteByte(b);
    }
    private List<byte> StringToBytes(string str)
    {
        return new List<byte>(System.Text.Encoding.UTF8.GetBytes(str));
    }
    #endregion
    #region String Reader
    protected string ReadString(ref DataStreamReader reader)
    {
        int length = reader.ReadInt();
        byte[] byteArray = new byte[length];

        for (int i = 0; i < length; i++)
            byteArray[i] = reader.ReadByte();
        
        return BytesToString(new List<byte>(byteArray));
    }
    private string BytesToString(List<byte> bytes)
    {
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }
    #endregion
}


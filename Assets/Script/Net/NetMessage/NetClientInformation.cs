using Unity.Networking.Transport;

public class NetClientInformation : NetMessage
{
    public string playerName{set;get;} 
    public int playerValue{set;get;}
    public NetClientInformation() // <-- Making the box
    {
        Code = OpCode.CLIENT_INFORMATION;
    }

    public NetClientInformation(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.CLIENT_INFORMATION;
        Deserialize(ref reader);
    }

    public override void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteFixedString128(playerName); 
        writer.WriteInt(playerValue);
    }

    public override void Deserialize(ref Unity.Collections.DataStreamReader reader)
    {
        //We already read the byte in the NetUtility::OnData
        playerName = reader.ReadFixedString128().ToString();
        playerValue = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_CLIENT_INFORMATION?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_CLIENT_INFORMATION?.Invoke(this, cnn);
    }
}

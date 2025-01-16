using Unity.Networking.Transport;

public class NetMakeKill : NetMessage
{
    public int pieceId;
    public NetMakeKill() // <-- Making the box
    {
        Code = OpCode.MAKE_KILL;
    }

    public NetMakeKill(DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.MAKE_KILL;
        Deserialize(ref reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(pieceId);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        pieceId = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_KILL?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_KILL?.Invoke(this, cnn);
    }
}

using Unity.Networking.Transport;

public class NetMakeKill : NetMessage
{
    public int pieceId;
    public int teamSwap;
    public NetMakeKill() // <-- Making the box
    {
        Code = OpCode.MAKE_KILL;
    }

    public NetMakeKill(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.MAKE_KILL;
        Deserialize(ref reader);
    }

    public override void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(pieceId);
        writer.WriteInt(teamSwap);
    }

    public override void Deserialize(ref Unity.Collections.DataStreamReader reader)
    {
        pieceId = reader.ReadInt();
        teamSwap = reader.ReadInt();
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

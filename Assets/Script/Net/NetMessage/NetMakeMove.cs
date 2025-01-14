using Unity.Networking.Transport;

public class NetMakeMove : NetMessage
{
    public int pieceId;
    public int destinationX;
    public int destinationY;
    public int teamId;

    public int endTurn;
    public NetMakeMove() // <-- Making the box
    {
        Code = OpCode.MAKE_MOVE;
    }

    public NetMakeMove(DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.MAKE_MOVE;
        Deserialize(ref reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(pieceId);
        writer.WriteInt(destinationX);
        writer.WriteInt(destinationY);
        writer.WriteInt(teamId);
        writer.WriteInt(endTurn);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        pieceId = reader.ReadInt();
        destinationX = reader.ReadInt();
        destinationY = reader.ReadInt();
        teamId = reader.ReadInt();
        endTurn = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}
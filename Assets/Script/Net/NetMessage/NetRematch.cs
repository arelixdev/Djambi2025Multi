using Unity.Networking.Transport;
using UnityEngine;

public class NetRematch : NetMessage
{
    public int teamId;
    public byte wantRematch;
    public NetRematch() // <-- Making the box
    {
        Code = OpCode.REMATCH;
    }

    public NetRematch(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.REMATCH;
        Deserialize(ref reader);
    }

    public override void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId);
        writer.WriteByte(wantRematch);
    }

    public override void Deserialize(ref Unity.Collections.DataStreamReader reader)
    {
        //We already read the byte in the NetUtility::OnData
        teamId = reader.ReadInt();
        wantRematch = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_REMATCH?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_REMATCH?.Invoke(this, cnn);
    }
}

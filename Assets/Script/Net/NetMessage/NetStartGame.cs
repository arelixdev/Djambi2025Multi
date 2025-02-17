using Unity.Networking.Transport;
using UnityEngine;

public class NetStartGame : NetMessage
{
    public int whoStart;

    public NetStartGame() // <-- Making the box
    {
        Code = OpCode.START_GAME;
    }

    public NetStartGame(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.START_GAME;
        Deserialize(ref reader);
    }

    public override void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(whoStart);
    }

    public override void Deserialize(ref Unity.Collections.DataStreamReader reader)
    {
        //We already read the byte in the NetUtility::OnData
        whoStart = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_START_GAME?.Invoke(this, cnn);
    }
}

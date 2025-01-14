using Unity.Networking.Transport;
using UnityEngine;

public class NetStartGame : NetMessage
{

    public NetStartGame() // <-- Making the box
    {
        Code = OpCode.START_GAME;
    }

    public NetStartGame(DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.START_GAME;
        Deserialize(ref reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        //We already read the byte in the NetUtility::OnData
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

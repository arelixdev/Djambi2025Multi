using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetUpdateColorLobby : NetMessage
{
    public int playerValue;
    public int colorValue;
    public NetUpdateColorLobby() // <-- Making the box
    {
        Code = OpCode.UPDATE_COLOR_LOBBY;
    }

    public NetUpdateColorLobby(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.UPDATE_COLOR_LOBBY;
        Deserialize(ref reader);
    }

     public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);

        writer.WriteInt(playerValue);
        writer.WriteInt(colorValue);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        playerValue = reader.ReadInt();
        colorValue = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_UPDATE_COLOR_LOBBY?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_UPDATE_COLOR_LOBBY?.Invoke(this, cnn);
    }
}

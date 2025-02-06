using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetUpdateReadyLobby : NetMessage
{
    public int playerValue;
    public int isReady;
    public NetUpdateReadyLobby() // <-- Making the box
    {
        Code = OpCode.UPDATE_READY_LOBBY;
    }

    public NetUpdateReadyLobby(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.UPDATE_READY_LOBBY;
        Deserialize(ref reader);
    }

     public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(playerValue);
        writer.WriteInt(isReady);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        playerValue = reader.ReadInt();
        isReady = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_UPDATE_READY_LOBBY?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_UPDATE_READY_LOBBY?.Invoke(this, cnn);
    }
}

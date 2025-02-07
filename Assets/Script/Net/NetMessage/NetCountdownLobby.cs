using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetCountdownLobby : NetMessage
{
    public NetCountdownLobby() // <-- Making the box
    {
        Code = OpCode.UPDATE_COUNTDOWN_LOBBY;
    }

    public NetCountdownLobby(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.UPDATE_READY_LOBBY;
        Deserialize(ref reader);
    }

     public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_UPDATE_COUNTDOWN_LOBBY?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_UPDATE_COUNTDOWN_LOBBY?.Invoke(this, cnn);
    }
}

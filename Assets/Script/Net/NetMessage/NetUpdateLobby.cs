using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetUpdateLobby : NetMessage
{
    public List<ClientInformation> clients = new List<ClientInformation>();
    public int numberClientsMax;

    public NetUpdateLobby() // <-- Making the box
    {
        Code = OpCode.UPDATE_LOBBY;
    }

    public NetUpdateLobby(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.UPDATE_LOBBY;
        Deserialize(ref reader);
    }

     public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(clients.Count);

        foreach (var client in clients)
        {
            writer.WriteFixedString32(client.playerName);
            writer.WriteInt(client.playerValue);
            writer.WriteInt(client.colorValue);
        }

        writer.WriteInt(numberClientsMax);
    }

    public override void Deserialize(ref DataStreamReader reader)
    {
        int count = reader.ReadInt();
        clients = new List<ClientInformation>();

        for (int i = 0; i < count; i++)
        {
            ClientInformation client = new ClientInformation
            {
                playerName = reader.ReadFixedString32().ToString(),
                playerValue = reader.ReadInt(),
                colorValue = reader.ReadInt()
            };
            clients.Add(client);
        }

        numberClientsMax = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_UPDATE_LOBBY?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_UPDATE_LOBBY?.Invoke(this, cnn);
    }
}

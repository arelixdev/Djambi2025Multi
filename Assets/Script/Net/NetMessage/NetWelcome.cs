using Unity.Networking.Transport;
using UnityEngine;

public class NetWelcome : NetMessage
{
    public int AssignedTeam{set;get;}
    public int numberPlayer{set;get;}

    public NetWelcome() // <-- Making the box
    {
        Code = OpCode.WELCOME;
    }

    public NetWelcome(Unity.Collections.DataStreamReader reader) // <-- Receiving the box
    {
        Code = OpCode.WELCOME;
        Deserialize(ref reader);
    }

    public override void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam);
        writer.WriteInt(numberPlayer);  
    }

    public override void Deserialize(ref Unity.Collections.DataStreamReader reader)
    {
        //We already read the byte in the NetUtility::OnData
        AssignedTeam = reader.ReadInt();
        numberPlayer = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    }
}

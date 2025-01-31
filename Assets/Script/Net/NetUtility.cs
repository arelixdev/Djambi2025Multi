using System;
using Unity.Networking.Transport;
using UnityEngine;

public enum OpCode{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    MAKE_KILL = 5,
    REMATCH = 6,
}

public static class NetUtility
{
    public static void OnData(Unity.Collections.DataStreamReader stream, NetworkConnection cnn, Server server = null)
    {
        NetMessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch(opCode)
        {
            case OpCode.KEEP_ALIVE:
                msg = new NetKeepAlive(stream);
                break;
            case OpCode.WELCOME:
                msg = new NetWelcome(stream);
                break;
            case OpCode.START_GAME:
                msg = new NetStartGame(stream);
                break;
            case OpCode.MAKE_MOVE:
                msg = new NetMakeMove(stream);
                break;
            case OpCode.MAKE_KILL:
                msg = new NetMakeKill(stream);
                break;
            case OpCode.REMATCH:
                msg = new NetRematch(stream);
                break;
            default:
                Debug.Log("Unknown OpCode: " + opCode);
                break;
        }

        if(server != null)
        {
            msg.ReceivedOnServer(cnn);
        }
        else
        {
            msg.ReceivedOnClient();
        }
    }

    //Net messages C = Client, S = Server
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_GAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_MAKE_KILL;
    public static Action<NetMessage> C_REMATCH;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_MAKE_KILL;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;
}

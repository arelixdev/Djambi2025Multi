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
    CLIENT_INFORMATION = 7,
    UPDATE_LOBBY = 8,
    UPDATE_COLOR_LOBBY = 9,
    UPDATE_READY_LOBBY = 10,
    UPDATE_COUNTDOWN_LOBBY = 11,
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
            case OpCode.CLIENT_INFORMATION:
                msg = new NetClientInformation(stream);
                break;
            case OpCode.UPDATE_LOBBY:   
                msg = new NetUpdateLobby(stream);
                break;
            case OpCode.UPDATE_COLOR_LOBBY:
                msg = new NetUpdateColorLobby(stream);
                break;
            case OpCode.UPDATE_READY_LOBBY:
                msg = new NetUpdateReadyLobby(stream);
                break;
            case OpCode.UPDATE_COUNTDOWN_LOBBY:
                msg = new NetCountdownLobby(stream);
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
    public static Action<NetMessage> C_CLIENT_INFORMATION;
    public static Action<NetMessage> C_UPDATE_LOBBY;
    public static Action<NetMessage> C_UPDATE_COLOR_LOBBY;
    public static Action<NetMessage> C_UPDATE_READY_LOBBY;
    public static Action<NetMessage> C_UPDATE_COUNTDOWN_LOBBY;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_MAKE_KILL;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;
    public static Action<NetMessage, NetworkConnection> S_CLIENT_INFORMATION;
    public static Action<NetMessage, NetworkConnection> S_UPDATE_LOBBY;
    public static Action<NetMessage, NetworkConnection> S_UPDATE_COLOR_LOBBY;
    public static Action<NetMessage, NetworkConnection> S_UPDATE_READY_LOBBY;
    public static Action<NetMessage, NetworkConnection> S_UPDATE_COUNTDOWN_LOBBY;
}

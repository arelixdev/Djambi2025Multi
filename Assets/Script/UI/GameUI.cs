using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public enum cameraAngle{
    menu = 0,
    botSide = 1,
    topSide = 2,
}

public class GameUI : MonoBehaviour
{
    public static GameUI instance { get; private set; }

    public Server server;
    public Client client;

    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TMP_Text numberPlayerWaitingGame;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject[] cameraAngles;

    public Action<bool> SetLocalGame;

    void Awake()
    {
        instance = this;
        RegisterEvents();
    }

    //Cameras
    public void ChangeCamera(cameraAngle angle)
    {
        for(int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }
        cameraAngles[(int)angle].SetActive(true);
    }


    void Update()
    {
        numberPlayerWaitingGame.text = $"({DjambiBoard.Instance.GetPlayerCount()+1}/4)";
    }

    //Button

    public void OnLocalButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnOnlineButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        
        server.Init(8007);
        client.Init("82.66.188.111", 8007);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        
        client.Init(addressInput.text, 8007);
        startButton.gameObject.SetActive(false);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnHostStartGameButton()
    {
        if(DjambiBoard.Instance.GetPlayerCount() < 1)
        {
            return;
        }
        Server.Instance.BroadCast(new NetStartGame());
        //start game if 2 player 3 or 4 or 5 or 6 change game board
    }

     private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient; 
    }

    private void UnregisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }

    private void OnStartGameClient(NetMessage message)
    {
        menuAnimator.SetTrigger("InGameMenu");
    }
}

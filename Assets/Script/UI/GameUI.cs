using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public enum cameraAngle{
    menu = 0,
    botSide = 1,
    topSide = 2,
}

public class GameUI : MonoBehaviour
{
    private const string ANIMATOR_TRIGGER_WAITINNG_ROOM = "HostMenu"; //TODO change to waiting room
    public static GameUI Instance { get; private set; }

    [Header("Net")]
    public Server server;
    public Client client;

    [Header("Setup")]
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject[] cameraAngles;

    [Header("Online Menu")]
    [SerializeField] private TMP_InputField playerNameInputfield;

    [Header("Create Menu")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private TMP_Dropdown numberPlayerDropdown;
    [SerializeField] private TMP_InputField gameNameInput;
    [SerializeField] private Toggle privateGameToggle;

    [Header("Waiting Menu")]
    [SerializeField] private GameObject waitingCreationRoomPanel; 
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text numberPlayerWaitingGame;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private Button startButton;

    private string hiddenText = "********";
    

    

    public Action<bool> SetLocalGame;

    void Awake()
    {
        Instance = this;
        RegisterEvents();
    }

    public int GetNumberPlayerValue()
    {
        return numberPlayerDropdown.value + 2;
    }

    void Start()
    {
        createRoomPanel.SetActive(false);

        playerNameInputfield.text = PlayerManager.Instance.GetPlayerName();
        playerNameInputfield.onValueChanged.AddListener((string newText) => {
            PlayerManager.Instance.SetPlayerName(newText);
        });

        EventTrigger trigger = roomCodeText.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => ShowRoomCode());
        trigger.triggers.Add(entryEnter);

        // Pointer Exit
        EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => HideRoomCode());
        trigger.triggers.Add(entryExit);
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

    public void UpdateRoomInformation(){

        roomNameText.text = gameNameInput.text;
        roomCodeText.text = hiddenText;
        waitingCreationRoomPanel.SetActive(false);
    }

    public void ShowRoomCode()
    {
        roomCodeText.text = Server.Instance.GetJoinCode();
    }

    public void HideRoomCode()
    {
        roomCodeText.text = hiddenText;
    }

    //Button

    public void OnLocalButton()
    {
        menuAnimator.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        //server.Init(8007);
        //client.Init("127.0.0.1", 8007);
    }

    #region OnlineMenu

    public void OnOnlineButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);

        //server.Init(8007);
        //client.Init("IP RANDOM", 8007);

        if(PlayerManager.Instance.GetPlayerName() == "")
        {
            return;
        }
        createRoomPanel.SetActive(true);
    }

    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        
        //client.Init(addressInput.text, 8007);
        startButton.gameObject.SetActive(false);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineBackButton()
    {
        menuAnimator.SetTrigger("StartMenu");
    }
        
    #endregion

    #region CreateRoomMenu
        public void OnCreateRoomCloseButton()
        {
            createRoomPanel.SetActive(false);
        }

        public void OnCreateRoomCreateButton()
        {
            //TODO Launch server and client create room
            if(gameNameInput.text == "")
            {
                return;
            }
            menuAnimator.SetTrigger(ANIMATOR_TRIGGER_WAITINNG_ROOM);
            PlayerManager.Instance.CreateRoom(gameNameInput.text);
            createRoomPanel.SetActive(false);
        }
    #endregion

    #region WaitingRoomMenu

    public void OnWaitingRoomClipboardBtn()
    {
        GUIUtility.systemCopyBuffer = Server.Instance.GetJoinCode();
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

    #endregion

    public void OnLeaveFromGameMenu()
    {
        ChangeCamera(cameraAngle.menu);
        menuAnimator.SetTrigger("StartMenu");
        
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

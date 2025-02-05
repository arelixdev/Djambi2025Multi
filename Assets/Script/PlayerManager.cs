using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static PlayerManager Instance { get; private set; }

    public List<ClientInformation> clients = new List<ClientInformation>();

    public List<Color> colorList = new List<Color>();

    private string playerName;
    private int playerValue;

    void Awake()
    {
        Instance = this;

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "Player" + Random.Range(0, 1000)); 
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, name);
    }

    public int GetPlayerValue()
    {
        return playerValue;
    }

    public void SetPlayerValue(int value)
    {
        playerValue = value;
    }

    public async void CreateRoom(string roomName)
    {
        Debug.Log("Create Room" + GameUI.Instance.GetNumberPlayerValue());
    
        await Server.Instance.Init(GameUI.Instance.GetNumberPlayerValue(), roomName);

        // Attendre que le join code soit valide avant de démarrer le client
        while (string.IsNullOrEmpty(Server.Instance.GetJoinCode()))
        {
            await Task.Delay(100);
        }

        Debug.Log("Join Code obtained: " + Server.Instance.GetJoinCode());

        await Client.Instance.Init(Server.Instance.GetJoinCode());

        GameUI.Instance.UpdateCreateRoomInformation();
    }
    
}

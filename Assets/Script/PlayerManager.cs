using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static PlayerManager Instance { get; private set; }

    private string playerName;

    // Control vars
    bool isHost;
    bool isPlayer;

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

    public void CreateRoom()
    {
        Debug.Log("Create Room" + GameUI.instance.GetNumberPlayerValue());
        Server.Instance.Init(GameUI.instance.GetNumberPlayerValue());
        //Client.Instance.Init();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        /*for(int i=0; i < playerDataNetworkList.Count; i++){
            PlayerData playerData = playerDataNetworkList[i];
            if(playerData.clientId == clientId){
                playerDataNetworkList.RemoveAt(i);
            }
        }*/
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        /*playerDataNetworkList.Add(new PlayerData{
            clientId = clientId,
            colorId =  GetFirstUnusedColorId()
        });

        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);*/
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        /*if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString()){
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is already in progress";
            return;
        }

        if(NetworkManager.Singleton.ConnectedClientsList.Count >= MAX_PLAYER_AMOUNT){
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;*/
    }
    
}

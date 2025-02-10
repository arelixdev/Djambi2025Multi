using System;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.Assertions;
using System.Collections;

[Serializable]
public class ClientInformation{
    public string playerName;
    public int playerValue;
    public int colorValue;
    public int isReady;
}

public class ClientList
{
    private List<ClientInformation> _clients = new List<ClientInformation>();

    public Action OnChanged; // Événement déclenché à chaque modification

    public void Add(ClientInformation client)
    {
        _clients.Add(client);
        OnChanged?.Invoke();
    }

    public void Remove(ClientInformation client)
    {
        _clients.Remove(client);
        OnChanged?.Invoke();
    }

    public List<ClientInformation> GetClients()
    {
        return new List<ClientInformation>(_clients); // Retourne une copie pour éviter les modifications directes
    }
}

public class Server : MonoBehaviour
{
    public static Server Instance { get; private set; }

    public ClientList Clients = new ClientList();

    private void Awake()
    {
        Instance = this;
        Clients.OnChanged = SendLobbyUpdate; // Appelle SendLobbyUpdate() à chaque modification
    }

    // Allocation response objects
    Allocation hostAllocation;

    public NetworkDriver driver;

    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    string joinCode;
    string roomName;

    

    List<Region> regions = new List<Region>();

    //Methods

    string GetRegionOrQosDefault()
    {
        // Return null (indicating to auto-select the region/QoS) if regions list is empty OR auto-select/QoS is chosen
        if (!regions.Any())
        {
            return null;
        }
        // else use chosen region (offset -1 in dropdown due to first option being auto-select/QoS)
        return regions[0].Id;
    }

    public string GetJoinCode()
    {
        return joinCode;
    }

    public string GetRoomName()
    {
        return roomName;
    }

    public void SetRoomName(string name)
    {
        roomName = name;
    }

    public Allocation GetHostAllocation()
    {
        return hostAllocation;
    }

    public int GetNumberPlayerMax()
    {
        return connections.Length;
    }

    public async Task Init(int numberPlayerMax, string roomName)
    {
        await InitializeUnityServices();
        string region = GetRegionOrQosDefault();
        int maxConnections = numberPlayerMax;
        hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        Debug.Log($"Host Allocation ID: {hostAllocation.AllocationId}, region: {hostAllocation.Region}");

        Debug.Log("Host - Binding to the Relay server using UTP.");
        var relayServerData = new RelayServerData(hostAllocation, "udp");

        // Create NetworkSettings using the Relay server data.
        var settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayServerData);
        
        driver = NetworkDriver.Create(settings);

        var endpoint = NetworkEndpoint.AnyIpv4;
        if (driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Le serveur n'a pas pu être bind !");
        }
        else
        {
            Debug.Log("Le serveur est bind et écoute !");
        }

        if (driver.Listen() != 0)
        {
            Debug.LogError("Le serveur n'a pas pu écouter !");
        }
        else
        {
            Debug.Log("Le serveur écoute !");
        }
        connections = new NativeList<NetworkConnection>(numberPlayerMax, Allocator.Persistent); 

        Debug.Log("Host - Getting a join code for my allocation. I would share that join code with the other players so they can join my session.");

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
            Debug.Log("Host - Got join code: " + joinCode);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }

        isActive = true;
    }

    private async Task InitializeUnityServices()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized!");

                // S'authentifier (obligatoire pour utiliser Relay)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in Anonymously!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Unity Services Initialization Failed: {e.Message}");
            }
        }
    }
    public void Shutdown()
    {
        if(driver.IsCreated)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }

    public void OnDestroy() {
        Shutdown();
    }

    public void Update(){

         // Skip update logic if the Host is not yet bound.
        if (!driver.IsCreated)
        {
            return;
        }

        // This keeps the binding to the Relay server alive,
        // preventing it from timing out due to inactivity.
        driver.ScheduleUpdate().Complete();

        // Clean up stale connections.
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                Debug.Log("Stale connection removed");
                connections.RemoveAt(i);
                --i;
            }
        }

        // Accept incoming client connections.
        NetworkConnection incomingConnection;
        while ((incomingConnection = driver.Accept()) != default(NetworkConnection))
        {
            // Adds the requesting Player to the serverConnections list.
            // This also sends a Connect event back the requesting Player,
            // as a means of acknowledging acceptance.
            Debug.Log($"Tentative de connexion détectée : {incomingConnection}");
            var serverClients = Clients.GetClients();
            foreach (var client in serverClients)
            {
                Debug.Log("Client found in PlayerManager" + client.isReady);
                var matchingClient = PlayerManager.Instance.clients.Find(c => c.playerName == client.playerName);
                if (matchingClient != null)
                {
                    client.playerValue = matchingClient.playerValue;
                    client.colorValue = matchingClient.colorValue;
                }
            }
            connections.Add(incomingConnection);

            SendLobbyUpdate();  
        }

        UpdateMessagePump();
    }

    public void UpdateLobbyInformation()
    {
        var serverClients = Clients.GetClients();
        foreach (var client in serverClients)
        {
            var matchingClient = PlayerManager.Instance.clients.Find(c => c.playerName == client.playerName);
            if (matchingClient != null)
            { 
                client.playerValue = matchingClient.playerValue;
                client.colorValue = matchingClient.colorValue;
                client.isReady = matchingClient.isReady;
            }
        }

        // Vérifier si tous les joueurs sont prêts
        bool allPlayersReady = serverClients.All(client => client.isReady == 1);

        // Vérifier si le nombre de joueurs prêts correspond au nombre attendu
        if (allPlayersReady && serverClients.Count == GameUI.Instance.GetNumberPlayerValue())
        {
            StartGame();
        }
    }

    private Coroutine startGameCoroutine;

    private void StartGame()
    {
        if (startGameCoroutine != null)
        {
            StopCoroutine(startGameCoroutine);
        }
        startGameCoroutine = StartCoroutine(StartGameCountdown());
    }


    private IEnumerator StartGameCountdown()
    {
        float countdown = 3f;
        NetCountdownLobby msg = new NetCountdownLobby();

        while (countdown > 0)
        {
            
            msg.countdown = countdown;
            BroadCast(msg);
            yield return new WaitForSeconds(1f);
            countdown--;

            // Vérifier si tous les joueurs sont toujours prêts
            var serverClients = Clients.GetClients();
            bool allPlayersReady = serverClients.All(client => client.isReady == 1);
            
            if (!allPlayersReady)
            {
                msg = new NetCountdownLobby();
                msg.countdown = 0;
                BroadCast(msg);
                startGameCoroutine = null;
                yield break; // Stop la coroutine
            }
        }

        Debug.Log("Lancement du jeu !");
        msg = new NetCountdownLobby();
        msg.countdown = 0;
        BroadCast(msg);
        if (DjambiBoard.Instance.GetPlayerCount() < 1)
        {
            startGameCoroutine = null;
            yield break;
        }

        BroadCast(new NetStartGame());
        startGameCoroutine = null; // Réinitialiser la référence
    }


    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        for(int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if(cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                }
                else if(cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown();
                }
            }
        }
    }

    //Server Specific

    public void SendToClient(NetworkConnection c, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(c, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    public void SendLobbyUpdate()
    {
        NetUpdateLobby updateLobby = new NetUpdateLobby();
        updateLobby.clients = Clients.GetClients();
        BroadCast(updateLobby);
    }

    public void BroadCast(NetMessage msg)
    {
        for(int i = 0; i < connections.Length; i++)
        {
            if(connections[i].IsCreated)
            {
                SendToClient(connections[i], msg);
            }
        }
    }
}

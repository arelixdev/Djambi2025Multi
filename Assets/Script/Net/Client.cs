using System;
using System.Threading.Tasks;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Allocation response objects
    JoinAllocation playerAllocation;

    public NetworkDriver driver;

    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;

    //Methods
    public async Task Init(string joinCode)
    {
        if (String.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Please input a join code.");
            return;
        }

        await InitializeUnityServices();

        Debug.Log("Player - Joining host allocation using join code. Upon success, I have 10 seconds to BIND to the Relay server that I've allocated.");

        try
        {
            playerAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Player Allocation ID: " + playerAllocation.AllocationId);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        Debug.Log("Player - Binding to the Relay server using UTP.");

        // Extract the Relay server data from the Join Allocation response.
        var relayServerData = new RelayServerData(playerAllocation, "udp");

        // Create NetworkSettings using the Relay server data.
        var settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayServerData);
        
        driver = NetworkDriver.Create(settings);
        /*NetworkEndpoint endPoint = NetworkEndpoint.Parse(ip, port);

        connection = driver.Connect(endPoint);

        Debug.Log("Attemping to connect to Server on" + endPoint.Address);

        isActive = true;*/

        RegisterToEvent();
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
        if(isActive)
        {
            //UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }

    public void OnDestroy() {
        Shutdown();
    }

    public void Update(){
        if(!isActive)
            return;

        driver.ScheduleUpdate().Complete();
        CheckAlive();

        UpdateMessagePump();

    }
    private void CheckAlive()
    {
        if(!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            //Shutdown();
        }
    }

    private void UpdateMessagePump()
    {
        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        while((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if(cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("Connected to server");
            }
            else if(cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if(cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connection = default(NetworkConnection);
                connectionDropped?.Invoke(); 
            }
        }
    }

    public void SendToServer(NetMessage msg)
    {
        Unity.Collections.DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }  

    //Event Parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm)
    {
        SendToServer(nm);
    }
}

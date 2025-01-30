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

public class Server : MonoBehaviour
{
    public static Server Instance { get; private set; }

    // Allocation response objects
    Allocation hostAllocation;

    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;

    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    string joinCode = "n/a";

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

    public async void Init(int numberPlayerMax)
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
        /*NetworkEndpoint endPoint = NetworkEndpoint.AnyIpv4;
        endPoint.Port = port;

        if(driver.Bind(endPoint) != 0)
        {
            Debug.Log("Failed to bind to port " + endPoint.Port);
            return;
        }
        else
        {
            driver.Listen();
            Debug.Log("Server started on port " + endPoint.Port);
        }

        connections = new NativeList<NetworkConnection>(4, Allocator.Persistent); 
        isActive = true;*/
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
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }

    public void OnDestroy() {
        Shutdown();
    }

    public void Update(){
        if(!isActive)
            return;

        KeepAlive();

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();

    }

    private void KeepAlive()
    {
        if(Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            BroadCast(new NetKeepAlive());
        }
    }
    private void CleanupConnections()
    {
        for(int i = 0; i < connections.Length; i++)
        {
            if(!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
            Debug.Log("Accepted a connection");
        }
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
                    //Shutdown();
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

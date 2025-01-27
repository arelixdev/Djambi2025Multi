using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;

    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;

    //Methods
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndpoint endPoint = NetworkEndpoint.Parse(ip, port);

        connection = driver.Connect(endPoint);

        Debug.Log("Attemping to connect to Server on" + endPoint.Address);

        isActive = true;

        RegisterToEvent();
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

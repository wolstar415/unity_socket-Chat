using System;
using System.Collections.Generic;
using SocketIOClient;
using UnityEngine;

//using Debug = System.Diagnostics.Debug;

public class SocketManager : MonoBehaviour
{
    public SocketIOUnity socket;
    // Start is called before the first frame update
    void Start()
    {
        //var uri = new Uri("http://127.0.0.1:7777");
        var uri = new Uri("http://3.36.51.5:57825");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        
        socket.On("hello", (a) =>
        {
            Debug.Log(a.PacketId);
            Debug.Log(a.SocketIO);
            Debug.Log(a);
            Debug.Log(a.ToString());
            Debug.Log(socket.Id);
            Debug.Log(socket.Namespace);
            Debug.Log(socket.Options);

        });
        Debug.Log("Connecting...");
        socket.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            socket.Emit("hello", "asd");
        }   
        if (Input.GetKeyDown(KeyCode.W))
        {
            socket.Emit("update item", 1, 2, 3);
        }   
    }
}

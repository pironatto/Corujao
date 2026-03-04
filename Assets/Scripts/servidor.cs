using UnityEngine;
using NativeWebSocket;
using System.Text;

public class servidor : MonoBehaviour
{
    WebSocket websocket;

    async void Start()
    {
        websocket = new WebSocket("ws://localhost:3000"); // Replace with your PHP server address and port

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
          
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Message received from server: " + message);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed: " + e);
        };

        await websocket.Connect();
    }

    void OnDestroy()
    {
        if (websocket != null)
        {
            websocket.Close();
        }
    }

    // Example function to send a message
    public void SendMessageToServer(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.SendText(message);
        }
    }
}

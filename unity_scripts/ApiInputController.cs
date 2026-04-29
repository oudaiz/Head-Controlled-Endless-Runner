using UnityEngine;
using NativeWebSocket;
using System;
using System.Collections.Generic;

public class ApiInputController : MonoBehaviour
{
    public string socketUrl = "ws://127.0.0.1:8765";

    private WebSocket websocket;
    private PlayerMovement player;

    [Serializable]
    public class CommandMessage
    {
        public string command;
        public int command_id;
    }

    async void Start()
    {
        if (GameLaunchSettings.SelectedInputMode != InputMode.Camera)
        {
            enabled = false;
            return;
        }

        player = FindAnyObjectByType<PlayerMovement>();

        websocket = new WebSocket(socketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connected.");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket closed.");
        };

        websocket.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            CommandMessage data = JsonUtility.FromJson<CommandMessage>(json);

            if (data != null)
                ExecuteCommand(data.command);
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }

    void ExecuteCommand(string command)
    {
        if (player == null) return;

        switch (command)
        {
            case "LEFT":
                player.CommandMoveLeft();
                break;
            case "RIGHT":
                player.CommandMoveRight();
                break;
            case "JUMP":
                player.CommandJump();
                break;
            case "SLIDE":
                player.CommandSlide();
                break;
        }
    }
}
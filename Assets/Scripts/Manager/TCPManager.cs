using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    private TcpListener _server;
    private bool _isRunning;
    private List<TcpClient> _connectedClients = new List<TcpClient>(); // Track connected clients

    private GameManager _gameManager;
    private Queue<Action> _mainThreadActions = new Queue<Action>(); // Queue for actions to be executed on the main thread

    // Initialize server
    public void StartServer(int port, GameManager gameManager)
    {
        _server = new TcpListener(IPAddress.Any, port);
        _server.Start();
        _isRunning = true;
        Debug.Log("Server started on port: " + port);
        _gameManager = gameManager;

        // Accept client connections asynchronously
        //Task.Run(() => AcceptClient());

        // use this if need to block the game if not connected
        AcceptClientAsync();
    }

    private async void AcceptClientAsync()
    {
        while (_isRunning)
        {
            //Debug.Log("Waiting for a client to connect...");
            try
            {
                TcpClient newClient = await _server.AcceptTcpClientAsync();
                if (!_isRunning) return; // Check if the server is still running
                
                Debug.Log("Client connected");
                _connectedClients.Add(newClient);

                // Notify the game manager that a client has connected and the game can start
                _gameManager.StartRLGame();

                // Handle communication asynchronously for the connected client
                _ = Task.Run(() => HandleClient(newClient));
            }
            catch (Exception e)
            {
                if (_isRunning)
                {
                    Debug.LogError("Error while waiting for client connection: " + e.Message);
                }
            }
        }
    }
    private async Task AcceptClient()
    {
        while (_isRunning)
        {
            try
            {
                TcpClient newClient = await _server.AcceptTcpClientAsync();
                Debug.Log("New client connected");

                // Add the new client to the list
                _connectedClients.Add(newClient);

                // Handle communication asynchronously
                _ = Task.Run(() => HandleClient(newClient));
            }
            catch (ObjectDisposedException)
            {
                // This exception can occur if the listener is stopped while waiting for a connection
                if (_isRunning)
                {
                    Debug.LogError("Server stopped unexpectedly.");
                }
                else
                {
                    Debug.Log("Server has been stopped.");
                }
            }
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int byteCount;

            while ((byteCount = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string receivedData = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Debug.Log("Received: " + receivedData);

                // Queue the processing of received data to be executed on the main thread
                EnqueueMainThreadAction(() => ProcessReceivedAction(receivedData));
            }
        }
    }

    private void EnqueueMainThreadAction(Action action)
    {
        lock (_mainThreadActions)
        {
            _mainThreadActions.Enqueue(action);
        }
    }

    private void Update()
    {
        // Execute queued actions on the main thread
        lock (_mainThreadActions)
        {
            while (_mainThreadActions.Count > 0)
            {
                var action = _mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
    }

    private void ProcessReceivedAction(string data)
    {
        try
        {
            // Deserialize the received JSON action data using JsonUtility
            ActionData action = JsonUtility.FromJson<ActionData>(data);
            if (action != null)
            {
                // Process the action (e.g., play the card)
                Debug.Log($"Processing Action: {action.actionType}, Data: {JsonUtility.ToJson(action)}");

                // Implement your logic to play the card here
                if (action.actionType == "PlayCard")
                {
                    int cardIndex = action.data.index;
                    int colorIndex = action.data.colorIndex; // For wildcards
                    _gameManager.AutoPlayCard(cardIndex, colorIndex); // Implement this method in your game logic
                }
            }
            else
            {
                Debug.LogWarning("Failed to deserialize action data.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing received action: " + ex.Message);
        }
    }

    public void SendData(string data)
    {
        if (_server != null && _connectedClients.Count > 0)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            // Send data to each connected client
            foreach (var client in _connectedClients)
            {
                if (client.Connected)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(dataBytes, 0, dataBytes.Length);
                        Debug.Log("Data sent to client: " + data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to send data to client: " + e.Message);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No connected clients to send data to.");
        }
    }

    private void OnApplicationQuit()
    {   

        StopServer();
    }

    private void OnDisable()
    {   
        StopServer();
    }

    private void StopServer()
    {
        if (_isRunning)
        {   
            Debug.Log("Stopping server...");
            _isRunning = false;
            _server?.Stop();
            foreach (var client in _connectedClients)
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }
            _connectedClients.Clear();
            Debug.Log("Server stopped.");
        }
    }

}
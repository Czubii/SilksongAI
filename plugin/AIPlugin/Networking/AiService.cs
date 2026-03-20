using AIPlugin.Utilities;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.Networking
{
    /// <summary>
    /// High level integration layer between external python AI server and the game for real time game controll
    /// This class is responsible for Holding the GameClient, managing lifecycle and auto reconnection to the server if something fails
    /// </summary>
    public class AiService : MonoBehaviour, IDisposable
    {
        private AiClient _client;
        private AiGateway _gateway;
        public AiGateway Gateway => _gateway;

        private Task _connectTask;
        private Task _reconnectTask;
        public bool IsConnected => _client?.IsConnected ?? false;

        private bool _autoReconnect = true;
        private int _reconnectAttempts = 10;
        public int _reconnectAttemptDelay = 2000;

        public event Action OnConnected;
        public event Action OnDisconnected;
        private bool _notifyConnected;
        private bool _notifyDisconnected;

      
        public void Initialize(string ip, int port)
        {
            _client = new AiClient(ip, port);
            _gateway = new AiGateway(_client);

            _client.OnDisconnect += HandleDisconnect;
            _client.OnConnect += HandleConnect;
        }
        private void HandleDisconnect() //TODO add some game pausing or something nice here 
        {
            _notifyDisconnected = true; 
            AIPlugin.Log.LogWarning("Lost Connection to the AI server");
            if (_autoReconnect)
            {
                AIPlugin.Log.LogWarning("Trying to reconnect...");
                _reconnectTask = TryReconnect();
            }
        }
        private void HandleConnect() 
        {
            _notifyConnected = true;
        }
        void Update()
        {
            if (_notifyConnected)
            {
                OnConnected?.Invoke();
                _notifyConnected = false;   
            }
            if (_notifyDisconnected)
            {
                OnDisconnected?.Invoke();
                _notifyDisconnected = false;
            }
        }

        public void ConnectToServer()
        {
            if (IsConnected || _connectTask != null) return;

            _connectTask = Connect();
        }
        private async Task TryReconnect()
        {
            try
            {
                for (int i = 0; i < _reconnectAttempts; i++)
                {
                    await Task.Delay(_reconnectAttemptDelay);

                    await Connect();

                    if (IsConnected)
                    {
                        ThreadSafeLogService.Log($"Reconnected Succesfully", AIPlugin.Log.LogMessage);
                        return;
                    }
                }
                ThreadSafeLogService.Log($"Could not reconnect after {_reconnectAttempts} attempts", AIPlugin.Log.LogMessage);
            }
            finally
            {
                _reconnectTask = null;
            }

        }
        private async Task Connect()
        {
            try
            {
                if (IsConnected) return;
                await _client.ConnectAsync();
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log($"Connection failed: {ex.Message}", AIPlugin.Log.LogWarning);
            }
            finally
            {
                _connectTask = null;
            }
        }

        public void Dispose()
        {
            _client.OnDisconnect -= HandleDisconnect;
            _client.OnConnect -= HandleConnect;
            _client?.Dispose();
            _gateway?.Dispose();    
        } 

    }
}

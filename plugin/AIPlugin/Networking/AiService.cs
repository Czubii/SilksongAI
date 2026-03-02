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
    public class AiService : IDisposable
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

        public static class ThreadSafeLog
        {
            private static ConcurrentQueue<(string msg, Action<string> logAction)> _queue = new ConcurrentQueue<(string, Action<string>)>();
            public static void Log(string message, Action<string> unityLog = null)
            {
                _queue.Enqueue((message, unityLog ?? Debug.Log));
            }
            public static void Flush()
            {
                while (_queue.TryDequeue(out var item))
                {
                    item.logAction(item.msg);
                }
            }
        }
        public AiService(string ip, int port) 
        {
            _client = new AiClient(ip, port);
            _gateway = new AiGateway(_client);

            _client.OnDisconnect += OnDisconnect;
        }
        private void OnDisconnect() //TODO add some game pausing or something nice here 
        {
            AIPlugin.Log.LogWarning("Lost Connection to the AI server");
            if (_autoReconnect)
            {
                AIPlugin.Log.LogWarning("Trying to reconnect...");
                _reconnectTask = TryReconnect();
            }
        }
        void Update()
        {
            ThreadSafeLog.Flush();
        }

        public void ConnectToServer()
        {
            if (IsConnected || _connectTask != null) return;

            _connectTask = Connect();
        }
        private async Task TryReconnect()
        {
            for (int i = 0; i < _reconnectAttempts; i++)
            {
                await Task.Delay(_reconnectAttemptDelay);
                await Connect();

                if (IsConnected)
                {
                    ThreadSafeLog.Log($"Reconnected Succesfully", AIPlugin.Log.LogMessage);
                    return;
                }
            }
            ThreadSafeLog.Log($"Could not reconnect after {_reconnectAttempts} attempts", AIPlugin.Log.LogMessage);
        }
        private async Task Connect()
        {
            try
            {
                var asyncConnectTask = _client.ConnectAsync();
                await asyncConnectTask;
            }
            catch (Exception ex)
            {
                ThreadSafeLog.Log($"Connection failed: {ex.Message}", AIPlugin.Log.LogWarning);
            }
            finally
            {
                _connectTask = null;
            }
        }

        public void Dispose()
        {
            _client.OnDisconnect -= OnDisconnect;
            _client?.Dispose();
            _gateway?.Dispose();    
        } 

    }
}

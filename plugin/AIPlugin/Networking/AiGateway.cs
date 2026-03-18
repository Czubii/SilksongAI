using AIPlugin.Utilities;
using MessagePack;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Playables;

namespace AIPlugin.Networking
{
    public class AiGateway: IDisposable
    {
        private AiClient _client;
        private ConcurrentDictionary<string, object> _pendingRequests;
        public AiGateway(AiClient client)
        {
            _client = client;

            _client.OnMessageRecieved += OnMessageRecieved;
            _client.OnDisconnect += OnDisconnect;

            _pendingRequests = new ConcurrentDictionary<string, object>();
        }
        private void OnDisconnect()
        {
            foreach (var request in _pendingRequests)
            {
                var tcsType = request.Value.GetType();
                tcsType.GetMethod("SetException", new[] { typeof(Exception) })?.
                    Invoke(request.Value, new[] { new Exception("Server disconnected before sending the response")});
            }
        }
        public async Task<(TResponse, string)> SendRequestAsync<TResponse, TPayload>(string type, TPayload payload) 
            where TResponse: class
            where TPayload: class
        {
            if (_client == null || !_client.IsConnected) return default;

            try
            {
                string ID = Guid.NewGuid().ToString();

                var request = new Protocol.RequestEnvelope<TPayload>()
                {
                    RequestId = ID,
                    Type = type,
                    Payload = payload
                };

                var taskCompletion = new TaskCompletionSource<Protocol.ResponseEnvelope<TResponse>>();
                _pendingRequests[ID] = taskCompletion;

                byte[] bytes = MessagePackSerializer.Serialize(request);
                await _client.SendAsync(bytes);

                var envelope = await taskCompletion.Task;

                if (!envelope.Success)
                    throw new Exception("Server Error:\n" + envelope.ServerLog);

                return (envelope.Payload, envelope.ServerLog);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log($"There was an error when contacting the server: \n {ex}", 
                    AIPlugin.Log.LogError);
                throw ex;
            }
        }
        public void OnMessageRecieved(byte[] data)
        {
            Protocol.ResponseEnvelope<object> header;
            try
            {
                header = MessagePackSerializer.Deserialize<Protocol.ResponseEnvelope<object>>(data, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
                return;
            }

            if (!_pendingRequests.TryGetValue(header.RequestId, out var boxedTcs))
                return;

            var tcsType = boxedTcs.GetType();
            var payloadType = tcsType.GenericTypeArguments[0];

            try
            {
                var typedEnvelope = MessagePackSerializer.Deserialize(payloadType, data, MessagePack.Resolvers.ContractlessStandardResolver.Options);

                tcsType.GetMethod("SetResult")?.Invoke(
                    boxedTcs,
                    new[] { typedEnvelope });
            }
            catch (Exception ex)
            {
                
                try
                {
                    tcsType.GetMethod("SetException", new[] {typeof(Exception)})?.
                        Invoke(boxedTcs, new[] { ex });
                }
                catch (Exception ex2)
                {
                    ThreadSafeLogService.Log($"Failed to process message: {ex}", AIPlugin.Log.LogError);
                    ThreadSafeLogService.Log($"Failed to invoke SetException on TCS: {ex2}", 
                        AIPlugin.Log.LogError);
                }
            }
            finally 
            {
                _pendingRequests.TryRemove(header.RequestId, out _);
            }
        }   
        public void Dispose()
        {
            _client.OnMessageRecieved -= OnMessageRecieved;
        }
    }
}




using AIPlugin.Utilities;
using MessagePack;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Playables;
using static AIPlugin.Networking.Protocol;

namespace AIPlugin.Networking
{
    public class AiGateway: IDisposable
    {
        private AiClient _client;
        private ConcurrentDictionary<string, TaskCompletionSource<ResponseEnvelope>> _pendingRequests;
        public ServerEvents Events = new ServerEvents();
        public AiGateway(AiClient client)
        {
            _client = client;

            _client.OnMessageRecieved += OnMessageRecieved;
            _client.OnDisconnect += OnDisconnect;

            _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<Protocol.ResponseEnvelope>>();
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

                var taskCompletion = new TaskCompletionSource<ResponseEnvelope>();
                _pendingRequests[ID] = taskCompletion;

                byte[] bytes = MessagePackSerializer.Serialize(request);
                await _client.SendAsync(bytes);

                var envelope = await taskCompletion.Task;

                if (!envelope.Success)
                    throw new Exception("Server Error:\n" + envelope.ServerLog);

                var deserializedPayload = DeserializePayload<TResponse>(envelope.Payload);

                return (deserializedPayload, envelope.ServerLog);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log($"There was an error when contacting the server: \n {ex}", 
                    AIPlugin.Log.LogError);
                throw ex;
            }
        }
        public static T DeserializePayload<T>(object payload)
        {
            if (payload is byte[] bytes)
                return MessagePackSerializer.Deserialize<T>(bytes);

            throw new InvalidCastException($"Payload cannot be converted to {typeof(T)}");
        }
        private string GetMessageKind(byte[] data)
        {
            string kind = null;
            try
            {
                var reader = new MessagePackReader(data);
                int mapLength = reader.ReadMapHeader();

                for (int i = 0; i < mapLength; i++)
                {
                    var key = reader.ReadString();
                    if (key == "kind")
                    {
                        kind = reader.ReadString();
                        break;
                    }

                    reader.Skip();
                }
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(
                    $"Exception occured when trying to determine the server message kind: {ex}",
                    AIPlugin.Log.LogError);
            }

            return kind;
        }
        public void OnMessageRecieved(byte[] data)
        {
            var kind = GetMessageKind(data);

            if(kind == null)
            {
                ThreadSafeLogService.Log(
                    $"Incoming message lacks kind",
                    AIPlugin.Log.LogError);
            }
            
            switch (kind)
            {
                case "response":
                    HandleResponseMessage(data);
                    break;

                case "event":
                    HandleEventMessage(data);
                    break;

                default:
                    ThreadSafeLogService.Log(
                    $"Unknown server message kind: {kind}",
                    AIPlugin.Log.LogError);
                    break;
            }
        }

        private void HandleEventMessage(byte[] data)
        {
            try
            {
                var envelope = MessagePackSerializer.Deserialize<EventEnvelope>(data);

                if (envelope.EventType == null) throw new ArgumentNullException("envelope.EventType cannot be null");

                Events.RaiseEvent(envelope.EventType, envelope.Payload);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
            }
        }

        private void HandleResponseMessage(byte[] data)
        {
            ResponseEnvelope responseEnvelope;
            try
            {
                responseEnvelope = MessagePackSerializer.Deserialize<ResponseEnvelope>(data);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log("Got exception when deserializing the reponse message: \n", AIPlugin.Log.LogError);
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
                return;
            }

            if (!_pendingRequests.TryGetValue(responseEnvelope.RequestId, out var tcs))
                return;

            try
            {
                tcs.SetResult(responseEnvelope);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            finally
            {
                _pendingRequests.TryRemove(responseEnvelope.RequestId, out _);
            }
        }
        public void Dispose()
        {
            _client.OnMessageRecieved -= OnMessageRecieved;
        }
    }
}




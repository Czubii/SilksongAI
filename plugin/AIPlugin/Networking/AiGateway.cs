using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIPlugin.Networking
{
    public class AiGateway: IDisposable
    {
        private AiClient _client;
        private ConcurrentDictionary<string, TaskCompletionSource<Protocol.ResponseEnvelope>> _requestCompletionSources;
        public AiGateway(AiClient client)
        {
            _client = client;

            _client.OnMessageRecieved += OnMessageRecieved;

            _requestCompletionSources = new ConcurrentDictionary<string, TaskCompletionSource<Protocol.ResponseEnvelope>>();
        }
        public async Task<List<string>> ListModelsAsync()
        {
            if (_client == null || !_client.IsConnected) return null;

            try
            {
                var response = await SendRequestAsync("get_models");
                var models = ((object[])response.Payload).Select(x => x.ToString()).ToList();

                return models;
            }
            catch (Exception ex)
            {
                AiService.ThreadSafeLog.Log(ex.ToString(), AIPlugin.Log.LogError);
            }

            return null;
        }
        private async Task<Protocol.ResponseEnvelope> SendRequestAsync(string type, object payload = null)
        {
            Guid guid = Guid.NewGuid();

            var request = new Protocol.RequestEnvelope()
            {
                RequestId = guid.ToString(),
                Type = type,
                Payload = payload
            };

            byte[] bytes = MessagePackSerializer.Serialize(request, MessagePack.Resolvers.ContractlessStandardResolver.Options);

            TaskCompletionSource<Protocol.ResponseEnvelope> taskCompletion = new TaskCompletionSource<Protocol.ResponseEnvelope>();
            _requestCompletionSources.TryAdd(guid.ToString(), taskCompletion);

            await _client.SendAsync(bytes);

            Protocol.ResponseEnvelope response = await taskCompletion.Task;

            return response;
        }
        public void OnMessageRecieved(byte[] data)
        {
            var response = MessagePackSerializer.Deserialize<Protocol.ResponseEnvelope>(data, MessagePack.Resolvers.ContractlessStandardResolver.Options);

            if (_requestCompletionSources.TryGetValue(response.RequestId, out var tcs))
            {
                tcs.SetResult(response);
                _requestCompletionSources.TryRemove(response.RequestId, out _);
            }
        }   
        public void Dispose()
        {
            _client.OnMessageRecieved -= OnMessageRecieved;
        }
    }
}

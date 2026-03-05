using AIPlugin.Utilities;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            _pendingRequests = new ConcurrentDictionary<string, object>();
        }
        public async Task<FrameUserInputs> PredictInputsAsync(LivePredictionFrameData frameData)
        {
            if (_client == null || !_client.IsConnected) return null;

            try
            {
                return await SendRequestAsync<FrameUserInputs, LivePredictionFrameData >("predict_inputs", frameData);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
            }

            return null;
        }
        public async Task<ModelsOverviewResponse> ListModelsAsync()
        {
            if (_client == null || !_client.IsConnected) return null;

            try
            {
                return await SendRequestAsync<ModelsOverviewResponse>("get_models");
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
            }

            return null;
        }
        public async Task<ModelsOverviewResponse> SelectModel(string BossName, string ModelName)
        {
            if (_client == null || !_client.IsConnected) return null;

            try
            {
                var payload = new SelectModelRequest(){
                    BossName = BossName,
                    ModelName = ModelName
                };
                return await SendRequestAsync<ModelsOverviewResponse, SelectModelRequest>("select_model", payload);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
            }

            return null;
        }
        private async Task<TResponse> SendRequestAsync<TResponse>(string type)
        {
            return await SendRequestAsync<TResponse, Empty>(type, default);
        }
        private async Task<TResponse> SendRequestAsync<TResponse, TPayload>(string type, TPayload payload)
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

            byte[] bytes = MessagePackSerializer.Serialize(request, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            await _client.SendAsync(bytes);

            var envelope = await taskCompletion.Task;

            if(!envelope.Success)
                throw new Exception("Server Error: " + envelope.ErrorMessage);

            return envelope.Payload;
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
                    tcsType.GetMethod("SetException")?.Invoke(boxedTcs, new[] { ex });
                }
                catch
                {
                    ThreadSafeLogService.Log("Failed to invoke SetException on TCS", AIPlugin.Log.LogError);
                }

                ThreadSafeLogService.Log($"Failed to process message: {ex}", AIPlugin.Log.LogError);
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

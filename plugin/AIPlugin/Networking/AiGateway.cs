using AIPlugin.Utilities;
using MessagePack;
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

            _pendingRequests = new ConcurrentDictionary<string, object>();
        }
        public async Task<FrameUserInputs> PredictInputsAsync(InferenceFrame frameData)
        {
            if (_client == null || !_client.IsConnected) return null;

            try
            {
                return await SendRequestAsync<FrameUserInputs, InferenceFrame >("predict_inputs", frameData);
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
            }

            return null;
        }
        public async Task<Payloads.ModelsOverviewResponse> ListModelsAsync()
        {
            return await SendRequestAsync<Payloads.ModelsOverviewResponse>("get_models");
        }
        public async Task<Payloads.ModelsOverviewResponse> SelectModel(string BossName, string ModelName)
        {
            var payload = new Payloads.SelectModelRequest(){
                BossName = BossName,
                ModelName = ModelName
            };
            return await SendRequestAsync<Payloads.ModelsOverviewResponse, Payloads.SelectModelRequest>("select_model", payload);
        }

        public async Task<Payloads.GetArchitecturesResponse> GetArchitecturesAsync()
        {
            return await SendRequestAsync<Payloads.GetArchitecturesResponse>("get_architectures");
        }

        private async Task<TResponse> SendRequestAsync<TResponse>(string type)
        {
            return await SendRequestAsync<TResponse, Empty>(type, default);
        }
        private async Task<TResponse> SendRequestAsync<TResponse, TPayload>(string type, TPayload payload)
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
                ThreadSafeLogService.Log(BitConverter.ToString(bytes.Take(8).ToArray()), AIPlugin.Log.LogWarning);
                await _client.SendAsync(bytes);

                var envelope = await taskCompletion.Task;

                if (!envelope.Success)
                    throw new Exception("Server Error: " + envelope.ErrorMessage);

                return envelope.Payload;
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log(ex.ToString(), AIPlugin.Log.LogError);
            }

            return default;
            
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

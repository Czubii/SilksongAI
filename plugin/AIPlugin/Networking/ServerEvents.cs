using AIPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Networking
{
    public class ServerEvents
    {
        private Dictionary<string, Delegate> _eventRegistry = new Dictionary<string, Delegate>();

        public event Action OnNewArtifactCreated;
        public event Action OnTrainingFinished;
        public event Action OnInferecneArtifactSelected;
        public event Action<EventPayloads.TrainingEpoch> OnTrainingEpoch;
        
        private void RegisterEvent<T>(string server_key, Action<T> handler)
        {
            _eventRegistry[server_key] = handler;
        }
        
        public ServerEvents() 
        {
            RegisterEvent<object>("new_artifact", _ => OnNewArtifactCreated?.Invoke());
            RegisterEvent<object>("training_finished", _ => OnTrainingFinished?.Invoke());
            RegisterEvent<object>("inference_artifact_selected", _ => OnInferecneArtifactSelected?.Invoke());
            RegisterEvent<EventPayloads.TrainingEpoch>("training_epoch", payload => OnTrainingEpoch?.Invoke(payload));
        }

        public void RaiseEvent(string eventName, byte[] payload)
        {
            bool eventExists = _eventRegistry.TryGetValue(eventName, out Delegate action);

            if (!eventExists) 
            {
                ThreadSafeLogService.Log($"Unknown server event: {eventName}", AIPlugin.Log.LogError);
                return;
            }
            try
            {
                var delegateType = action.GetType();
                Type payloadType;

                if (delegateType.IsGenericType && delegateType.GetGenericTypeDefinition() == typeof(Action<>))
                {
                    payloadType = delegateType.GetGenericArguments()[0];
                }
                else
                {
                    // It's parameterless (wrapped as Action<object>)
                    payloadType = typeof(object);
                }

                // Deserialize bytes to the correct type
                object deserializedPayload;
                if (payloadType == typeof(object))
                {
                    deserializedPayload = null; // parameterless events ignore payload
                }
                else
                {
                    deserializedPayload = MessagePack.MessagePackSerializer.Deserialize(payloadType, payload);
                }

                MainThreadDispatcher.Enqueue(() => {
                    action.DynamicInvoke(deserializedPayload);
                });
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log($"Exception occured when rising event: {eventName} \n {ex}", 
                    AIPlugin.Log.LogError);
            }

        }

    }
}

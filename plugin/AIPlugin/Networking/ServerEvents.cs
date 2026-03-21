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
        private Dictionary<string, Action> _eventRegistry = new Dictionary<string, Action>();

        public event Action OnNewArtifactCreated;
        public event Action OnTrainingEpoch;

        public ServerEvents() 
        {
            _eventRegistry.Add("new_artifact", () => OnNewArtifactCreated?.Invoke());
            _eventRegistry.Add("training_epoch", () => OnTrainingEpoch?.Invoke());
        }

        public void RaiseEvent(string eventName, byte[] payload)
        {
            bool eventExists = _eventRegistry.TryGetValue(eventName, out Action action);

            if (!eventExists) 
            {
                ThreadSafeLogService.Log($"Unknown server event: {eventName}", AIPlugin.Log.LogError);
                return;
            }
            try
            {
                ThreadSafeLogService.Log($"Invoking event: {eventName}.",
                AIPlugin.Log.LogError);
                action?.Invoke();
            }
            catch (Exception ex)
            {
                ThreadSafeLogService.Log($"Exception occured when rising event: {eventName} \n {ex}", 
                    AIPlugin.Log.LogError);
            }

        }

    }
}

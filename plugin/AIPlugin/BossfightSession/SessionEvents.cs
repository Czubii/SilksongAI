using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public interface ISessionListener
    {
        void OnFightStarted();
        void OnFightFinished(AttemptResult result);
    }
    public class SessionEvents
    {
        private readonly List<ISessionListener> _listeners = new List<ISessionListener>();
        public void Subscribe(ISessionListener listener) => _listeners.Add(listener);
        public bool Unsubscribe(ISessionListener listener) => _listeners.Remove(listener);

        public void RaiseStarted()
        {
            foreach (var listener in _listeners)
                try
                {
                    listener.OnFightStarted();
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError(ex);
                }
        }
        public void RaiseFinished(AttemptResult result)
        {
            foreach (var listener in _listeners)
                try
                {
                    listener.OnFightFinished(result);
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError(ex);
                }
        }
    }
}


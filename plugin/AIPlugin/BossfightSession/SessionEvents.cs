using Steamworks;
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

    public interface IFrameCaptureListener
    {
        void OnFrameCaptured(RecordingFrame frameData);
        void OnCaptureStarted();
        void OnCaptureFinished(AttemptResult result);
    }
    public class SessionEvents
    {
        private readonly List<ISessionListener> _sessionListeners = new List<ISessionListener>();
        
        public void Subscribe(ISessionListener listener){

            if (!_sessionListeners.Contains(listener))
                _sessionListeners.Add(listener);
        } 
        public void Unsubscribe(ISessionListener listener)
        {
            _sessionListeners.Remove(listener);
        }

        public void RaiseStarted()
        {
            foreach (var listener in _sessionListeners)
                try
                {
                    listener?.OnFightStarted();
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError(ex);
                }
        }
        public void RaiseFinished(AttemptResult result)
        {
            foreach (var listener in _sessionListeners)
                try
                {
                    listener?.OnFightFinished(result);
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError(ex);
                }
        }
    }
}


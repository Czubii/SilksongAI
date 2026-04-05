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
        private readonly List<IFrameCaptureListener> _frameCaptureListeners = new List<IFrameCaptureListener>();
        public void Subscribe(ISessionListener listener){

            if (!_sessionListeners.Contains(listener))
                _sessionListeners.Add(listener);

            if(listener is IFrameCaptureListener frameListener && !_frameCaptureListeners.Contains(frameListener))
            {
                _frameCaptureListeners.Add(frameListener);
            }
        } 
        public void Unsubscribe(ISessionListener listener)
        {
            _sessionListeners.Remove(listener);

            if (listener is IFrameCaptureListener frameListener)
            {
                _frameCaptureListeners.Remove(frameListener);
            }
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

        public void RaiseFrameCaptured(RecordingFrame frameData)
        {
            foreach (var listener in _frameCaptureListeners)
                try
                {
                    listener?.OnFrameCaptured(frameData);
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError($"RaiseFrameCaptured: {ex}");
                }
        }
        public void RaiseCaptureStarted()
        {
            foreach (var listener in _frameCaptureListeners)
                try
                {
                    listener?.OnCaptureStarted();
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError($"RaiseCaptureStarted: {ex}");
                }
        }
        public void RaiseCaptureFinished(AttemptResult result)
        {
            foreach (var listener in _frameCaptureListeners)
                try
                {
                    listener?.OnCaptureFinished(result);
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError($"RaiseCaptureFinished: {ex}");
                }
        }
    }
}


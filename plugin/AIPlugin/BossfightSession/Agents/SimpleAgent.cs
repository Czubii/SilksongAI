using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System;

namespace AIPlugin.BossfightSession.Agents
{
    /// <summary>
    /// Colects the frame data, sends reqest to ai server, and forwards the ai controls further to be applied
    /// </summary>
    public class SimpleAgent : BaseAgent
    {
        protected Requests.InferenceAction _actionRequest = null;
        protected override void CreateActionRequest(RecordingFrame frame, Action<FrameUserInputs> successCallback, out Func<bool> requestFinished)
        {
            _actionRequest = new Requests.InferenceAction(service.Gateway, InferenceFrame.FromRecordingFrameData(frame));
            _actionRequest.OnSuccess += successCallback;

            requestFinished = _actionRequest.Finished;
        }
    }

   
}

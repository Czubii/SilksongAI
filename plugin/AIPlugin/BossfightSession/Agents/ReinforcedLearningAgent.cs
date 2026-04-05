using AIPlugin.BossfightSession.Agents;
using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public class ReinforcedLearningAgent: BaseAgent
    {
        protected Requests.PredictAction _actionRequest = null;
        protected override void CreateActionRequest(RecordingFrame frame, Action<FrameUserInputs> successCallback, out Func<bool> requestFinished)
        {
            _actionRequest = new Requests.PredictAction(service.Gateway, InferenceFrame.FromRecordingFrameData(frame));
            _actionRequest.OnSuccess += successCallback;

            requestFinished = _actionRequest.Finished;
        }
    }
}

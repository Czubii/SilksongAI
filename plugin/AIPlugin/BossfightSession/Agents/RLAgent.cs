using AIPlugin.BossfightSession.Agents;
using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIPlugin.Networking.Requests.Requests;

namespace AIPlugin.BossfightSession
{
    public class RLAgent: BaseAgent
    {
        protected InferenceAction _actionRequest = null;

        private RLFightFisihedRefreshable _rlfightFinished;

        public override void Initialize(AiService service)
        {
            base.Initialize(service);
            _rlfightFinished = new RLFightFisihedRefreshable(service.Gateway, () => new RLFightFisihed(service.Gateway));
        }
        public override void OnCaptureFinished(AttemptResult result)
        {
            base.OnCaptureFinished(result);
            _rlfightFinished.Send();
        }
        
        protected override void CreateActionRequest(RecordingFrame frame, Action<FrameUserInputs> successCallback, out Func<bool> requestFinished)
        {
            _actionRequest = new InferenceAction(service.Gateway, InferenceFrame.FromRecordingFrameData(frame));
            _actionRequest.OnSuccess += successCallback;

            requestFinished = _actionRequest.Finished;
        }
    }
}

using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public class SessionDispatcher
    {

        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AIBossfightAgent _agent;
        private AiService _service;

        private Requests.GetInferenceArtifactRefreshable _inferenceArtifactRequester;
        public (string BossName, string ArtifactName)? SelectedArtifact = null;

        public SessionDispatcher(
            SessionOrchestrator sessionOrchestrator,
            BossfightRecorder recorder,
            AIBossfightAgent aiAgent,
            AiService service) 
        { 
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _agent = aiAgent;
            _service = service;

            _inferenceArtifactRequester = 
                new Requests.GetInferenceArtifactRefreshable(_service.Gateway, 
                () => new Requests.GetInferenceArtifact(_service.Gateway));

            service.OnConnected += _inferenceArtifactRequester.Send;
            service.Gateway.Events.OnInferecneArtifactSelected += _inferenceArtifactRequester.Send;
            _inferenceArtifactRequester.OnSuccess += UpdateArtifactSelection;
        }
        private void UpdateArtifactSelection(Requests.GetInferenceArtifact.Response selection)
        {
            SelectedArtifact = (selection.TargetBossName, selection.ArtifactName);
        }
        public bool CanStartAiSession()
        {
            return (_service?.IsConnected ?? false) && SelectedArtifact != null;
        }
        public bool CanStartSession() => _sessionOrchestrator.CanStart();
        public bool IsSessionActive() => _sessionOrchestrator.IsSessionActive();
        public void StartSessionLocaly(
            int numFights, 
            BossMetadata boss, 
            SessionContext.SessionSettings settings, 
            bool record)
        {
            if(!_sessionOrchestrator?.CanStart() ?? true) return;

            var context = new SessionContext(boss, numFights, settings);

            _recorder.enabled = record;

            _sessionOrchestrator.TryStart(context);
        }
        public void StartAiSessionLocaly(
            int numFights,
            SessionContext.SessionSettings settings,
            bool record)
        {
            if (!CanStartAiSession())
            {
                AIPlugin.Log.LogWarning("Cannot Start AI session as some things had not beed set up yet");
                return;
            }

        }
        public void RequestStop(string reason = null)
        { 
            if (reason == null)
                _sessionOrchestrator.RequestStop();
            else
                _sessionOrchestrator.RequestStop(reason);
        }

    }
}

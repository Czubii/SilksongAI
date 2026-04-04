using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using AIPlugin.PluginGUI.Windows;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public class SessionDispatcher
    {
        private ArtifactSelection _artifactSelection;
        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AIBossfightAgent _agent;
        private AiService _service;

        private Requests.InitializeLiveInference _initializeInferenceRequest = null;

        public SessionDispatcher(
            ArtifactSelection selection,
            SessionOrchestrator sessionOrchestrator,
            BossfightRecorder recorder,
            AIBossfightAgent aiAgent,
            AiService service) 
        { 
            _artifactSelection = selection; 
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _agent = aiAgent;
            _service = service;
        }
        public bool CanStartSession() => (_sessionOrchestrator?.CanStart() ?? false) & !InitializationInProgress();
        public bool CanStartAiSession() => CanStartSession() && _artifactSelection.AnySelected && _service.IsConnected;
        private bool InitializationInProgress() => !_initializeInferenceRequest?.Finished() ?? false;
        public bool IsSessionActive() => _sessionOrchestrator.IsSessionActive();

        private void StartSessionInternal(
            int numFights,
            BossMetadata boss,
            SessionContext.SessionSettings settings,
            bool record,
            bool aiEnabled)
        {

            var context = new SessionContext(boss, numFights, settings);

            _recorder.enabled = record;
            _agent.enabled = aiEnabled;
           

            _sessionOrchestrator.TryStart(context);
        }
        public void StartSession(
            int numFights, 
            BossMetadata boss, 
            SessionContext.SessionSettings settings, 
            bool record)
        {
            if(!CanStartSession()) return;
            StartSessionInternal(numFights, boss, settings, record, false);    
        }
        public void StartAiSession(
            int numFights,
            SessionContext.SessionSettings settings,
            bool record,
            Action<string> OnServerError = null)
        {
            if(!CanStartAiSession()) return;

            var boss = BossReferenceDatabase.All.Find(x => x.InternalName == _artifactSelection.Artifact.BossName);
            if (boss == null) throw new Exception($"Could not find boss with name {_artifactSelection.Artifact.BossName}");

            var context = new SessionContext(boss, numFights, settings);

            var payload = new Requests.InitializeLiveInference.Payload()
            {
                ArtifactName = _artifactSelection.Artifact.Name,
                TargetBossName = _artifactSelection.Artifact.BossName,
            };

            _initializeInferenceRequest = new Requests.InitializeLiveInference(_service.Gateway, payload);

            _initializeInferenceRequest.OnError += OnServerError;
            _initializeInferenceRequest.OnSuccess += (_) => {
                StartSessionInternal(numFights, boss, settings, record, true);
            };

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

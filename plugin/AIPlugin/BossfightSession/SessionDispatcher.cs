using AIPlugin.BossfightSession.Agents;
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
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

namespace AIPlugin.BossfightSession
{
    public class SessionDispatcher
    {
        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AiService _service;
        private AgentManager _agentManager;

        private Requests.InitializeInferenceSession _initializeInferenceRequest = null;

        public SessionDispatcher(
            SessionOrchestrator sessionOrchestrator,
            BossfightRecorder recorder,
            AgentManager agentManager,
            AiService service) 
        { 
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _service = service;
            _agentManager = agentManager;
        }
        public bool CanStartSession() => (_sessionOrchestrator?.CanStart() ?? false) & !InitializationInProgress();
        private bool InitializationInProgress() => !_initializeInferenceRequest?.Finished() ?? false;
        public bool IsSessionActive() => _sessionOrchestrator.IsSessionActive();

        private void StartSessionInternal(
            int numFights,
            BossMetadata boss,
            SessionContext.SessionSettings settings,
            bool record,
            AgentManager.AgentSelection agent)
        {
            var context = new SessionContext(boss, numFights, settings);

            _agentManager.SelectAgent(agent);
            _recorder.enabled = record;
            

            _sessionOrchestrator.TryStart(context);
        }

        public void StartSession(
            int numFights, 
            BossMetadata boss, 
            SessionContext.SessionSettings settings, 
            bool record)
        {
            if(!CanStartSession()) return;
            StartSessionInternal(numFights, boss, settings, record, AgentManager.AgentSelection.None);    
        }
        public void StartAiSession(
            int numFights,
            ArtifactSelection artifactSelection,
            SessionContext.SessionSettings settings,
            bool record,
            Action<string> OnServerError = null)
        {
            if(!CanStartSession()) return;

            var boss = BossReferenceDatabase.All.Find(x => x.InternalName == artifactSelection.Artifact.BossName);
            if (boss == null) throw new Exception($"Could not find boss with name {artifactSelection.Artifact.BossName}");

            var payload = new Requests.InitializeInferenceSession.Payload()
            {
                ArtifactName = artifactSelection.Artifact.Name,
                TargetBossName = artifactSelection.Artifact.BossName,
            };

            _initializeInferenceRequest = new Requests.InitializeInferenceSession(_service.Gateway, payload);

            _initializeInferenceRequest.OnError += OnServerError;
            _initializeInferenceRequest.OnSuccess += (_) => {
                StartSessionInternal(numFights, boss, settings, record, AgentManager.AgentSelection.Simple);
            };
        }
        public void StartRLFight(string BossName)
        {
            var boss = BossReferenceDatabase.All.Find(x => x.InternalName == BossName);
            var settings = new SessionContext.SessionSettings(false, false, false);

            StartSessionInternal(1, boss, settings, false, AgentManager.AgentSelection.ReinforcementLearning);
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

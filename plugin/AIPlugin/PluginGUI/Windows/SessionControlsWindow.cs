using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AIPlugin.PluginGUI.CustomGUI;

namespace AIPlugin.PluginGUI
{
    /// <summary>
    /// Responsible for starting session / selecting boss / basic session settings like the number of 
    /// trials and boss selection
    /// </summary>
    public class SessionControlsWindow: BaseWindow
    {
        private class SessionForm: IForm
        {
            public CustomGUI.DropdownState<BossMetadata> BossDropdownState = new CustomGUI.DropdownState<BossMetadata>();
            public int NumFights = 1;
            public bool RecordingEnabled = false;
            public bool KeepTools = false;
            public bool KeepAbilities = false;

            public Vector2 Scroll = new Vector2();

            public bool IsValid()
            {
                return NumFights > 0;
            }
        }

        private SessionForm _sessionForm = new SessionForm();

        private bool _aiEnabled = false;

        private SessionDispatcher _dispatcher;

        private AiService _service;

        private Requests.GetArtifactsRefreshable _artifactRequester;
        private Requests.SetInferenceArtifact _artifactSetter = null;

        private readonly List<(string, BossMetadata)> _bossDropdownElements;

        private CustomGUI.ArtifactSelectionDropdowns _aiModeArtifactSelection;

        public SessionControlsWindow(
            string name, 
            AiService service,
            SessionDispatcher dispatcher): 
            base(name, new Rect(100, 100, 300, 400))
        {
            _dispatcher = dispatcher;
            _service = service;
            var bossDisplayNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();

            _artifactRequester = new Requests.GetArtifactsRefreshable(service.Gateway,
                () => new Requests.GetArtifacts(service.Gateway));

            service.Gateway.Events.OnNewArtifactCreated += _artifactRequester.Send;
            service.OnConnected += _artifactRequester.Send;
            _artifactRequester.OnSuccess += OnArtifactResults;

            _bossDropdownElements = bossDisplayNames.Zip(BossReferenceDatabase.All.ToList(), (a, b) => (a, b)).ToList();
            _aiModeArtifactSelection = new CustomGUI.ArtifactSelectionDropdowns();
        }
        private void OnArtifactResults(Requests.GetArtifacts.Response artifacts)
        {
            _aiModeArtifactSelection.UpdateElements(artifacts.BossArtifacts);
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUI.enabled = _service.IsConnected;
            if (!_service.IsConnected) _aiEnabled = false;
            _aiEnabled = LabeledToggle(_aiEnabled, "Use AI");
            GUI.enabled = true;

            if (!_aiEnabled) DrawSessionForm();
            else DrawAiSessionForm();
        }
        public void DrawSessionForm()
        {
            UI.Form(_sessionForm)
                .BeginScrollView(x => x.Scroll)
                .Dropdown("Boss", _bossDropdownElements, x => x.BossDropdownState)
                .IntegerField("Number of Fights: ", x => x.NumFights, GUILayout.Width(120))
                .Toggle("Recording Enabled:", x => x.RecordingEnabled)
                .Toggle("Keep Crest and Tools:", x => x.KeepTools)
                .Toggle("Keep Abilities and Health:", x => x.KeepAbilities)
                .EndScrollView()
                .FlexibleSpace()
                .Button("Start Session", StartSession, CanStart())
                .Button("Stop Session", StopSession, CanStop())
                .End();
        }
        public void DrawAiSessionForm()
        {
            UI.Form(_sessionForm)
                .BeginScrollView(x => x.Scroll)
                .CustomAction(_aiModeArtifactSelection.Draw)
                .Space(10)
                .Button("Apply", () => {
                    var selection = _aiModeArtifactSelection.SelectedOption;
                    var payload = new Requests.SetInferenceArtifact.Payload()
                    {
                        ArtifactName = selection.ArtifactName,
                        TargetBossName = selection.BossName
                    };

                    _artifactSetter = new Requests.SetInferenceArtifact(_service.Gateway, payload);
                    _artifactSetter.OnError += NotifyError;

                }, _artifactSetter?.Finished() ?? true &&
                   _aiModeArtifactSelection.SelectionValid())
                .Space(30)
                .IntegerField("Number of Fights: ", x => x.NumFights, GUILayout.Width(120))
                .Toggle("Recording Enabled:", x => x.RecordingEnabled)
                .Toggle("Keep Crest and Tools:", x => x.KeepTools)
                .Toggle("Keep Abilities and Health:", x => x.KeepAbilities)
                .EndScrollView()
                .FlexibleSpace()
                .Button("Start Session", StartAiSession, CanStartAi())
                .Button("Stop Session", StopSession, CanStop())
                .End();
        }
        private bool CanStart() => _dispatcher.CanStartSession() && _sessionForm.IsValid();
        private void StartSession()
        {
            var settings = new SessionContext.SessionSettings(_sessionForm.KeepAbilities, _sessionForm.KeepTools);
            //TODO add this functionality finally

            var boss = _sessionForm.BossDropdownState.SelectedOption;
            _dispatcher.StartSessionLocaly(_sessionForm.NumFights, boss, settings, _sessionForm.RecordingEnabled);
        }
        private bool CanStartAi()
        {
            var GUIArtifactSelection = _aiModeArtifactSelection.SelectedOption;
            return _dispatcher.CanStartAiSession()
                && _sessionForm.IsValid()
                && _dispatcher.SelectedArtifact != null
                && _dispatcher.SelectedArtifact.Value.ArtifactName == GUIArtifactSelection.ArtifactName
                && _dispatcher.SelectedArtifact.Value.BossName == GUIArtifactSelection.BossName;
        }
        private void StartAiSession()
        {
            var settings = new SessionContext.SessionSettings(_sessionForm.KeepAbilities, _sessionForm.KeepTools);
            //TODO add this functionality finally

            _dispatcher.StartAiSessionLocaly(_sessionForm.NumFights, settings, _sessionForm.RecordingEnabled);        
        }
        private bool CanStop() => _dispatcher.IsSessionActive();
        private void StopSession()
        {
            _dispatcher.RequestStop();
        }
    }
}

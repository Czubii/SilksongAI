using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using AIPlugin.PluginGUI.Windows;
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

        private SessionDispatcher _dispatcher;

        private readonly List<(string, BossMetadata)> _bossDropdownElements;

        private ArtifactSelection _artifactSelection;

        public SessionControlsWindow(
            string name, 
            ArtifactSelection artifactSelection,
            SessionDispatcher dispatcher): 
            base(name, new Rect(100, 100, 350, 500))
        {
            _artifactSelection = artifactSelection; 
            _dispatcher = dispatcher;
            var bossDisplayNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();

            _bossDropdownElements = bossDisplayNames.Zip(BossReferenceDatabase.All.ToList(), (a, b) => (a, b)).ToList();
        }

        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            UI.Form(_sessionForm)
               .BeginScrollView(x => x.Scroll)

               .GUIEnabled(!_dispatcher.IsSessionActive())
               .BeginCard("Session Settings: ", Styles.Card)
               .IntegerField("Number of Fights: ", x => x.NumFights, GUILayout.Width(120))
               .Toggle("Recording Enabled:", x => x.RecordingEnabled)
               .Toggle("Keep Crest and Tools:", x => x.KeepTools)
               .Toggle("Keep Abilities and Health:", x => x.KeepAbilities)
               .EndCard()

               .EndScrollView()

               .BeginCard("Session: ", Styles.Card)
               .Dropdown("Boss", _bossDropdownElements, x => x.BossDropdownState)
               .Button("Start", StartSession, CanStart())
               .EndCard()

               .BeginCard("AI Session: ", Styles.Card)
               .CustomAction(() => {
                   ArtifactSelectionCard(_artifactSelection);
               })
               .Button("Start", StartAiSession, CanStartAi())
               .EndCard()

               .GUIEnabled(true)
               .Button("Stop Session", StopSession, CanStop())
               .End();
        }
        private bool CanStart() => _dispatcher.CanStartSession() && _sessionForm.IsValid();
        private bool CanStartAi() => CanStart() && _artifactSelection.AnySelected;
        private void StartSession()
        {
            var settings = new SessionContext.SessionSettings(_sessionForm.KeepAbilities, _sessionForm.KeepTools);
            //TODO add this functionality finally

            var boss = _sessionForm.BossDropdownState.SelectedOption;
            _dispatcher.StartSession(_sessionForm.NumFights, boss, settings, _sessionForm.RecordingEnabled);
        }
        private void StartAiSession()
        {
            var settings = new SessionContext.SessionSettings(_sessionForm.KeepAbilities, _sessionForm.KeepTools);
            //TODO add this functionality finally

            _dispatcher.StartAiSession(_sessionForm.NumFights, _artifactSelection, settings, _sessionForm.RecordingEnabled, NotifyError);        
        }
        private bool CanStop() => _dispatcher.IsSessionActive();
        private void StopSession()
        {
            _dispatcher.RequestStop();
        }
    }
}

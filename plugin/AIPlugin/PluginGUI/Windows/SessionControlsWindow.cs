using AIPlugin.BossfightSession;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.CullingGroup;

namespace AIPlugin.PluginGUI
{
    /// <summary>
    /// Responsible for starting session / selecting boss / basic session settings like the number of 
    /// trials and boss selection
    /// </summary>
    public class SessionControlsWindow: BaseWindow
    {
        private class Form: IForm
        {
            public CustomGUI.DropdownState<BossMetadata> BossDropdownState = new CustomGUI.DropdownState<BossMetadata>();
            public int NumFights = 1;
            public bool RecordingEnabled = false;
            public bool AiEnabled = false;
            public bool KeepTools = false;

            public Vector2 Scroll = new Vector2();

            public bool IsValid()
            {
                return NumFights > 0;
            }
        }

        private Form form = new Form();

        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AIBossfightAgent _aiBossfightAgent;

        private readonly List<(string, BossMetadata)> _bossDropdownElements;

        public SessionControlsWindow(
            string name, 
            SessionOrchestrator sessionOrchestrator, 
            BossfightRecorder recorder, 
            AIBossfightAgent aiAgent): 

            base(name, new Rect(100, 100, 200, 300))
        {
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _aiBossfightAgent = aiAgent;

            var bossDisplayNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();

            _bossDropdownElements = bossDisplayNames.Zip(BossReferenceDatabase.All.ToList(), (a, b) => (a, b)).ToList();
        }
        public override bool CanEnable() => true;
        private bool CanStart() => _sessionOrchestrator.CanStart() && form.IsValid();
        private bool CanStop() => _sessionOrchestrator.IsSessionActive();
        public override void DrawContent()
        {
            UI.Form(form)
                .BeginScrollView(x => x.Scroll)
                .Dropdown("Boss:", _bossDropdownElements, x => x.BossDropdownState)
                .IntegerField("Number of Fights: ", x => x.NumFights)
                .Toggle("Recording Enabled:", x => x.RecordingEnabled)
                .Toggle("Keep Tools:", x => x.KeepTools)
                .EndScrollView()
                .FlexibleSpace()
                .GUIEnabled(CanStart())
                .Button("Start Session", StartSession)
                .GUIEnabled(CanStop())
                .Button("Stop Session", StopSession)
                .End();
        }

        private void StartSession()
        {
            var context = new SessionContext(form.BossDropdownState.SelectedOption, form.NumFights,
                new SessionContext.SessionSettings(false, false));//TODO add this functionality finally

            _sessionOrchestrator.TryStart(context);
        }
        private void StopSession()
        {
            _sessionOrchestrator.RequestStop();
        }
    }
}

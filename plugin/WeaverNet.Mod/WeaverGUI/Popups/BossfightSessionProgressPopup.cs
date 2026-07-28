using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Popups
{
    public class BossfightSessionProgressPopup : BasePopup
    {
        public override bool AlwaysVisible => true;
        private readonly IBossfightSessionOrchestrator _orchestrator;
        private readonly IBossfightSessionStatus _sessionStatus;
        public BossfightSessionProgressPopup(
            IBossfightSessionOrchestrator orchestrator,
            IBossfightSessionStatus sessionStatus) : 
            base(PopupLayer.Floating, 
                new Vector2(Screen.width - 310f, 10f), 
                new Vector2(300f, 100f))
        {
            _sessionStatus = sessionStatus;
            _orchestrator = orchestrator;
        }
        protected override void DrawContent()
        {
            if (!IsAlive) return;

            if (_sessionStatus.IsRunning)
            {
                GUILayout.BeginVertical();
                PluginGUI.Label($"Boss: {_sessionStatus.CurrentBoss.DisplayName}", WeaverNetStyles.ElementLabelTitle);
                GUILayout.Space(8);
                if (_sessionStatus.Progress.Percentage != null)
                    PluginGUI.ProgressBar((float)_sessionStatus.Progress.Percentage, _sessionStatus.Progress.Message);

                GUILayout.Space(8);

                if (PluginGUI.Button("Stop Session"))
                {
                    _ = _orchestrator.StopAsync();
                }

                GUILayout.EndVertical();
            }
            else
            {
                PluginGUI.Label($"Session Not Active", WeaverNetStyles.ElementLabelTitle);
            }

        }
    }
}

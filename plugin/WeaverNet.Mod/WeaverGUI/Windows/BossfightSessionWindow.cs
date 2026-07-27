using System.Collections.ObjectModel;
using UnityEngine;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Views;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    internal class BossfightSessionWindow: MultiViewWindow
    {
        private readonly IBossfightSessionOrchestrator _orchestrator;
        private readonly IBossfightSessionStatus _sessionStatus;
        public BossfightSessionWindow(
            string name,
            IBossfightSessionOrchestrator orchestrator,
            IBossfightSessionStatus sessionStatus,
            StandardBossfightSetupView standardBossfightSetupView)
            : base(name, new Rect(0, 0, 350, 400))
        {
            _sessionStatus = sessionStatus;
            _orchestrator = orchestrator;

            _sessionStatus.StatusChanged += SessionStatusChanged;

            AddView("Main", DrawMain, "Session Type Selection");
            AddView("SessionProgress", DrawSessionProgress, "Session Progress");
            AddView("StandardBossfightSetup", standardBossfightSetupView, "Player Fight Session Configuration");
        }
        public override bool CanEnable() => true;
        private void SessionStatusChanged()
        {
            if(_sessionStatus.IsRunning && base.CurrentView != "SessionProgress")
            {
                SwitchView("SessionProgress");
            }
            else if (!_sessionStatus.IsRunning && base.CurrentView == "SessionProgress")
            {
                SwitchView("Main");
            }
        }
        private void DrawMain()
        {

            if(PluginGUI.Button("Player Fight Session"))
            {
                SwitchView("StandardBossfightSetup");
            }
        }
        private void DrawSessionProgress()
        {
            GUILayout.BeginVertical();
            PluginGUI.Label($"Boss: {_sessionStatus.CurrentBoss.DisplayName}");

            if (_sessionStatus.Progress.Percentage != null)
                PluginGUI.ProgressBar((float)_sessionStatus.Progress.Percentage, _sessionStatus.Progress.Message);

            if (PluginGUI.Button("Stop Session"))
            {
                _ = _orchestrator.StopAsync();
            }
            GUILayout.EndVertical();
        }
    }
}

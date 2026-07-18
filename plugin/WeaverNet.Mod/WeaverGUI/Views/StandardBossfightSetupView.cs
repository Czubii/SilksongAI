using System;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;

namespace WeaverNet.Mod.WeaverGUI.Views
{
    public class StandardBossfightSetupView: IView
    {
        private class ViewState
        {
            public Vector2 Scroll = new Vector2();
            public bool KeepCurrentLoadout = false;
            public DropdownState<Loadout> LoadoutDropdownState = new DropdownState<Loadout>();
            public DropdownState<BossData> BossDropdownState = new DropdownState<BossData>();
            public IntFieldState NumberOfAttempts = new IntFieldState(1);
        }

        private readonly IBossfightCatalog _catalog;
        private readonly IBossfightSessionOrchestrator _orchestrator;
        private ViewState _state = new ViewState();
        public event Action<string> ViewRequested;
        public StandardBossfightSetupView(
            IBossfightCatalog catalog,
            IBossfightSessionOrchestrator bossfightSessionOrchestrator)
        {
            _catalog = catalog;
            _orchestrator = bossfightSessionOrchestrator;
        }
        public void Draw()
        {
            _state.Scroll = PluginGUI.BeginScrollView(_state.Scroll);
            _state.KeepCurrentLoadout =
                PluginGUI.LabeledToggle(_state.KeepCurrentLoadout, "Keep Current Loadout");

            GUI.enabled = !_state.KeepCurrentLoadout;
            _state.LoadoutDropdownState = PluginGUI.Dropdown(_state.LoadoutDropdownState, _catalog.Loadouts, a => a.Name, "Loadout");
            GUI.enabled = true;

            _state.BossDropdownState =
                PluginGUI.Dropdown(_state.BossDropdownState, _catalog.Bosses, a=>a.DisplayName, "Boss");

            _state.NumberOfAttempts =
                PluginGUI.IntField(_state.NumberOfAttempts, "Number Of Attempts", 1, 100);

            PluginGUI.EndScrollView();
            PluginGUI.BeginHorizontal();
            if (PluginGUI.Button("Cancel")) ViewRequested?.Invoke("Main");
            GUI.enabled = _orchestrator.CanStart();
            if (PluginGUI.Button("Start"))
            {
                //var session = new BossfightSessionBuilder(
                //    _state.BossDropdownState.SelectedOption,
                //    new IterationBoundary(_state.NumberOfAttempts.Value),
                //    new LoadoutManager()).Build();
                //_ = _orchestrator.StartAsync(session, CancellationToken.None);
            }
            GUI.enabled = true;
            PluginGUI.EndHorizontal();
        }
    }
}

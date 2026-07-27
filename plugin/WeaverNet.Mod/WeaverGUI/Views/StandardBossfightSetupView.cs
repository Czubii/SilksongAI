using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Boundaries;
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
        private readonly IBossfightSessionAssembler _sessionAssembler;
        private readonly IBossfightSessionOrchestrator _orchestrator;
        private ViewState _state = new ViewState();
        public event Action<string> ViewRequested;
        public StandardBossfightSetupView(
            IBossfightCatalog catalog,
            IBossfightSessionAssembler sessionAssembler,
            IBossfightSessionOrchestrator bossfightSessionOrchestrator)
        {
            _catalog = catalog;
            _sessionAssembler = sessionAssembler;
            _orchestrator = bossfightSessionOrchestrator;
        }
        public void Draw()
        {
            _state.Scroll = PluginGUI.BeginScrollView(_state.Scroll);
            _state.KeepCurrentLoadout =
                PluginGUI.Toggle(_state.KeepCurrentLoadout);

            GUI.enabled = !_state.KeepCurrentLoadout;
            //_state.LoadoutDropdownState = PluginGUI.Dropdown(_state.LoadoutDropdownState, _catalog.Loadouts, a => a.Name);
            //GUI.enabled = true;

            //_state.BossDropdownState =
            //    PluginGUI.Dropdown(_state.BossDropdownState, _catalog.Bosses, a=>a.DisplayName);

            _state.NumberOfAttempts =
                PluginGUI.IntField(_state.NumberOfAttempts, 1, 100);

            PluginGUI.EndScrollView();
            PluginGUI.BeginHorizontal();
            if (PluginGUI.Button("Cancel")) ViewRequested?.Invoke("Main");
            GUI.enabled = _orchestrator.CanStart;
            if (PluginGUI.Button("Start"))
            {
                StartSession();
            }
            GUI.enabled = true;
            PluginGUI.EndHorizontal();
        }

        private void StartSession()
        {
            //Loadout loadout;
            //if (_state.KeepCurrentLoadout) // TODO
            //{
            //    //loadout = 
            //}
            var sessionConfig = new BossfightSessionConfiguration(
                _state.BossDropdownState.SelectedOption,
                _state.LoadoutDropdownState.SelectedOption,
                new IterationBoundary(_state.NumberOfAttempts.Value), //TODO: how to pass parameters for this? List of objects under one interface containing the configurations for each feature?
                new List<BossfightSessionOption>() // no features for now //TODO: how to pass parameters for this? List of objects under one interface containing the configurations for each feature?
                );

            var session = _sessionAssembler.Assemble(sessionConfig);

            _ = _orchestrator.StartAsync(session, CancellationToken.None); // fire and forget
        }
    }
}

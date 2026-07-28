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
using WeaverNet.Mod.WeaverGUI.Popups;

namespace WeaverNet.Mod.WeaverGUI.Views
{
    public class StandardBossfightSetupView: BaseView
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
        private readonly IBossfightSessionStatus _sessionStatus;
        private ViewState _state = new ViewState();
        public StandardBossfightSetupView(
            IBossfightCatalog catalog,
            IBossfightSessionAssembler sessionAssembler,
            IBossfightSessionOrchestrator bossfightSessionOrchestrator,
            IBossfightSessionStatus sessionStatus)
        {
            _catalog = catalog;
            _sessionAssembler = sessionAssembler;
            _orchestrator = bossfightSessionOrchestrator;
            _sessionStatus = sessionStatus;
        }
        public override void DrawContent()
        {
            _state.Scroll = PluginGUI.BeginScrollView(_state.Scroll);
            _state.KeepCurrentLoadout = PluginGUI.Labeled("Keep loadout",
                () => PluginGUI.Toggle(_state.KeepCurrentLoadout),
                "Keep current health, tools, etc.");

            GUI.enabled = !_state.KeepCurrentLoadout;
            _state.LoadoutDropdownState = PluginGUI.Labeled("Loadout", 
                () => PluginGUI.Dropdown(Context, _state.LoadoutDropdownState, _catalog.Loadouts, a => a.Name),
                "Select loadout used during bossfights (the current one will be reverted after the session)");
            GUI.enabled = true;

            _state.BossDropdownState = PluginGUI.Labeled("Boss",
                () => PluginGUI.Dropdown(Context, _state.BossDropdownState, _catalog.Bosses, a => a.DisplayName),
                "Select the boss to fight");

            _state.NumberOfAttempts = PluginGUI.Labeled("Fights",
                () => PluginGUI.IntField(_state.NumberOfAttempts, 1, 100),
                "Total number of fights");

            PluginGUI.EndScrollView();
            GUILayout.FlexibleSpace();
            PluginGUI.HorizontalLine();
            GUILayout.Space(6);
            PluginGUI.BeginHorizontal();
            if (PluginGUI.Button("Cancel")) RequestView("Main");
            GUILayout.Space(10);
            GUI.enabled = _orchestrator.CanStart;
            if (PluginGUI.Button("Start"))
            {
                var confirmPopup = new ConfirmPopup(WindowRect,
                    "Are you sure?",
                    "Are you sure you want to start the session?",
                    () => StartSession());
                ShowPopup(confirmPopup);
            }
            GUI.enabled = true;
            PluginGUI.EndHorizontal();
        }

        private async void StartSession()
        {
            var progressPopup = new BossfightSessionProgressPopup(_orchestrator, _sessionStatus);

            try
            {
                var sessionConfig = new BossfightSessionConfiguration(
                    _state.BossDropdownState.SelectedOption,
                    _state.LoadoutDropdownState.SelectedOption,
                    new IterationBoundary(_state.NumberOfAttempts.Value), //TODO: how to pass parameters for this? List of objects under one interface containing the configurations for each feature?
                    new List<BossfightSessionOption>() // no features for now //TODO: how to pass parameters for this? List of objects under one interface containing the configurations for each feature?
                    );

                var session = _sessionAssembler.Assemble(sessionConfig);
                var sessionTask = _orchestrator.StartAsync(session, CancellationToken.None);

                Context.HideGUI();
                ShowPopup(progressPopup);

                await sessionTask;

                // Session completed successfully
                var successPopup = new NotificationPopup(
                    WindowRect,
                    "Session Complete",
                    "The has conlcuded without any issues."
                );
                ShowPopup(successPopup);
            }
            catch (Exception ex)
            {
                var errorPopup = new ErrorPopup(WindowRect, "Error", $"Exception met during the session: \n {ex.Message}");
                ShowPopup(errorPopup);
            }
            finally
            {
                Context.ShowGUI();
                progressPopup.Close();
            }

        }
    }
}

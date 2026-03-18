using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using BepInEx.Configuration;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AIPlugin.PluginGUI
{
    /// <summary>
    /// Responsible for starting session / selecting boss / basic session settings like the number of trials and boss selection
    /// </summary>
    public class ArtifactCreatorWindow : BaseWindow//TODO: ENABLE CURSOUR WHEN WINDOW ACTIVE
    {
        private enum State
        {
            ArtifactSetup,
            ArtifactCreationResults
        }

        private State _state = State.ArtifactSetup;

        private Vector2 _scroll = new Vector2();

        AiService _service;

        private CustomGUI.DropdownState _architectureDropdownState = new CustomGUI.DropdownState();
        private CustomGUI.DropdownState _newModelBossDropdownState = new CustomGUI.DropdownState();

        private ServerRequestTask<Responses.Architectures> _architectureRequest = null;
        private ServerRequestTask<EmptyPayload, Requests.NewModel> _newModelRequest = null;

        private Requests.NewModel _newModelConfig = null;

        public ArtifactCreatorWindow(string name, AiService service):
            base(name, new Rect(100, 300, 250, 500))
        {
            _service = service;

            service.OnConnected += OnConnected;
            service.OnDisconnected += OnDisconnected;
        }

        private void OnConnected()
        {
            if (_architectureRequest == null)
                _architectureRequest = new ServerRequestTask<Responses.Architectures>(_service.Gateway,
                    RequestType.get_architectures);
        }
        private void OnDisconnected()
        {
            _architectureRequest = null;
        }
        public override bool CanEnable() => _service.IsConnected;
        public override void DrawContent()
        {
            if (_architectureRequest == null || !_architectureRequest.FinishedWithSuccess()) return;
            switch (_state)
            {
                case State.ArtifactSetup:
                    DrawArtifactSetup();
                    break;
                case State.ArtifactCreationResults:
                    DrawArtifactCreationResults();
                    break;
            }
        }

        private void DrawArtifactSetup()
        {

            var architectures = _architectureRequest.Result;

            GUILayout.BeginVertical();
            _scroll = GUILayout.BeginScrollView(_scroll, Styles.ScrollView, Styles.VerticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.skin.verticalScrollbarThumb = Styles.VerticalScrollbarThumb;

            GUILayout.Label("The dataset used to train the model using behavioral " +
                "cloning is created based on recordings at the time of creating the " +
                "artifact and cannot be changed later (unless done manually). Make sure all the recordings " +
                "you want to use exist in the respecive directory before creating the model.");

            bool anyParamEmpty = false;
            List<string> architectureNames = architectures.ArchitectureParams.Keys.ToList();
            GUILayout.Label("Architecture: ");
            var oldIdx = _architectureDropdownState.SelectedIdx;
            _architectureDropdownState =
                CustomGUI.Dropdown(_architectureDropdownState, architectureNames);

            var selectedArchitectureName = architectureNames[_architectureDropdownState.SelectedIdx];

            if (oldIdx != _architectureDropdownState.SelectedIdx || _newModelConfig == null)
            {
                _newModelConfig = new Requests.NewModel()
                {
                    ArchitectureName = selectedArchitectureName,
                    TargetBossName = BossReferenceDatabase.All.Select(s => s.InternalName).
                        ToList()[_newModelBossDropdownState.SelectedIdx],
                    ModelName = "",
                    Params = architectures.ArchitectureParams[selectedArchitectureName],
                    Overwrite = false
                };
            }
            GUILayout.Label("Target Boss:");
            List<string> bossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
            oldIdx = _newModelBossDropdownState.SelectedIdx;
            _newModelBossDropdownState = CustomGUI.Dropdown(_newModelBossDropdownState, bossNames);
            if (oldIdx != _newModelBossDropdownState.SelectedIdx)
            {
                _newModelConfig.TargetBossName = BossReferenceDatabase.All.Select(s => s.InternalName).
                        ToList()[_newModelBossDropdownState.SelectedIdx];
            }

            GUILayout.Space(20.0f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Artifact Name:");
            GUILayout.FlexibleSpace();
            _newModelConfig.ModelName = GUILayout.TextField(_newModelConfig.ModelName, Styles.TextField, GUILayout.Width(110));
            if (_newModelConfig.ModelName.Trim().Length == 0) anyParamEmpty = true;
            GUILayout.EndHorizontal();

            _newModelConfig.Overwrite = CustomGUI.Toggle(_newModelConfig.Overwrite,
                "Overwrite if name exists: ");

            GUILayout.Space(20.0f);

            GUILayout.Label("Required Model Parameters: ");
            for (int i = 0; i < _newModelConfig.Params.Count(); i++)
            {
                _newModelConfig.Params[i] = CustomGUI.ServerFunctionParamField(_newModelConfig.Params[i],
                    new GUILayoutOption[] { GUILayout.Width(50) });

                if (_newModelConfig.Params[i].Value.Trim().Length == 0) anyParamEmpty = true;
            }

            GUILayout.Space(20.0f);
            GUILayout.Label("Recording Filters (Leave category empty to not filter): ");

            _newModelConfig.RequireSuccess = CustomGUI.Toggle(_newModelConfig.RequireSuccess,
                "Require Success:");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name:");
            GUILayout.FlexibleSpace();
            _newModelConfig.PlayerName = GUILayout.TextField(_newModelConfig.PlayerName, Styles.TextField, GUILayout.Width(110));
            GUILayout.EndHorizontal();

            GUILayout.Label($"Use top {_newModelConfig.UsePercentBest * 100:0.}% Recordings:");
            _newModelConfig.UsePercentBest = GUILayout.HorizontalSlider(
                _newModelConfig.UsePercentBest, 0.1f, 1.0f,
                Styles.SliderTrack, Styles.SliderThumb);
            _newModelConfig.UsePercentBest = Mathf.Round(_newModelConfig.UsePercentBest / 0.05f) * 0.05f;

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            bool prevEnabled = GUI.enabled;
            if (anyParamEmpty || (!_newModelRequest?.Finished() ?? false)) GUI.enabled = false;
            if (GUILayout.Button("Create", Styles.Button))
            {
                _state = State.ArtifactCreationResults;
                _newModelRequest = new ServerRequestTask<EmptyPayload, Requests.NewModel>
                    (_service.Gateway, RequestType.new_model, _newModelConfig);
            }
            GUI.enabled = prevEnabled;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawArtifactCreationResults()
        {
            GUILayout.BeginVertical();


            if (!_newModelRequest.Finished())
            {
                GUILayout.Label("Model creation in progress. This may take up to a minute based on number " +
                "of used recordings.");
            }
            else
            {
                GUILayout.Label("Model creation process finished. Result: ");

                _scroll = GUILayout.BeginScrollView(_scroll, Styles.ScrollView, Styles.VerticalScrollbar, GUILayout.ExpandHeight(true));
                GUI.skin.verticalScrollbarThumb = Styles.VerticalScrollbarThumb;
                GUILayout.Label(_newModelRequest.Log);

                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Go Back", Styles.Button))
                {
                    _state = State.ArtifactSetup;
                }
            }
            GUILayout.EndVertical();
        }
    }
}

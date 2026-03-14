using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AIPlugin.PluginGUI
{
    /// <summary>
    /// Responsible for starting session / selecting boss / basic session settings like the number of trials and boss selection
    /// </summary>
    public class ArtifactCreatorWindow : BasePluginWindow//TODO: ENABLE CURSOUR WHEN WINDOW ACTIVE
    {
        private enum CurrentWindow
        {
            Main,
            NewModel,
            TrainModel
        }
        private CurrentWindow currentWindow;
        public override Vector2 Size { get; } = new Vector2(250, 500);
        public override Vector2 Position { get; set; } = new Vector2(0, 0);

        private Vector2 _scroll = new Vector2();

        AiService _service;

        private ConfigFile _configFile;
        private ConfigEntry<KeyboardShortcut> _enableKey;

        private Payloads.GetArchitecturesResponse _serverAIArchitectures = null;
        private Payloads.NewModelRequest _newModelConfig = null;
        private Task _architectureRequestTask = null;
        private Task _newModelTask = null;
        private bool _modelCreationResultsDisplayed = true;
        private string _modelCreationResults = null;

        private CustomGUI.DropdownState _architectureDropdownState = new CustomGUI.DropdownState();
        private CustomGUI.DropdownState _newModelBossDropdownState = new CustomGUI.DropdownState();
        public void Initialize(ConfigFile config, AiService service)
        {
            _service = service;
            _enableKey = config.Bind("Windows", "Show/Hide Artifact Creation Window", new KeyboardShortcut(KeyCode.F3));
            service.OnConnected += OnConnected;
            service.OnDisconnected += OnDisconnected;
        }
        public override bool EnableKeyDown() => _enableKey.Value.IsDown();

        private void OnConnected()
        {
            if(_architectureRequestTask == null)
            _architectureRequestTask = GetArchitecturesAsync();
        }
        private void OnDisconnected()
        {
            _newModelConfig = null;
            _serverAIArchitectures = null;
        }
        public void OnGUI()
        {
            GUILayout.Window(1, new Rect(Position, Size), Draw, "Artifact Creator");
        }

        void Draw(int windowID)
        {
            GUILayout.BeginVertical();

            if (!_service.IsConnected)
            {
                if (GUILayout.Button("Connect to AI Server"))
                {
                    _service.ConnectToServer();
                }
            }
            else if (currentWindow == CurrentWindow.Main)
            {
                if (_serverAIArchitectures == null)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("New Artifact"))
                {
                    currentWindow = CurrentWindow.NewModel;
                }
                GUI.enabled = true;


                if (GUILayout.Button("Train Existing Artifact"))
                {
                    currentWindow = CurrentWindow.TrainModel;
                }
            }
            else if (currentWindow == CurrentWindow.NewModel) DrawModelCreation();
            else if (currentWindow == CurrentWindow.TrainModel) DrawModelTrainingSettings();


            
            GUILayout.EndVertical();
            GUI.enabled = true;
        }
        private void DrawModelCreation()
        {
            if (_modelCreationResultsDisplayed) DrawModelCreationSettings();
            else DrawModelCreationProgress();
        }
        private void DrawModelCreationSettings()
        {
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            GUILayout.Label("The dataset used to train the model using behavioral " +
                "cloning is created based on recordings at the time of creating the " +
                "artifact and cannot be changed later (unless done manually). Make sure all the recordings " +
                "you want to use exist in the respecive directory before creating the model.");

            bool anyParamEmpty = false;
            List<string> architectureNames = _serverAIArchitectures.ArchitectureParams.Keys.ToList();
            GUILayout.Label("Architecture: ");
            var oldIdx = _architectureDropdownState.SelectedIdx;
            _architectureDropdownState =
                CustomGUI.Dropdown(_architectureDropdownState, architectureNames);

            var selectedArchitectureName = architectureNames[_architectureDropdownState.SelectedIdx];

            if (oldIdx != _architectureDropdownState.SelectedIdx || _newModelConfig == null) 
            {
                _newModelConfig = new Payloads.NewModelRequest() 
                { 
                    ArchitectureName = selectedArchitectureName,
                    TargetBossName = "",
                    ModelName = "",
                    Params = _serverAIArchitectures.ArchitectureParams[selectedArchitectureName],
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
            _newModelConfig.ModelName = GUILayout.TextField(_newModelConfig.ModelName, GUILayout.Width(110));
            if (_newModelConfig.ModelName.Trim().Length == 0) anyParamEmpty = true;
            GUILayout.EndHorizontal();

            _newModelConfig.Overwrite = CustomGUI.LabelToggle(_newModelConfig.Overwrite, 
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

            _newModelConfig.RequireSuccess = CustomGUI.LabelToggle(_newModelConfig.RequireSuccess,
                "Require Success:");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name:");
            GUILayout.FlexibleSpace();
            _newModelConfig.PlayerName = GUILayout.TextField(_newModelConfig.PlayerName, GUILayout.Width(110));
            GUILayout.EndHorizontal();
            
            GUILayout.Label($"Use top {_newModelConfig.UsePercentBest * 100:0.}% Recordings:");
            _newModelConfig.UsePercentBest = GUILayout.HorizontalSlider(
                _newModelConfig.UsePercentBest, 0.1f, 1.0f);
            _newModelConfig.UsePercentBest = Mathf.Round(_newModelConfig.UsePercentBest / 0.05f) * 0.05f;

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))  currentWindow = CurrentWindow.Main;
            if (anyParamEmpty || _newModelTask != null) GUI.enabled = false;
            if (GUILayout.Button("Create"))
            {
                _newModelTask = NewModelAsync();
                _modelCreationResultsDisplayed = false;
                _modelCreationResults = null;
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private void DrawModelCreationProgress()
        {
            if (_modelCreationResults == null)
            {
                GUILayout.Label("Model creation in progress. This may take up to a minute based on number " +
                "of used recordings.");
            }
            else
            {
                GUILayout.Label("Model creation process finished. Result: ");
                GUILayout.Label(_modelCreationResults);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Go Back"))
                {
                    _modelCreationResultsDisplayed = true;
                }
            }
        }

        private void DrawModelTrainingSettings()
        {

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Train"))
            {

            }
            if (GUILayout.Button("Cancel")) currentWindow = CurrentWindow.Main;
        }

        private async Task NewModelAsync()
        {

            try
            {
                await _service.Gateway.NewModelAsync(_newModelConfig);
                _modelCreationResults = "ASDASDASD";
            }
            catch (Exception ex)
            {
                _modelCreationResults = $"Exception occured: {ex.Message}";
            }
            finally
            {
                _newModelTask = null;
            }
        }
        private async Task GetArchitecturesAsync()
        {
            try
            {
                _serverAIArchitectures = await _service.Gateway.GetArchitecturesAsync();
            }
            finally
            {
                _architectureRequestTask = null;
            }
        }
    }
}

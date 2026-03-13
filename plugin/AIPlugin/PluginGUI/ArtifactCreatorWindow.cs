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
        public override Vector2 Size { get; } = new Vector2(250, 300);
        public override Vector2 Position { get; set; } = new Vector2(0, 0);

        private Vector2 _scroll = new Vector2();

        AiService _service;

        private ConfigFile _configFile;
        private ConfigEntry<KeyboardShortcut> _enableKey;

        private Payloads.GetArchitecturesResponse _serverAIArchitectures = null;
        private Payloads.NewModelRequest _newModelConfig = null;
        private Task _architectureRequestTask = null;

        private CustomGUI.DropdownState _architectureDropdownState = new CustomGUI.DropdownState();
        private CustomGUI.DropdownState _newModelBossDropdownState = new CustomGUI.DropdownState();
        public void Initialize(ConfigFile config, AiService service)
        {
            _service = service;
            _enableKey = config.Bind("Windows", "Enable Artifact Creation Window", new KeyboardShortcut(KeyCode.F3));
            service.OnConnected += OnConnected;
            service.OnDisconnected += OnDisconnected;
        }
        public override bool EnableKeyDown() => _enableKey.Value.IsDown();
        void OnEnable() 
        { 
            currentWindow = CurrentWindow.Main;
        }

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
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

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
            else if (currentWindow == CurrentWindow.NewModel) DrawModelCreationSettings();
            else if (currentWindow == CurrentWindow.TrainModel) DrawModelTrainingSettings();


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.enabled = true;
        }
        private void DrawModelCreationSettings()
        {

            bool anyParamEmpty = false;
            List<string> architectureNames = _serverAIArchitectures.ArchitectureParams.Keys.ToList();
            GUILayout.Label("Architecture Selection: ");
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
            GUILayout.Label("Target Boss Selection:");
            List<string> bossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
            oldIdx = _newModelBossDropdownState.SelectedIdx;
            _newModelBossDropdownState = CustomGUI.Dropdown(_newModelBossDropdownState, bossNames);
            if (oldIdx != _newModelBossDropdownState.SelectedIdx)
            {
                _newModelConfig.TargetBossName = BossReferenceDatabase.All.Select(s => s.InternalName).
                        ToList()[_newModelBossDropdownState.SelectedIdx];
            }

            GUILayout.Space(10.0f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:");
            GUILayout.FlexibleSpace();
            _newModelConfig.ModelName = GUILayout.TextField(_newModelConfig.ModelName, GUILayout.Width(110));
            if (_newModelConfig.ModelName.Trim().Length == 0) anyParamEmpty = true;
            GUILayout.EndHorizontal();

            _newModelConfig.Overwrite = CustomGUI.LabelToggle(_newModelConfig.Overwrite, 
                "Overwrite if name exists: ");

            GUILayout.Space(10.0f);
            
            GUILayout.Label("Required Model Parameters: ");
            for (int i = 0; i < _newModelConfig.Params.Count(); i++)
            {
                _newModelConfig.Params[i] = CustomGUI.ServerFunctionParamField(_newModelConfig.Params[i],
                    new GUILayoutOption[] { GUILayout.Width(50) });

                if (_newModelConfig.Params[i].Value.Trim().Length == 0) anyParamEmpty = true;
            }

            GUILayout.Space(10.0f);

            GUILayout.FlexibleSpace();
            if (anyParamEmpty) GUI.enabled = false;
            if (GUILayout.Button("Create"))
            {
                //TODO
            }
            GUI.enabled = true;
            if (GUILayout.Button("Cancel"))  currentWindow = CurrentWindow.Main;
        }

        private void DrawModelTrainingSettings()
        {

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Train"))
            {

            }
            if (GUILayout.Button("Cancel")) currentWindow = CurrentWindow.Main;
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

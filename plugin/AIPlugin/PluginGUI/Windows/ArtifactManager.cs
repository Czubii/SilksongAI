using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AIPlugin.Networking.Requests.Requests.GetArtifacts.Response;
using static AIPlugin.PluginGUI.Windows.ArtifactManager;

namespace AIPlugin.PluginGUI.Windows
{
    public class ClientState
    {
        public bool AnySelected => Artifact != null;
        public Artifact Artifact { get; private set; } = null;
        public bool AllowControlFromServer = false;
        public bool ReinforcementLearningActive = false;
        public void UpdateArtifactSelection(Artifact artifact)
        {
            Artifact = artifact;
        }
        public void RemoveArtifactSelection()
        {
            Artifact = null;
        }
    }
    public class ArtifactManager: BaseWindow
    {

        private AiService _service;
        public ClientState Selection { get; private set; }
        private List<(string label, string value)> _bossDropdownElements = new List<(string label, string value)>();
        private CustomGUI.DropdownState<string> _bossDropdownState = new CustomGUI.DropdownState<string>();
        private Dictionary<string, List<Artifact>> _artifacts;
        private Requests.GetArtifactsRefreshable _artifactRequester;
        private enum Contents
        {
            ArtifactSelection,
            ArtifactConfig
        }
        private Contents CurrentContent = Contents.ArtifactSelection;

        public class ConfigForm: IForm
        {
            public float JumpThreshold;
            public float AttackThreshold;
            public float HealThreshold;
            public float SkillThreshold;
            public float DashThreshold;
            public float HarpoonThreshold;
            public ConfigForm(Artifact artifact)
            {
                JumpThreshold = artifact.BooleanThresholds[0];
                AttackThreshold = artifact.BooleanThresholds[1];
                HealThreshold = artifact.BooleanThresholds[2];
                SkillThreshold = artifact.BooleanThresholds[3];
                DashThreshold = artifact.BooleanThresholds[4];
                HarpoonThreshold = artifact.BooleanThresholds[5];
            }
            public bool IsValid()
            {
                return true;
            }
        }

        private ConfigForm _configForm = null;
        private Requests.SetArtifactConfig _setArtifactConfigRequest = null;

        public ArtifactManager(string name, ClientState selection, AiService service): base(name, new Rect(100, 300, 500, 500)) 
        {
            Selection = selection;
            _service = service;
            _artifactRequester = new Requests.GetArtifactsRefreshable(service.Gateway, () => new Requests.GetArtifacts(service.Gateway));

            _service.OnConnected += _artifactRequester.Send;
            _service.Gateway.Events.OnNewArtifactCreated += _artifactRequester.Send;
            _artifactRequester.OnSuccess += UpdateAvailableArtifacts;
            _artifactRequester.OnError += NotifyError;

            _service.OnDisconnected += Selection.RemoveArtifactSelection;
        }
        private void UpdateAvailableArtifacts(Requests.GetArtifacts.Response artifacts)
        {
            _artifacts = artifacts.BossArtifacts;
            BuildBossDropdownElements();
            if (!SelectionStillExists())
            {
                Selection.RemoveArtifactSelection();    
            }
        }
        private bool SelectionStillExists()
        {
            if (!Selection.AnySelected)
                return true;

            if (!_artifacts.TryGetValue(Selection.Artifact.BossName, out var artifactsForBoss))
                return false;

            var selectedArtifact = artifactsForBoss.Find(x => x.Name == Selection.Artifact.Name);

            if (selectedArtifact == null)
                return false;

            Selection.UpdateArtifactSelection(selectedArtifact);

            return true;
        }
        public override bool CanEnable() => _service.IsConnected;
        private void BuildBossDropdownElements()
        {
            if (_artifacts.Count == 0)
            {
                _bossDropdownElements = new List<(string label, string value)>();
                return;
            }

            _bossDropdownElements = new List<(string label, string value)>() { ("Any", "Any") };

            foreach (var pair in _artifacts)
            {
                var internalBossName = pair.Key;

                string displayName =
                    BossReferenceDatabase.All
                        .First(x => x.InternalName == internalBossName)
                        .DisplayName;

                if (displayName == null)
                {
                    AIPlugin.Log.LogError($"Unknown boss: {pair.Key}");
                    continue;
                }

                _bossDropdownElements.Add((displayName, internalBossName));
            }
        }
        public override void DrawContent()
        {
            switch (CurrentContent)
            {
                case Contents.ArtifactSelection:
                    DrawArtifactSelection();
                    break;

                case Contents.ArtifactConfig:
                    DrawArtifactConfig();
                    break;
            }
        }
        private void DrawArtifactConfig()
        {
            var lo = GUILayout.Width(100);
            CustomGUI.ArtifactCard(Selection.Artifact);
            UI.Form(_configForm)
                .BeginCard("Boolean Thresholds: ", Styles.Card)
                .HorizontalSlider("Jump", 0f, 1f, 0.05f, x => x.JumpThreshold, lo)
                .HorizontalSlider("Attack", 0f, 1f, 0.05f, x => x.AttackThreshold, lo)
                .HorizontalSlider("Heal", 0f, 1f, 0.05f, x => x.HealThreshold, lo)
                .HorizontalSlider("Skill", 0f, 1f, 0.05f, x => x.SkillThreshold, lo)
                .HorizontalSlider("Dash", 0f, 1f, 0.05f, x => x.DashThreshold, lo)
                .HorizontalSlider("Harpoon", 0f, 1f, 0.05f, x => x.HarpoonThreshold, lo)
                .EndCard()
                .End();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", Styles.Button))
            {
                CurrentContent = Contents.ArtifactSelection;
            }
            GUI.enabled = _setArtifactConfigRequest?.Finished() ?? true;
            if (GUILayout.Button("Save", Styles.Button))
            {
                var payload = new Requests.SetArtifactConfig.Payload()
                {
                    TargetBossName = Selection.Artifact.BossName,
                    ArtifactName = Selection.Artifact.Name,
                    BooleanThresholds = new List<float>
                    {
                        _configForm.JumpThreshold,
                        _configForm.AttackThreshold,
                        _configForm.HealThreshold,
                        _configForm.SkillThreshold,
                        _configForm.DashThreshold,
                        _configForm.HarpoonThreshold,
                    }
                };

                _setArtifactConfigRequest = new Requests.SetArtifactConfig(_service.Gateway, payload);
                _setArtifactConfigRequest.OnError += NotifyError;
                CurrentContent = Contents.ArtifactSelection;
            }
            GUI.enabled = true;
        }
        private void DrawArtifactSelection()
        {
            if (Selection.AnySelected)
            {
                GUILayout.Space(20);
                DrawArtifactSelectionCard(Selection.Artifact, true);
                GUILayout.Space(20);
            }
            _bossDropdownState = CustomGUI.Dropdown(_bossDropdownState, _bossDropdownElements, "Boss");
            if (_bossDropdownElements.Count == 0)
            {
                GUILayout.Label("There is no existing artifact. You can make one in \"Artifact Creator\" tab");
                return;
            }

            if (_bossDropdownState.SelectedOption == "Any")
            {
                foreach (var pair in _artifacts)
                {
                    var internalBossName = pair.Key;
                    var artifacts = pair.Value;

                    foreach (var artifact in artifacts)
                    {
                        if (Selection.AnySelected &&
                            artifact.BossName == Selection.Artifact.BossName &&
                            artifact.Name == Selection.Artifact.Name) continue;

                        DrawArtifactSelectionCard(artifact, false);
                    }
                }
            }
            else
            {
                var artifacts = _artifacts[_bossDropdownState.SelectedOption];

                foreach (var artifact in artifacts)
                {
                    if (Selection.AnySelected &&
                        artifact.BossName == Selection.Artifact.BossName &&
                        artifact.Name == Selection.Artifact.Name) continue;

                    DrawArtifactSelectionCard(artifact, false);
                }
            }
        }
        private void DrawArtifactSelectionCard(Artifact artifact, bool selected)
        {

            var style = selected ? Styles.CardGreenHighlight : Styles.Card;

            GUILayout.BeginVertical(style);

                GUILayout.BeginHorizontal();
                    GUILayout.Label(artifact.Name, Styles.HeaderLabel);
                    GUILayout.Space(5);
                    GUILayout.Label($"Boss: {artifact.BossName}",
                        new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft });
                    GUILayout.Space(5);
                    GUILayout.Label($"Architecture: {artifact.ArchitectureName}",
                        new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft });
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                    GUILayout.Label($"Created: {artifact.CreationDate}");
                    GUILayout.FlexibleSpace();

                    if (!selected && GUILayout.Button("Select", Styles.Button, GUILayout.Width(150)))
                    {
                        Selection.UpdateArtifactSelection(artifact);
                    }
                    if (selected && GUILayout.Button("Details / Config", Styles.Button, GUILayout.Width(150)))
                    {
                        _configForm = new ConfigForm(artifact);
                        CurrentContent = Contents.ArtifactConfig;
                    }

                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}

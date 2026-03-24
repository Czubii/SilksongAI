using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AIPlugin.PluginGUI
{
    public class ArtifactCreatorWindow : BaseWindow
    {
        private enum Content
        {
            ArtifactSetup,
            ArtifactCreationResults
        }

        private class Form: IForm
        {
            public CustomGUI.DropdownState<string> ArchitectureDropdownState = new CustomGUI.DropdownState<string>();
            public CustomGUI.DropdownState<string> BossDropdownState = new CustomGUI.DropdownState<string>();

            public string ArtifactName = "";
            public bool Overwrite = false;

            public List<ServerParam> ArchitectureParams = new List<ServerParam>();

            //Filters:
            public float PercentBestRecordings = 0.8f;
            public bool RequireSuccess = false;
            public string PlayerName = "";

            public Requests.NewArtifact.Payload GetRequestPayload()
            {
                Requests.NewArtifact.Payload requestPayload = new Requests.NewArtifact.Payload()
                {
                    ArchitectureName = ArchitectureDropdownState.SelectedOption,
                    TargetBossName = BossDropdownState.SelectedOption,

                    ArtifactName = ArtifactName,
                    Overwrite = Overwrite,

                    Params = ArchitectureParams,

                    UsePercentBest = PercentBestRecordings,
                    RequireSuccess = RequireSuccess,
                    PlayerName = PlayerName
                };

                return requestPayload;
            }
            public bool IsValid()
            {
                return ArtifactName != "";
            }
        }

        private Form _form = new Form();

        private Content _currentContent = Content.ArtifactSetup;

        private Vector2 _scroll = new Vector2();

        AiService _service;

        private readonly Requests.GetArchitecturesRefreshable _architectureRequester;
        private Requests.NewArtifact _newModelRequest = null;

        private readonly List<(string, string)> _bossesDropdownElements;

        public ArtifactCreatorWindow(string name, AiService service):
            base(name, new Rect(100, 300, 250, 500))
        {
            _service = service;

            _architectureRequester = new Requests.GetArchitecturesRefreshable(service.Gateway, () => new Requests.GetArchitectures(service.Gateway));

            service.OnConnected += _architectureRequester.Send;
            service.OnConnected += OnConnected;

            var displayBossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
            var internalBossNames = BossReferenceDatabase.All.Select(s => s.InternalName).ToList();

            _bossesDropdownElements = displayBossNames.Zip(internalBossNames, (d, i) => (d, i)).ToList();

        }
        private void OnConnected()
        {
            _form = new Form();
        }
        public override bool CanEnable() => _service.IsConnected && _architectureRequester.AnyResponse();
        public override void DrawContent()
        {
            switch (_currentContent)
            {
                case Content.ArtifactSetup:
                    DrawArtifactSetup();
                    break;
                case Content.ArtifactCreationResults:
                    DrawArtifactCreationResults();
                    break;
            }
        }
        private void DrawArtifactSetup()
        {
            var architectures = _architectureRequester.Result;
            var architectureNames = architectures.ArchitectureParams.Keys.ToList();

            List<(string label, string value)> architecturesDropdownElements = architectureNames.Zip(architectureNames, (d, i) => (d, i)).ToList();

            UI.Form(_form)
                .Label("The dataset used to train the model using behavioral " +
                "cloning is created based on recordings at the time of creating the " +
                "artifact and cannot be changed later (unless done manually). Make sure all the recordings " +
                "you want to use exist in the respecive directory before creating the model.")

                .Dropdown("Architecture: ", architecturesDropdownElements, x => x.ArchitectureDropdownState, 
                    () => InitializeArchitectureParams(architectures))

                .Dropdown("Boss: ", _bossesDropdownElements, x => x.BossDropdownState)
                .TextField("Name: ", x => x.ArtifactName, GUILayout.Width(120))
                .Toggle("Overwrite if name exists: ", x => x.Overwrite)

                .Space(20)
                .Label("Recording Filters:")
                .Toggle("Require Successs: ", x => x.RequireSuccess)
                .TextField("Player Name: ", x => x.PlayerName, GUILayout.Width(120))
                .HorizontalSlider($"Use {_form.PercentBestRecordings * 100.0f: 0.}% Best Rocrdings", 0.1f, 1.0f, 0.05f, x => x.PercentBestRecordings)

                .Space(20)
                .ParamListField("Required architecture parameters: ", x => x.ArchitectureParams, GUILayout.Width(120))

                .Space(20)
                .GUIEnabled(CanCreateArtifact())
                .Button("Create", () => CreateArtifact(architectures))
                .End();

        }
        private void InitializeArchitectureParams(Requests.GetArchitectures.Response architectures)
        {
            List<ServerParam> paramList = architectures.ArchitectureParams.Values.ToList()[_form.ArchitectureDropdownState.SelectedIdx];
            _form.ArchitectureParams = paramList;
        }
        private bool CanCreateArtifact()
        {
            return _form.IsValid() && (_newModelRequest?.Finished() ?? true);
        }
        private void CreateArtifact(Requests.GetArchitectures.Response architectures)
        {
            var payload = _form.GetRequestPayload();

            _newModelRequest = new Requests.NewArtifact(_service.Gateway, payload);
            _currentContent = Content.ArtifactCreationResults;
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
                    _currentContent = Content.ArtifactSetup;
                }
            }
            GUILayout.EndVertical();
        }
    }
}

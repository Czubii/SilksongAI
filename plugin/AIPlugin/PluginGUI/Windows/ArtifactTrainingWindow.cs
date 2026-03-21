using AIPlugin.Networking;
using AIPlugin.Utilities;
using HutongGames.PlayMaker.Actions;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AIPlugin.Networking.Responses;

namespace AIPlugin.PluginGUI
{
    public class ArtifactTrainingWindow : BaseWindow
    {
        class ArtifactOption
        {
            public string BossName;
            public string ArtifactName;

            public ArtifactOption(string boss, string artifact)
            {
                BossName = boss;
                ArtifactName = artifact;
            }
        }
        private class Form : IForm
        {
            public Vector2 Scroll = new Vector2();
            public CustomGUI.DropdownState<string> BossDropdownState = new CustomGUI.DropdownState<string>();
            public CustomGUI.DropdownState<ArtifactOption> ArtifactDropdownState = new CustomGUI.DropdownState<ArtifactOption>();
            public int NumEpochs = 1;
            public int BatchSize = 4;
            public float LearningRateExponent = -3.0f;
            public float LearningRate => Mathf.Pow(10, LearningRateExponent);
            public bool UseGPU = false;

            public bool IsValid()
            {
                return ArtifactDropdownState.SelectedOption != null 
                    && NumEpochs > 0 
                    && BatchSize > 0;
            }
        }

        private enum Content
        {
            TrainingForm,
            TrainingInProgress
        }

        private Content _currentContent = Content.TrainingForm;

        private Form _form = new Form();

        private AiService _service;

        private ServerRequest.Refreshable<Artifacts, EmptyPayload> _artifactRequester;
        private ServerRequest.TrainArtifact _artifactTrainingRequest = null;

        private readonly List<(string label, string value)> _bossDropdownElements;
        private List<(string label, ArtifactOption)> _artifactDropdownElements = new List<(string label, ArtifactOption)>();

        public ArtifactTrainingWindow(string name, AiService service) :
            base(name, new Rect(100, 300, 500, 500))
        {
            _service = service;

            _artifactRequester = ServerRequest.Refreshable.Watch(service.Gateway,
                () => new ServerRequest.GetArtifacts(service.Gateway));

            service.Gateway.Events.OnNewArtifactCreated += _artifactRequester.Send;
            service.OnConnected += _artifactRequester.Send;
            service.OnConnected += OnConnected;

            _artifactRequester.OnNewResultReady += BuildArtifactDropdownElements;

            var displayBossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
            displayBossNames.Insert(0, "Any");

            var internalBossNames = BossReferenceDatabase.All.Select(s => s.InternalName).ToList();
            internalBossNames.Insert(0, "");

            _bossDropdownElements = displayBossNames.Zip(internalBossNames, (a, b) => (a, b)).ToList();
        }
        private void OnConnected()
        {
            _form = new Form();
        }
        private void BuildArtifactDropdownElements()
        {
            var artifacts = _artifactRequester.Result.BossArtifacts;

            if (_form.BossDropdownState.SelectedOption == "") //Any boss
            {
                _artifactDropdownElements = new List<(string label, ArtifactOption)>();
                foreach (var pair in artifacts)
                {
                    var internalBossName = pair.Key;
                    var artifactNames = pair.Value;

                    foreach (var artifactName in artifactNames) 
                    {
                        _artifactDropdownElements.Add(($"{internalBossName}:    {artifactName}", new ArtifactOption(internalBossName, artifactName)));
                    }
                }
            }
            else
            {
                var bossName = _form.BossDropdownState.SelectedOption;
                var anyArtifacts = artifacts.TryGetValue(bossName, out var artifactNames);

                if (anyArtifacts && artifactNames.Count > 0)
                {
                    _artifactDropdownElements = artifactNames.Select(a => (a, new ArtifactOption(bossName, a))).ToList();
                }
                else
                {
                    _artifactDropdownElements = new List<(string label, ArtifactOption)>(); // Empty
                }
            }
        }
        public override bool CanEnable() => _service.IsConnected && _artifactRequester.AnyResponse();
        private bool CanStartTraining() => _form.IsValid() && (_artifactTrainingRequest?.Finished() ?? true);
        public override void DrawContent()
        {
            switch (_currentContent)
            {
                case Content.TrainingForm:
                    DrawTrainingForm();
                    break;
                case Content.TrainingInProgress:
                    DrawTrainingProgress();
                    break;
            }
        }
        private void DrawTrainingForm()
        {
            UI.Form(_form)
               .BeginScrollView(x => x.Scroll)
               .Dropdown("Boss: ", _bossDropdownElements, x => x.BossDropdownState, BuildArtifactDropdownElements)
               .Dropdown("Artifact: ", _artifactDropdownElements, x => x.ArtifactDropdownState)
               .IntegerField("Number of Epochs: ", x => x.NumEpochs, GUILayout.Width(120))
               .IntegerField("Batch Size: ", x => x.BatchSize, GUILayout.Width(120))
               .Toggle("Use GPU: ", x => x.UseGPU)
               .HorizontalSlider($"Learning Rate: {_form.LearningRate.ToString("G2")}", -10f, 0f, 0.1f, x => x.LearningRateExponent)
               .EndScrollView()
               .FlexibleSpace()
               .GUIEnabled(CanStartTraining())
               .Button("Start Training", StartTraining)
               .End();
        }

        private void DrawTrainingProgress()
        {
            GUILayout.BeginVertical();
            if(_artifactTrainingRequest == null)
            {
                _currentContent = Content.TrainingForm;
                return;
            }

            if (!_artifactTrainingRequest.Finished())
            {
                GUILayout.Label("Training Starting. Please wait");
                return;
            }

            if (!_artifactTrainingRequest.FinishedWithSuccess())
            {
                GUILayout.Label("Could not start training because an exception occured somwhere: \n");
                GUILayout.Label(_artifactTrainingRequest.Log);

                GUILayout.FlexibleSpace();

                if(GUILayout.Button("Go Back", Styles.Button))
                {
                    _currentContent = Content.TrainingForm;
                }
                return;
            }

            GUILayout.EndVertical();
        }
        private void StartTraining()
        {
            var payload = new Requests.TrainArtifcat()
            {
                TargetBossName = _form.ArtifactDropdownState.SelectedOption.BossName,
                ArtifactName = _form.ArtifactDropdownState.SelectedOption.ArtifactName,
                NumEpochs = _form.NumEpochs,
                BatchSize = _form.BatchSize,
                LearningRate = _form.LearningRate,
                UseGPU = _form.UseGPU,
            };

            _artifactTrainingRequest = new ServerRequest.TrainArtifact(_service.Gateway, payload);
            _currentContent = Content.TrainingInProgress;
        }
        //public override void DrawContent()
        //{
        //    if (_trainConfig == null) _trainConfig = new Requests.TrainArtifcat();

        //    bool anyEmpty = false;

        //    GUILayout.BeginVertical();


        //    GUILayout.Label("Boss Selection: ");
        //    _bossDropdownState = CustomGUI.Dropdown(_bossDropdownState, _bossDropdownItems);

        //    if(_bossDropdownState.SelectionChanged)
        //    {
        //        _artifactDropdownState = new CustomGUI.DropdownState();
        //        _trainConfig.TargetBossName = BossReferenceDatabase.All.Select(s => s.InternalName).
        //                ToList()[_bossDropdownState.SelectedIdx];
        //    }


        //    GUILayout.Label("Artifact Selection: ");
        //    List<string> artifactDropdownItems;
        //    List<string> artifactNames;
        //    if (_bossDropdownState.SelectedIdx == 0) //Any 
        //    { 
        //        artifactDropdownItems = new List<string>();
        //        artifactNames = new List<string>();
        //        foreach (var internalBossName in _artifactRequester.Result.BossArtifacts.Keys)
        //        {
        //            foreach (var artifactName in _artifactRequester.Result.BossArtifacts[internalBossName])
        //            {
        //                artifactDropdownItems.Add($"{internalBossName}:    {artifactName}"); //TODO somehow use display name here???
        //                artifactNames.Add(artifactName);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var internalBossName = BossReferenceDatabase.All.Select(s => s.InternalName).ToList()[_bossDropdownState.SelectedIdx - 1];
        //        var any_artifacts = _artifactRequester.Result.BossArtifacts.TryGetValue(internalBossName, out artifactDropdownItems);
        //        artifactNames = artifactDropdownItems;
        //        if (!any_artifacts)
        //        {
        //            artifactDropdownItems = new List<string>(); // Empty
        //            anyEmpty = true;
        //        }
        //    }

        //    _artifactDropdownState = CustomGUI.Dropdown(_artifactDropdownState, artifactDropdownItems);

        //    if (_artifactDropdownState.SelectionChanged)
        //    {
        //        _trainConfig.ArtifactName = artifactNames[_artifactDropdownState.SelectedIdx];
        //    }

        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Number of epochs: ");
        //    GUILayout.FlexibleSpace();
        //    _trainConfig.NumEpochs = CustomGUI.IntegerField(_trainConfig.NumEpochs, 
        //        new GUILayoutOption[] { GUILayout.Width(110) });
        //    if(_trainConfig.NumEpochs == 0) anyEmpty = true;
        //    GUILayout.EndHorizontal();

        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Batch size: ");
        //    GUILayout.FlexibleSpace();
        //    _trainConfig.BatchSize = CustomGUI.IntegerField(_trainConfig.BatchSize,
        //        new GUILayoutOption[] { GUILayout.Width(110) });
        //    if (_trainConfig.BatchSize == 0) anyEmpty = true;
        //    GUILayout.EndHorizontal();

        //    _trainConfig.UseGPU = CustomGUI.LabeledToggle(_trainConfig.UseGPU, "Use GPU: ");

        //    GUILayout.Label("TODO: learning rate");

        //    GUILayout.FlexibleSpace();

        //    bool prevEnabled = GUI.enabled;
        //    if (anyEmpty || (!_artifactTrainingRequest?.Finished() ?? false)) GUI.enabled = false;
        //    if (GUILayout.Button("Create", Styles.Button))
        //    {
        //        _artifactTrainingRequest = new ServerRequest.TrainArtifact(_service.Gateway, _trainConfig);
        //    }
        //    GUI.enabled = prevEnabled;

        //    GUILayout.EndVertical();
        //}
    }
}

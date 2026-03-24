using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using AIPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AIPlugin.Networking.EventPayloads;

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
            AwaitingTraining,
            TrainingInProgress,
            TrainingReuslts,
            FinalizationResults
        }

        private Content _currentContent = Content.TrainingForm;

        private Form _form = new Form();

        private AiService _service;

        private Requests.GetArtifactsRefreshable _artifactRequester;
        private Requests.StartBehavioralCloning _startTrainingRequest = null;
        private Requests.StopBehavioralCloning _stopTrainingRequest = null;
        private Requests.FinalizeBehavioralCloning _finalizeTrainingRequest = null;

        private readonly List<(string label, string value)> _bossDropdownElements;
        private List<(string label, ArtifactOption)> _artifactDropdownElements = new List<(string label, ArtifactOption)>();

        private TrainingEpoch _trainingInfo = null;
        private LinePlot _lossPlot;
        
        public ArtifactTrainingWindow(string name, AiService service) :
            base(name, new Rect(100, 300, 500, 500))
        {
            _service = service;

            _artifactRequester = new Requests.GetArtifactsRefreshable(service.Gateway, () => new Requests.GetArtifacts(service.Gateway));
            _lossPlot = new LinePlot(450, 200, "Loss Of Epoch:");

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
            _currentContent = Content.TrainingForm;
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
        private bool CanStartTraining() => _form.IsValid() && (_startTrainingRequest?.Finished() ?? true);
        public override void DrawContent()
        {
            GUILayout.BeginVertical();

            try
            {
                switch (_currentContent)
                {
                    case Content.TrainingForm:
                        DrawTrainingForm();
                        break;
                    case Content.AwaitingTraining:
                        DrawAwaitingTraining();
                        break;
                    case Content.TrainingInProgress:
                        DrawTrainingProgress();
                        break;
                    case Content.TrainingReuslts:
                        DrawTrainingResults();
                        break;
                    case Content.FinalizationResults:
                        DrawFinalizeTrainingResults();
                        break;
                }
            }
            finally
            {
                GUILayout.EndVertical();
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
            if (_trainingInfo == null)
            {
                GUILayout.Label($"Awaiting Data: ");
                GUILayout.Label($"Epoch: ?/?");
                GUILayout.Label($"Training Dataset loss: ?");
                GUILayout.Label($"Testing Dataset loss: ?");

                _lossPlot.Draw();

                CustomGUI.ProgressBar(0.0f, $"0%");
            }
            else
            {
                GUILayout.Label($"Training: ");
                GUILayout.Label($"Epoch: {_trainingInfo.CurrentEpoch}/{_trainingInfo.EndEpoch}");
                GUILayout.Label($"Training Dataset loss: {_trainingInfo.TrainingLoss}");
                GUILayout.Label($"Testing Dataset loss: {_trainingInfo.TestingLoss}");

                _lossPlot.Draw();

                int epochRange = _trainingInfo.EndEpoch - _trainingInfo.StartEpoch;
                float percentage = 0.0f;
                if (epochRange > 0) percentage = (float)_trainingInfo.CurrentEpoch / (float)epochRange;

                CustomGUI.ProgressBar(percentage, $"{percentage * 100: 0.}%");
            }

            if(_stopTrainingRequest != null ) GUI.enabled = false;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Stop Training", Styles.Button)) _stopTrainingRequest = new Requests.StopBehavioralCloning(_service.Gateway);
            GUI.enabled = true;
        }

        private void DrawAwaitingTraining()
        {

            if (_startTrainingRequest == null)
            {
                _currentContent = Content.TrainingForm;
                return;
            }

            if (!_startTrainingRequest.Finished())
            {
                GUILayout.Label("Training Starting. Please wait");
                return;
            }

            if (!_startTrainingRequest.FinishedWithSuccess())
            {
                GUILayout.Label("Could not start training because an exception occured somwhere: \n");
                GUILayout.Label(_startTrainingRequest.Log);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Go Back", Styles.Button))
                {
                    _currentContent = Content.TrainingForm;
                    _service.Gateway.Events.OnTrainingEpoch -= OnTrainingEpoch;
                    _service.Gateway.Events.OnTrainingFinished -= OnTrainingFinished;
                }
                return;
            }

            _currentContent = Content.TrainingInProgress;
        }

        private void DrawTrainingResults()
        {
            if (_trainingInfo == null)
            {
                GUILayout.Label($"Training Concluded. Results: ");
                GUILayout.Label($"Total Epochs: ?");
                GUILayout.Label($"Training Dataset loss: ?");
                GUILayout.Label($"Testing Dataset loss: ?");
            }
            else
            {
                GUILayout.Label($"Training Concluded. Results: ");
                GUILayout.Label($"Total Epochs: {_trainingInfo.CurrentEpoch}");
                GUILayout.Label($"Training Dataset loss: {_trainingInfo.TrainingLoss}");
                GUILayout.Label($"Testing Dataset loss: {_trainingInfo.TestingLoss}");
            }
            _lossPlot.Draw();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Discard Changes", Styles.Button)) FinalizeTraining(false);
            if(GUILayout.Button("Save Changes", Styles.Button)) FinalizeTraining(true);

            GUILayout.EndHorizontal();
        }

        private void DrawFinalizeTrainingResults()
        {
            if (!_finalizeTrainingRequest.Finished())
            {
                GUILayout.Label("Finalization in progress. Please Wait");
                return;
            }
            GUILayout.Label("Training Finalized. Result:");
            GUILayout.Label($"{_finalizeTrainingRequest.Log}");

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Go Back", Styles.Button)) _currentContent = Content.TrainingForm;
        }

        private void FinalizeTraining(bool save)
        {
            var payload = new Requests.FinalizeBehavioralCloning.Payload()
            {
                Save = save
            };
            _finalizeTrainingRequest = new Requests.FinalizeBehavioralCloning(_service.Gateway, payload);
            _currentContent = Content.FinalizationResults;
        }
        private void OnTrainingEpoch(TrainingEpoch info)
        {
            _trainingInfo = info;


            List<float> trainingLosses = info.LossHistory.Select(x => x.training).ToList();
            List<float> testingLosses = info.LossHistory.Select(x => x.testing).ToList();

            MainThreadDispatcher.Enqueue(() =>
            {
                try
                {
                    _lossPlot.SetSeries(0, trainingLosses, Styles.UIGreen, "Training Loss");
                    _lossPlot.SetSeries(1, testingLosses, Styles.UIOrange, "Testing Loss");
                    _lossPlot.Update();
                }
                catch (Exception ex)
                {
                    ThreadSafeLogService.Log($"Exception met when updating plot: {ex}", AIPlugin.Log.LogError);
                }
            });
        }
        private void OnTrainingFinished()
        {
            _currentContent = Content.TrainingReuslts;
            _service.Gateway.Events.OnTrainingFinished -= OnTrainingFinished;
            _service.Gateway.Events.OnTrainingEpoch -= OnTrainingEpoch;
        }
        private void StartTraining()
        {
            var payload = new Requests.StartBehavioralCloning.Payload()
            {
                TargetBossName = _form.ArtifactDropdownState.SelectedOption.BossName,
                ArtifactName = _form.ArtifactDropdownState.SelectedOption.ArtifactName,
                NumEpochs = _form.NumEpochs,
                BatchSize = _form.BatchSize,
                LearningRate = _form.LearningRate,
                UseGPU = _form.UseGPU,
            };

            _startTrainingRequest = new Requests.StartBehavioralCloning(_service.Gateway, payload);
            _stopTrainingRequest = null;
            _trainingInfo = null;
            _lossPlot.Clear();
            _currentContent = Content.AwaitingTraining;
            _service.Gateway.Events.OnTrainingEpoch += OnTrainingEpoch;
            _service.Gateway.Events.OnTrainingFinished += OnTrainingFinished;
        }
    }
}

using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using AIPlugin.PluginGUI.Windows;
using AIPlugin.Utilities;
using BepInEx;
using HutongGames.PlayMaker.Actions;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AIPlugin.Networking.EventPayloads;
using static AIPlugin.PluginGUI.CustomGUI;

namespace AIPlugin.PluginGUI
{
    public class BehavioralCloningWindow : BaseWindow
    {
        private class Form : IForm
        {
            public Vector2 Scroll = new Vector2();
            public int NumEpochs = 1;
            public int BatchSize = 4;
            public float LearningRateExponent = -3.0f;
            public float LearningRate => Mathf.Pow(10, LearningRateExponent);
            public bool UseGPU = false;

            public bool IsValid()
            {
                return NumEpochs > 0 && BatchSize > 0;
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

        private Requests.StartBehavioralCloning _startTrainingRequest = null;
        private Requests.StopBehavioralCloning _stopTrainingRequest = null;
        private Requests.FinalizeBehavioralCloning _finalizeTrainingRequest = null;

        private ArtifactSelection _artifactSelection;
        private Requests.GetArtifacts.Response.Artifact _traindedArtifact;

        private BCEpoch _trainingInfo = null;
        private LinePlot _lossPlot;
        
        public BehavioralCloningWindow(string name, ArtifactSelection artifactSelection, AiService service) :
            base(name, new Rect(100, 300, 500, 550))
        {
            _service = service;
            _artifactSelection = artifactSelection;

            _lossPlot = new LinePlot(450, 200, "Loss Of Epoch:");

            service.OnConnected += OnConnected;

        }
        private void OnConnected()
        {
            _form = new Form();
            _currentContent = Content.TrainingForm;
        }
        private void HandleError(string error)
        {
            _currentContent = Content.TrainingForm;
            FinalizeTraining(false);
            NotifyError(error);
        }
        public override bool CanEnable() => _service.IsConnected;
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
               .CustomAction(() => {
                    ArtifactSelectionCard(_artifactSelection);
               })
               .GUIEnabled(_artifactSelection.AnySelected)

               .BeginCard("Training Settings: ", Styles.Card)
               .IntegerField("Number of Epochs: ", x => x.NumEpochs, GUILayout.Width(120))
               .IntegerField("Batch Size: ", x => x.BatchSize, GUILayout.Width(120))
               .Toggle("Use GPU: ", x => x.UseGPU)
               .HorizontalSlider($"Learning Rate: {_form.LearningRate.ToString("G2")}", -10f, 0f, 0.1f, x => x.LearningRateExponent)
               .EndCard()

               .FlexibleSpace()
               .GUIEnabled(CanStartTraining())
               .Button("Start Training", StartTraining)
               .End();
        }

        private void DrawTrainingProgress()
        {
            ArtifactCard(_traindedArtifact);

            GUILayout.BeginVertical(Styles.Card);
            GUILayout.Label($"Training: ", Styles.HeaderLabel);
            if (_trainingInfo == null)
            {
                GUILayout.Label($"Epoch: ?/?");
                GUILayout.Label($"Training Dataset loss: ?");
                GUILayout.Label($"Testing Dataset loss: ?");

                _lossPlot.Draw();

                CustomGUI.ProgressBar(0.0f, $"0%");
            }
            else
            {
                GUILayout.Label($"Epoch: {_trainingInfo.CurrentEpoch}/{_trainingInfo.EndEpoch}");
                GUILayout.Label($"Training Dataset loss: {_trainingInfo.TrainingLoss}");
                GUILayout.Label($"Testing Dataset loss: {_trainingInfo.TestingLoss}");

                _lossPlot.Draw();

                int epochRange = _trainingInfo.EndEpoch - _trainingInfo.StartEpoch;
                float percentage = 0.0f;
                if (epochRange > 0) 
                    percentage = (float)(_trainingInfo.CurrentEpoch - _trainingInfo.StartEpoch) 
                        / (float)epochRange;

                CustomGUI.ProgressBar(percentage, $"{percentage * 100: 0.0}%");
            }

            GUILayout.EndVertical();

            if(_stopTrainingRequest != null ) GUI.enabled = false;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Stop Training", Styles.Button))
            {
                _stopTrainingRequest = new Requests.StopBehavioralCloning(_service.Gateway);
                _startTrainingRequest.OnError += HandleError;
            }
            GUI.enabled = true;
        }

        private void DrawAwaitingTraining()
        {
            ArtifactCard(_traindedArtifact);
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
                    _service.Gateway.Events.OnBCEpoch -= OnTrainingEpoch;
                    _service.Gateway.Events.OnTrainingFinished -= OnTrainingFinished;
                }
                return;
            }

            _currentContent = Content.TrainingInProgress;
        }

        private void DrawTrainingResults()
        {
            ArtifactCard(_traindedArtifact);

            GUILayout.BeginVertical(Styles.Card);
            GUILayout.Label($"Training Results: ", Styles.HeaderLabel);

            if (_trainingInfo == null)
            {
                GUILayout.Label($"Total Epochs: ?");
                GUILayout.Label($"Training Dataset loss: ?");
                GUILayout.Label($"Testing Dataset loss: ?");
            }
            else
            {
                GUILayout.Label($"Total Epochs: {_trainingInfo.CurrentEpoch}");
                GUILayout.Label($"Training Dataset loss: {_trainingInfo.TrainingLoss}");
                GUILayout.Label($"Testing Dataset loss: {_trainingInfo.TestingLoss}");
            }

            _lossPlot.Draw();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Discard Changes", Styles.Button)) FinalizeTraining(false);
            if(GUILayout.Button("Save Changes", Styles.Button)) FinalizeTraining(true);

            GUILayout.EndHorizontal();
        }

        private void DrawFinalizeTrainingResults()
        {
            ArtifactCard(_traindedArtifact);
            if (!_finalizeTrainingRequest.Finished())
            {
                GUILayout.Label("Finalization in progress. Please Wait");
                return;
            }
            _currentContent = Content.TrainingForm;
        }

        private void FinalizeTraining(bool save)
        {
            var payload = new Requests.FinalizeBehavioralCloning.Payload()
            {
                Save = save
            };
            _finalizeTrainingRequest = new Requests.FinalizeBehavioralCloning(_service.Gateway, payload);
            _finalizeTrainingRequest.OnError += HandleError;
            _currentContent = Content.FinalizationResults;
        }
        private void OnTrainingEpoch(BCEpoch info)
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
            _service.Gateway.Events.OnBCEpoch -= OnTrainingEpoch;
        }
        private void StartTraining()
        {
            _traindedArtifact = _artifactSelection.Artifact;
            var payload = new Requests.StartBehavioralCloning.Payload()
            {
                TargetBossName = _artifactSelection.Artifact.BossName,
                ArtifactName = _artifactSelection.Artifact.Name,
                NumEpochs = _form.NumEpochs,
                BatchSize = _form.BatchSize,
                LearningRate = _form.LearningRate,
                UseGPU = _form.UseGPU,
            };

            _startTrainingRequest = new Requests.StartBehavioralCloning(_service.Gateway, payload);
            _startTrainingRequest.OnError += HandleError;

            _stopTrainingRequest = null;
            _trainingInfo = null;
            _lossPlot.Clear();
            _currentContent = Content.AwaitingTraining;
            _service.Gateway.Events.OnBCEpoch += OnTrainingEpoch;
            _service.Gateway.Events.OnTrainingFinished += OnTrainingFinished;
        }
    }
}

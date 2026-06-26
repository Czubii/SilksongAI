using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AIPlugin.Networking.EventPayloads;

namespace AIPlugin.PluginGUI.Windows
{
    public class ReinforcementLearningWindow : BaseWindow
    {
        private class StartTrainingForm: IForm
        {
            public int NumEpochs = 1;
            public int NumFightsPerEpoch = 5;

            public bool IsValid() => NumEpochs > 0 && NumFightsPerEpoch > 0;
        }

        private StartTrainingForm _form = new StartTrainingForm();
        private enum Contents
        {
            StartTrainingForm,
            TrainingInProgress
        }

        private Contents _currentContent = Contents.StartTrainingForm;

        private AiService _service;
        private ArtifactSelection _artifactSelection;

        private Requests.RLStart _startRLRequest;

        public ReinforcementLearningWindow(
            string name, 
            ArtifactSelection artifactSelection,
            AiService service):
            base(name, new Rect(100, 300, 500, 550))
        {
            _service = service;
            _artifactSelection = artifactSelection;
        }
        public override bool CanEnable() => _service.IsConnected;
        public override void DrawContent()
        {
            switch (_currentContent)
            {
                case Contents.StartTrainingForm:
                    DrawStartTrainingForm();
                    break;
            }
        }
        private void DrawStartTrainingForm()
        {

            UI.Form(_form)
                .CustomAction(() => {
                    CustomGUI.ArtifactSelectionCard(_artifactSelection);
                })
                .GUIEnabled(_artifactSelection.AnySelected)

                .BeginCard("Training Settings", Styles.Card)
                .IntegerField("Number of Epochs: ", x => x.NumEpochs)
                .IntegerField("Number of Fights Per Epoch", x => x.NumFightsPerEpoch)
                .EndCard()
                .FlexibleSpace()
                .Button("Start", () =>
                {
                    var payload = new Requests.RLStart.Payload()
                    {
                        ArtifactName = _artifactSelection.Artifact.Name,
                        TargetBossName = _artifactSelection.Artifact.BossName,
                        NumEpochs = _form.NumEpochs,
                        FightsPerEpoch = _form.NumFightsPerEpoch,
                    };

                    _startRLRequest = new Requests.RLStart(_service.Gateway, payload);

                    _startRLRequest.OnError += NotifyError;
                    _startRLRequest.OnSuccess += (_) =>
                    {
                        _currentContent = Contents.TrainingInProgress;
                    };

                }, _form.IsValid() && (_startRLRequest?.Finished() ?? true))
                .End();
        }
    }
}

using AIPlugin.Networking;
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
        private AiService _service;

        private ServerRequest.Refreshable<Artifacts, EmptyPayload> _artifactRequester;
        private ServerRequest.TrainArtifact _artifactTrainingRequest = null;

        private List<string> _bossDropdownItems;

        private CustomGUI.DropdownState _bossDropdownState = new CustomGUI.DropdownState();
        private CustomGUI.DropdownState _artifactDropdownState = new CustomGUI.DropdownState();

        private Requests.TrainArtifcat _trainConfig = null;

        public ArtifactTrainingWindow(string name, AiService service) :
            base(name, new Rect(100, 300, 500, 500))
        {
            _service = service;
            _bossDropdownItems = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
            _bossDropdownItems.Insert(0, "Any");

            _artifactRequester = ServerRequest.Refreshable.Watch(service.Gateway,
                () => new ServerRequest.GetArtifacts(service.Gateway));

            service.Gateway.Events.OnNewArtifactCreated += _artifactRequester.Send;
            service.OnConnected += _artifactRequester.Send;
        }

        public override bool CanEnable() => _service.IsConnected;
        public override void DrawContent()
        {
            if (!_artifactRequester.AnyResponse()) return;
            if (_trainConfig == null) _trainConfig = new Requests.TrainArtifcat();

            bool anyEmpty = false;

            GUILayout.BeginVertical();


            GUILayout.Label("Boss Selection: ");
            _bossDropdownState = CustomGUI.Dropdown(_bossDropdownState, _bossDropdownItems);

            if(_bossDropdownState.SelectionChanged)
            {
                _artifactDropdownState = new CustomGUI.DropdownState();
                _trainConfig.TargetBossName = BossReferenceDatabase.All.Select(s => s.InternalName).
                        ToList()[_bossDropdownState.SelectedIdx];
            }

            
            GUILayout.Label("Artifact Selection: ");
            List<string> artifactDropdownItems;
            List<string> artifactNames;
            if (_bossDropdownState.SelectedIdx == 0) //Any 
            { 
                artifactDropdownItems = new List<string>();
                artifactNames = new List<string>();
                foreach (var internalBossName in _artifactRequester.Result.BossArtifacts.Keys)
                {
                    foreach (var artifactName in _artifactRequester.Result.BossArtifacts[internalBossName])
                    {
                        artifactDropdownItems.Add($"{internalBossName}:    {artifactName}"); //TODO somehow use display name here???
                        artifactNames.Add(artifactName);
                    }
                }
            }
            else
            {
                var internalBossName = BossReferenceDatabase.All.Select(s => s.InternalName).ToList()[_bossDropdownState.SelectedIdx - 1];
                var any_artifacts = _artifactRequester.Result.BossArtifacts.TryGetValue(internalBossName, out artifactDropdownItems);
                artifactNames = artifactDropdownItems;
                if (!any_artifacts)
                {
                    artifactDropdownItems = new List<string>(); // Empty
                    anyEmpty = true;
                }
            }

            _artifactDropdownState = CustomGUI.Dropdown(_artifactDropdownState, artifactDropdownItems);

            if (_artifactDropdownState.SelectionChanged)
            {
                _trainConfig.ArtifactName = artifactNames[_artifactDropdownState.SelectedIdx];
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Number of epochs: ");
            GUILayout.FlexibleSpace();
            _trainConfig.NumEpochs = CustomGUI.IntegerField(_trainConfig.NumEpochs, 
                new GUILayoutOption[] { GUILayout.Width(110) });
            if(_trainConfig.NumEpochs == 0) anyEmpty = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Batch size: ");
            GUILayout.FlexibleSpace();
            _trainConfig.BatchSize = CustomGUI.IntegerField(_trainConfig.BatchSize,
                new GUILayoutOption[] { GUILayout.Width(110) });
            if (_trainConfig.BatchSize == 0) anyEmpty = true;
            GUILayout.EndHorizontal();

            _trainConfig.UseGPU = CustomGUI.LabeledToggle(_trainConfig.UseGPU, "Use GPU: ");

            GUILayout.Label("TODO: learning rate");

            GUILayout.FlexibleSpace();

            bool prevEnabled = GUI.enabled;
            if (anyEmpty || (!_artifactTrainingRequest?.Finished() ?? false)) GUI.enabled = false;
            if (GUILayout.Button("Create", Styles.Button))
            {
                _artifactTrainingRequest = new ServerRequest.TrainArtifact(_service.Gateway, _trainConfig);
            }
            GUI.enabled = prevEnabled;

            GUILayout.EndVertical();
        }
    }
}

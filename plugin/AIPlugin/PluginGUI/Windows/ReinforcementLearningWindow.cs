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
        private AiService _service;
        private SessionDispatcher _sessionDispatcher;
        private ClientState _clientState;
        public ReinforcementLearningWindow(
            string name, 
            ClientState clientState,
            SessionDispatcher sessionDispatcher, 
            AiService service):
            base(name, new Rect(100, 300, 500, 550))
        {
            _service = service;
            _clientState = clientState;
        }
        public override bool CanEnable() => _service.IsConnected;
        public override void DrawContent()
        {
            throw new NotImplementedException();
        }
    }
}

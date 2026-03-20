using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.PluginGUI
{
    public class ServerControlsWindow : BaseWindow//TODO: ENABLE CURSOUR WHEN WINDOW ACTIVE
    {
       

        private AiService _service;

        public ServerControlsWindow(string name, AiService service) : base(name, new Rect(0,0, 200, 0))
        {
            _service = service;
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();
            if(!_service.IsConnected && GUILayout.Button("Connect To Server", Styles.Button))
                _service.ConnectToServer();
            else if(_service.IsConnected && GUILayout.Button("Disconnect From Server", Styles.GreenButton))
            {
                //TODO
            }


            GUILayout.EndVertical();
            GUI.enabled = true;
        }

    }
}

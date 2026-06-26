using AIPlugin.BossfightSession;
using AIPlugin.Networking;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static GamepadVibrationMixer.GamepadVibrationEmission;
using static HutongGames.PlayMaker.Actions.BlockEvents;

namespace AIPlugin.PluginGUI
{
    public class ServerControlsWindow : BaseWindow//TODO: ENABLE CURSOUR WHEN WINDOW ACTIVE
    {
        private AiService _service;
        private bool _useDefaultAdress = true;

        private class IPForm: IForm
        {
            public string _ip = "127.0.0.1";
            public int _port = 5000;
            public bool IsValid()
            {
                return !_ip.IsNullOrWhiteSpace();
            }
        }
        private IPForm _IPForm = new IPForm();

        public ServerControlsWindow(string name, AiService service) : base(name, new Rect(0,0, 300, 0))
        {
            _service = service;
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();

            if (!_service.IsConnected)
            {
                var _useDefaultAdressNew = !CustomGUI.LabeledToggle(!_useDefaultAdress, "Use Non-Default IP: ");

                if (_useDefaultAdressNew && !_useDefaultAdress)
                {
                    _IPForm = new IPForm();

                }
                _useDefaultAdress = _useDefaultAdressNew;

                UI.Form(_IPForm)
                    .GUIEnabled(!_useDefaultAdress)
                    .TextField("IP: ", x => x._ip, GUILayout.Width(120))
                    .IntegerField("Port: ", x => x._port, GUILayout.Width(120))
                    .End();


                GUI.enabled = !_service.ConnectingInProgress;
                if (!_service.IsConnected && GUILayout.Button("Connect To Server", Styles.Button))
                {

                    GUI.enabled = _useDefaultAdress || (!_useDefaultAdress && _IPForm.IsValid());

                    if (_useDefaultAdress)
                    {
                        _service.Connect("127.0.0.1", 5000);
                    }
                    else
                    {
                        _service.Connect(_IPForm._ip, _IPForm._port);
                    }


                    GUI.enabled = true;

                }
            }

            else if (_service.IsConnected && GUILayout.Button("Disconnect From Server", Styles.GreenButton))
            {
                //TODO
            }

            GUILayout.EndVertical();
            GUI.enabled = true;
        }

    }
}

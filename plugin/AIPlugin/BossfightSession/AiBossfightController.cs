using AIPlugin.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AIPlugin.BossfightSession.SessionManager;

namespace AIPlugin.BossfightSession
{
    /// <summary>
    /// Colects the frame data, sends reqest to ai server, and forwards the ai controls further to be applied
    /// </summary>
    public class AiBossfightController : MonoBehaviour, ISessionListener
    {
        private AiService _service;
        public void Initialize(AiService service)
        {
            _service = service;
            enabled = false;
            service.OnDisconnected += OnDisconnected;
        }
        public void OnDisconnected()
        {
            enabled = false;
        }
        public void OnEnable()
        {
            if (!_service?.IsConnected ?? true)
            {
                enabled = false;
                return;
            }
        }
        public void OnSessionStarted(SessionInfo info)
        {

        }
        public void OnSessionStopped(bool forced)
        {

        }
        public void OnFightStarted()
        {

        }
        public void OnFightFinished(FightResults results)
        {

        }


    }
}

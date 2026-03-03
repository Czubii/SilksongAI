//using AIPlugin.Networking;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using static AIPlugin.BossfightSession.BossfightRecorder;
//using static AIPlugin.BossfightSession.SessionManager;

//namespace AIPlugin.BossfightSession
//{
//    /// <summary>
//    /// Colects the frame data, sends reqest to ai server, and forwards the ai controls further to be applied
//    /// </summary>
//    public class AiBossfightController : MonoBehaviour, ISessionListener
//    {
//        public enum AiState
//        {
//            Idle,
//            Fighting
//        }

//        public AiState State;

//        private AiService _service;
//        private int _framesToNextRecord = 0;
//        public void Initialize(AiService service)
//        {
//            _service = service;
//            enabled = false;
//            service.OnDisconnected += OnDisconnected;
//        }
//        public void OnDisconnected()
//        {
//            enabled = false;
//        }
//        public void OnEnable()
//        {
//            if (!_service?.IsConnected ?? true)
//            {
//                enabled = false;
//                return;
//            }
//        }
//        public void OnFightStarted(SessionEnemyManager enemyManager)
//        {
//            _framesToNextRecord = 0;
//        }
//        public void OnFightFinished(FightResults results, bool forced)
//        {

//        }
//        public void Update()
//        {
//            if (State != AiState.Fighting || (GameManager.instance?.IsGamePaused() ?? true)) return;

//            _framesToNextRecord--;

//            if (_framesToNextRecord <= 0)
//            {
//                RecordFrame();
//                _framesToNextRecord = SessionConfig.RecordFrameDelta;
//            }
//        }
//        private void RecordFrame()
//        {

//        }

//    }
//}

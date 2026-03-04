using AIPlugin.Networking;
using UnityEngine;
using static AIPlugin.BossfightSession.BossfightRecorder;


namespace AIPlugin.BossfightSession
{
    /// <summary>
    /// Colects the frame data, sends reqest to ai server, and forwards the ai controls further to be applied
    /// </summary>
    public class AiBossfightController : MonoBehaviour, ISessionListener
    {
        public enum AiState
        {
            Idle,
            Fighting
        }

        public AiState State;

        private AiService _service;
        private SessionEnemyManager _enemyManager;
        private int _framesToNextPost = 0;

        public void Initialize(SessionEnemyManager enemyManager, AiService service)
        {
            _enemyManager = enemyManager;
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
        public void OnFightStarted()
        {
            if (State != AiState.Idle || !enabled) return;
            _framesToNextPost = 0;
        }
        public void OnFightFinished(AttemptResult result)
        {

        }
        public void Update()
        {
            if (State != AiState.Fighting || (GameManager.instance?.IsGamePaused() ?? true)) return;

            _framesToNextPost--;

            if (_framesToNextPost <= 0)
            {
                PostFrame();
                _framesToNextPost = SessionConfig.RecordFrameDelta;
            }
        }
        private void PostFrame()
        {

        }

    }
}

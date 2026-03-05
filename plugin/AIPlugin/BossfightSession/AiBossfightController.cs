using AIPlugin.Networking;
using AIPlugin.Utilities;
using InControl.UnityDeviceProfiles;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
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
        private int _framesToNextRequest = 0;
        private Task _aiControllTask = null;

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

            State = AiState.Fighting;
            _framesToNextRequest = 0;
        }
        public void OnFightFinished(AttemptResult result)
        {
            State = AiState.Idle;
        }
        public void Update()
        {
            if (State != AiState.Fighting || (GameManager.instance?.IsGamePaused() ?? true)) return;

            _framesToNextRequest--;
            if (_framesToNextRequest <= 0)
            {
                if (_aiControllTask == null)
                {
                    _aiControllTask = ApplyAIControll();
                    _framesToNextRequest = SessionConfig.RecordFrameDelta;
                }
                else
                {
                    AIPlugin.Log.LogWarning($"AiBossfightController: Applying AI controll " +
                        $"took longer than SessionConfig.RecordFrameDelta + {-_framesToNextRequest} frames");
                }
            }
        }
        private async Task ApplyAIControll()
        {
            try
            {
                var inputs = await RequestInputs();
                if (inputs == null) return;

                ThreadSafeLogService.Log(string.Join(", ", inputs), AIPlugin.Log.LogMessage);
            }
            catch (Exception e)
            {
                ThreadSafeLogService.Log($"Exception while ApplyAIControll: {e.ToString()}");
            }
            finally { _aiControllTask = null; }
        }
        private async Task<FrameUserInputs> RequestInputs()
        {
            EnemyInstance boss = _enemyManager?.GetTargetInstance() ?? null;
            List<EnemyInstance> enemies = _enemyManager?.GetNonTargetInstances() ?? null;

            LivePredictionFrameData frameData = FrameDataCollector.GetLive(boss, enemies);

            if (frameData == null)
            {
                AIPlugin.Log.LogWarning("AiBossfightController: No frame data. Skipping Frame");
                return null;
            }

            return await _service.Gateway.PredictInputsAsync(frameData);
        }

    }
}

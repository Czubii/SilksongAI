using MessagePack;
using System.Collections.Generic;
using System;
using UnityEngine;
using static AIPlugin.BossfightSession.BossfightRecorder;

namespace AIPlugin.BossfightSession
{
    public class FrameCapturer: MonoBehaviour, ISessionListener
    {
        private SessionEvents _events;

        private int _framesToNextCapture = 0;

        private bool _capturingActive = false;

        private SessionEnemyTracker _enemyTracker;

        public void Initialize(SessionEvents sessionEventHandler, SessionEnemyTracker enemyTracker)
        {
            _enemyTracker = enemyTracker; 
            _events = sessionEventHandler;
        }
        public void OnFightStarted()
        {
            _capturingActive = true;
        }
        public void OnFightFinished(AttemptResult result)
        {
            _capturingActive = false;
        }

        private void Update()
        {
            if (!_capturingActive || (GameManager.instance?.IsGamePaused() ?? true)) return;

            _framesToNextCapture--;

            if (_framesToNextCapture <= 0)
            {
                //TODO:  check if any listener exists and is enabled
                CaptureFrame();
                _framesToNextCapture = SessionConfig.CaptureFrameDelta;
            }
        }

        private void CaptureFrame()
        {
            EnemyInstance boss = _enemyTracker?.GetTargetInstance() ?? null;
            List<EnemyInstance> enemies = _enemyTracker?.GetNonTargetInstances() ?? null;

            if (boss == null || enemies == null)
            {
                AIPlugin.Log.LogWarning("CaptureFrame: Couldn't get enemies. Frame will be skipped");
                return;
            }
            try
            {
                RecordingFrameData frameData = FrameDataCollector.GetRecording(boss, enemies);
                NotifyFrameCaptured(frameData);
            }
            catch (Exception e)
            {
                AIPlugin.Log.LogError(e);
                AIPlugin.Log.LogError("Exception meth when trying to capture frame. Frame will be skipped");
            }
        }

        private void NotifyFrameCaptured(RecordingFrameData frameData) => _events.RaiseFrameCaptured(frameData);
    }
}

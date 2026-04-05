using AIPlugin.Utilities;
using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static AIPlugin.BossfightSession.BossfightRecorder;

namespace AIPlugin.BossfightSession
{
    public class FrameCapturer: MonoBehaviour, ISessionListener
    {
        private SessionEvents _events;

        private int _framesToNextCapture = 0;

        private bool _capturingActive = false;

        private SessionEnemyTracker _enemyTracker;

        private RecordingFrame _prevFrame = null;
        public void Initialize(SessionEvents sessionEventHandler, SessionEnemyTracker enemyTracker)
        {
            _enemyTracker = enemyTracker; 
            _events = sessionEventHandler;
        }
        public void OnFightStarted()
        {
            _capturingActive = true;
            _prevFrame = null;
            _events.RaiseCaptureStarted();
        }
        public void OnFightFinished(AttemptResult result)
        {
            _capturingActive = false;
            CaptureFrame(result);
            _events.RaiseFinished(result);
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

        private void CaptureFrame(AttemptResult? result = null)
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
                RecordingFrame frame = FrameDataCollector.GetRecording(boss, enemies);

                if (_prevFrame != null) 
                {
                    frame.Reward = RewardCalculator.Calculate(_prevFrame, frame, result);
                }

                _events.RaiseFrameCaptured(frame);
                _prevFrame = frame;
            }
            catch (Exception e)
            {
                AIPlugin.Log.LogError(e);
                AIPlugin.Log.LogError("Exception meth when trying to capture frame. Frame will be skipped");
            }
        }

    }
}

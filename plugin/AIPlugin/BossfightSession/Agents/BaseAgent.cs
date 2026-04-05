using AIPlugin.Networking;
using AIPlugin.Networking.Requests;
using HarmonyLib;
using InControl;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;

namespace AIPlugin.BossfightSession.Agents
{
    public abstract class BaseAgent: MonoBehaviour, IFrameCaptureListener
    {
        private class InferenceDelay
        {
            private readonly Stopwatch _timer = new Stopwatch();
            private long _sampleSum = 0;
            private int _nSamples = 0;
            public void StartMeasure()
            {
                _timer.Restart();
            }
            public void StopMeasure()
            {
                if (!_timer.IsRunning)
                    return;

                _timer.Stop();
                _sampleSum += _timer.ElapsedMilliseconds;
                _nSamples++;
            }
            public double AverageMS()
            {
                if (_nSamples == 0)
                    return 0;

                return (double)_sampleSum / _nSamples;
            }
            public void Reset()
            {
                _sampleSum = 0;
                _nSamples = 0;
            }
        }

        protected AiService service;
        private Func<bool> _actionRequestFinished;
        private InferenceDelay _inferenceDelay;

        public virtual void Initialize(AiService service)
        {
            this.service = service;
            enabled = false;
            _inferenceDelay = new InferenceDelay();
        }
        public virtual bool CanEnable()
        {
            return service?.IsConnected ?? false;
        }
        public virtual void OnEnable()
        {
            if (!CanEnable())
            {
                enabled = false;
                return;
            }
            service.OnDisconnected += OnDisconnected;
        }
        public virtual void OnDisable()
        {
            service.OnDisconnected -= OnDisconnected;
            AIInputState.AIControlEnabled = false;
        }
        public void OnDisconnected()
        {
            enabled = false;
        }
        public void OnCaptureStarted()
        {
            _inferenceDelay.Reset();
            if (enabled)
            {
                AIInputState.AIControlEnabled = true;
            }
        }
        public void OnCaptureFinished(AttemptResult result)
        {
            AIInputState.AIControlEnabled = false;
        }
        public void OnFrameCaptured(RecordingFrame frame)
        {
            if (!enabled) return;

            if (_actionRequestFinished?.Invoke() ?? true)
            {
                _inferenceDelay.StartMeasure();
                void OnRequestSuccess(FrameUserInputs inputs)
                {
                    _inferenceDelay.StopMeasure();
                    ApplyAIControll(inputs);
                }
                CreateActionRequest(frame, OnRequestSuccess, out _actionRequestFinished);
            }
            else
            {
                AIPlugin.Log.LogWarning($"AiBossfightController: Obtaining server AI response took longer than expected");
            }
        }
        private void ApplyAIControll(FrameUserInputs inputs)
        {
            AIInputState.Inputs = inputs;
        }
        protected abstract void CreateActionRequest(RecordingFrame frame, Action<FrameUserInputs> successCallback, out Func<bool> requestFinished);
    }
}

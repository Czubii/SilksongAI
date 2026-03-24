using AIPlugin.Networking;
using AIPlugin.Utilities;
using HarmonyLib;
using InControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using UnityEngine;


namespace AIPlugin.BossfightSession
{
    /// <summary>
    /// Colects the frame data, sends reqest to ai server, and forwards the ai controls further to be applied
    /// </summary>
    public class AIBossfightAgent : MonoBehaviour, ISessionListener, IFrameCaptureListener
    {
        private AiService _service;
        private Task _aiControllTask = null;

        public void Initialize(AiService service)
        {
            _service = service;
            enabled = false;
        }
        public bool CanEnable()
        {
            return _service?.IsConnected ?? false;
        }
        public void OnEnable()
        {
            if (!_service?.IsConnected ?? true)
            {
                enabled = false;
                return;
            }
            _service.OnDisconnected += OnDisconnected;
        }
        public void OnDisable()
        {
            _service.OnDisconnected -= OnDisconnected;
            AIInputState.AIControlEnabled = false;
        }
        public void OnDisconnected()
        {
            enabled = false;
        }
        public void OnFightStarted()
        {
            if (enabled)
            {
                AIInputState.AIControlEnabled = true;
            }
        }
        public void OnFightFinished(AttemptResult result)
        {
            //TODO: add server handshake to verify boss selection etc and proceed only if successful
            AIInputState.AIControlEnabled = false;
        }
        public void OnFrameCaptured(RecordingFrame frame)
        {
            if(!enabled) return;

            if (_aiControllTask == null)
            {
                _aiControllTask = ApplyAIControll(frame);
            }
            else
            {
                AIPlugin.Log.LogWarning($"AiBossfightController: Obtaining server AI response took longer than expected");
            }
        }
        private async Task ApplyAIControll(RecordingFrame frame)
        {
            try
            {
                //var inputs = await _service.Gateway.PredictInputsAsync(InferenceFrame.FromRecordingFrameData(frame));
                //if (inputs == null) return;

                //AIInputState.Inputs = inputs; //TODO make this thread safe???
            }
            catch (Exception e)
            {
                ThreadSafeLogService.Log($"Exception while GetAIPrediction: {e.ToString()}", AIPlugin.Log.LogError);
            }
            finally { _aiControllTask = null; }
        }
    }

    public static class AIInputState
    {
        public static bool AIControlEnabled = false;
        public static FrameUserInputs Inputs = new FrameUserInputs();
    }

    [HarmonyPatch(typeof(HeroController), "LookForInput")]
    class HeroController_LookForInput_Patch
    {
        static bool Prefix(HeroController __instance, InputHandler ___inputHandler)
        {
            try
            {
                if (AIInputState.AIControlEnabled)
                {
                    ulong currentTick = InputManager.CurrentTick;
                    float deltaTime = Time.deltaTime;

                    MethodInfo method = typeof(PlayerTwoAxisAction).GetMethod(
                        "UpdateWithAxes", BindingFlags.Instance | BindingFlags.NonPublic);

                    if (method == null)
                        AIPlugin.Log.LogError("UpdateWithAxes not found");

                    method.Invoke(___inputHandler.inputActions.MoveVector, new object[]
                    {   AIInputState.Inputs.Horizontal,
                        AIInputState.Inputs.Vertical,
                        currentTick,
                        deltaTime });

                    if (AIInputState.Inputs.Vertical > 0)
                    {
                        ___inputHandler.inputActions.Up.CommitWithValue(AIInputState.Inputs.Vertical, currentTick, deltaTime);
                        ___inputHandler.inputActions.Down.CommitWithValue(0.0f, currentTick, deltaTime);
                    }
                    else
                    {
                        ___inputHandler.inputActions.Up.CommitWithValue(0.0f, currentTick, deltaTime);
                        ___inputHandler.inputActions.Down.CommitWithValue(-AIInputState.Inputs.Vertical, currentTick, deltaTime);
                    }
                    ___inputHandler.inputActions.Jump.CommitWithState(AIInputState.Inputs.Jump, currentTick, deltaTime);
                    ___inputHandler.inputActions.Dash.CommitWithState(AIInputState.Inputs.Dash, currentTick, deltaTime);
                    ___inputHandler.inputActions.Attack.CommitWithState(AIInputState.Inputs.Attack, currentTick, deltaTime);
                    ___inputHandler.inputActions.Cast.CommitWithState(AIInputState.Inputs.Heal, currentTick, deltaTime);
                    ___inputHandler.inputActions.QuickCast.CommitWithState(AIInputState.Inputs.Skill, currentTick, deltaTime);
                    ___inputHandler.inputActions.SuperDash.CommitWithState(AIInputState.Inputs.Harpoon, currentTick, deltaTime);
                }
            }
            catch (Exception ex)
            {
                AIPlugin.Log.LogError(ex);
            }

            return true;
        }
    }
}

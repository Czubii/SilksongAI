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
        public void OnDisable()
        {
            AIInputState.AIControlEnabled = false;
            State = AiState.Idle;
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
            AIInputState.AIControlEnabled = true;
            _framesToNextRequest = 0;
        }
        public void OnFightFinished(AttemptResult result)
        {
            AIInputState.AIControlEnabled = false;
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

                AIInputState.Inputs = inputs;
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

            if (boss == null || enemies == null)
            {
                ThreadSafeLogService.Log("AIBossfightController: Couldn't get enemies. Skipping Frame", AIPlugin.Log.LogWarning);
                return null;
            }

            LivePredictionFrameData frameData;

            try
            {
                frameData = FrameDataCollector.GetLive(boss, enemies);
            }
            catch (Exception e)
            {
                ThreadSafeLogService.Log($"AIBossfightController: Exception {e}. Skipping Frame", AIPlugin.Log.LogError);
                return null;
            }

            return await _service.Gateway.PredictInputsAsync(frameData);
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
                    {   AIInputState.Inputs.horizontal,
                        AIInputState.Inputs.vertical,
                        currentTick,
                        deltaTime });

                    if (AIInputState.Inputs.vertical > 0)
                    {
                        ___inputHandler.inputActions.Up.CommitWithValue(AIInputState.Inputs.vertical, currentTick, deltaTime);
                        ___inputHandler.inputActions.Down.CommitWithValue(0.0f, currentTick, deltaTime);
                    }
                    else
                    {
                        ___inputHandler.inputActions.Up.CommitWithValue(0.0f, currentTick, deltaTime);
                        ___inputHandler.inputActions.Down.CommitWithValue(-AIInputState.Inputs.vertical, currentTick, deltaTime);
                    }
                    ___inputHandler.inputActions.Jump.CommitWithState(AIInputState.Inputs.jump, currentTick, deltaTime);
                    ___inputHandler.inputActions.Dash.CommitWithState(AIInputState.Inputs.dash, currentTick, deltaTime);
                    ___inputHandler.inputActions.Attack.CommitWithState(AIInputState.Inputs.attack, currentTick, deltaTime);
                    ___inputHandler.inputActions.Cast.CommitWithState(AIInputState.Inputs.heal, currentTick, deltaTime);
                    ___inputHandler.inputActions.QuickCast.CommitWithState(AIInputState.Inputs.skill, currentTick, deltaTime);
                    ___inputHandler.inputActions.SuperDash.CommitWithState(AIInputState.Inputs.harpoon, currentTick, deltaTime);
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

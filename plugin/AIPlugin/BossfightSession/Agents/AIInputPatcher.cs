using HarmonyLib;
using InControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.BossfightSession.Agents
{
    public static class AIInputState
    {
        public static bool AIControlEnabled = false;
        public static FrameUserInputs Inputs = new FrameUserInputs();
    }

    [HarmonyPatch(typeof(HeroController), "LookForInput")]
    class AIInputPatcher
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SilksongAI
{
    public static class inputTracker
    {
        public static SilksongAI.Controls? GetInputs()
        {
            var IH = InputHandler.Instance;
            if (IH == null) return null;

            Controls controls = new Controls()
            {

                jump = IH.inputActions.Jump,
                left = IH.inputActions.Left.RawValue,
                right = IH.inputActions.Right.RawValue,
                up = IH.inputActions.Up.RawValue,
                down = IH.inputActions.Down.RawValue,

                attack = IH.inputActions.Attack,
                heal = IH.inputActions.Cast,
                skill = IH.inputActions.QuickCast,
                dash = IH.inputActions.Dash,
                harpoon = IH.inputActions.SuperDash

            };

            return controls;
        }
    }
}

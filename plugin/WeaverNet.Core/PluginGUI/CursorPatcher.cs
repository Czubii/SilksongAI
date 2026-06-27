using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.PluginGUI
{
    public static class CursorPatcher
    {
        private static bool _forceFisible = false;
        public static bool ForceFisible
        {
            get => _forceFisible; 
            set
            {
                _forceFisible=value;
                Cursor.visible=value;
                if(value) Cursor.lockState = CursorLockMode.None;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor))]
        [HarmonyPatch("visible", MethodType.Setter)]
        private static void CursorVisiblePrefix(ref bool value)
        {
            if (_forceFisible) value = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor))]
        [HarmonyPatch("lockState", MethodType.Setter)]
        private static void CursorLockStatePrefix(ref CursorLockMode value)
        {
            if (_forceFisible) value = CursorLockMode.None;
        }
    }
}

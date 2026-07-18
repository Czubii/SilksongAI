using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public static partial class PluginGUI
    {
        private static GUILayoutOption[] MergeOptions(GUILayoutOption defaultOption, GUILayoutOption[] options)
        {
            if (options == null || options.Length == 0)
                return new[] { defaultOption };

            GUILayoutOption[] result = new GUILayoutOption[options.Length + 1];

            result[0] = defaultOption;
            options.CopyTo(result, 1);

            return result;
        }
    }
}

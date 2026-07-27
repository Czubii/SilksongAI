using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static T Lazy<T>(ref T field, Func<T> create) where T : class
        {
            if (field == null) field = create();
            return field;
        }

    }
}

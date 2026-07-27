using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Mod.Game;
using WeaverNet.Mod.Game.Debug;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;
using WeaverNet.Diagnostics.Extensions;
using WeaverNet.Core.Game;
using WeaverNet.Mod.Game.Player;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    public class DebugWindow : __BaseWindow
    {
        private HitboxVisualizer HitboxVisualizer { get; }
        private ILoadoutManager LoadoutManager { get; }
        private bool _hitboxesEnabled = false;
        private Loadout _loadout = null;
        public DebugWindow(string name, HitboxVisualizer hitbox) : base(name, new Rect(0, 0, 200, 150))
        {
            HitboxVisualizer = hitbox;
            LoadoutManager = new LoadoutManager();//TODO remove 
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();
            _hitboxesEnabled = Elements.PluginGUI.Toggle(_hitboxesEnabled);
            if(_hitboxesEnabled != HitboxVisualizer.enabled)
            {
                HitboxVisualizer.enabled = _hitboxesEnabled;
            }

            GUILayout.Label("Loadouts");
            if (GUILayout.Button("Print Current Loadout", WeaverNetStyles.Button))
            {
                LoadoutManager.BuildLoadout("Debug Print Loadout").Print();
            }
            if (GUILayout.Button("Store Current Loadout", WeaverNetStyles.Button))
            {
                _loadout = LoadoutManager.BuildLoadout("Debug Store Loadout");
            }
            GUI.enabled = _loadout != null;
            if (GUILayout.Button("Apply Stored Loadout", WeaverNetStyles.Button))
            {
                LoadoutManager.SetLoadout(_loadout);
            }

            GUI.enabled = true;
            GUILayout.EndVertical();

        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Mod.PluginGUI.Elements;
using WeaverNet.Mod.PluginGUI.Styles;

namespace WeaverNet.Mod.PluginGUI.Windows
{
    public class UtilitiesWindow : BaseWindow
    {
        private readonly ITeleportService _teleportService;
        private readonly IBossDatabase _bossDatabase;


        private DropdownState<string> _bossDropdownState = new DropdownState<string>();
        List<(string, string)> bossDropdownElements;
        public UtilitiesWindow(string name, ITeleportService teleportService, IBossDatabase bossDatabase) : base(name, new Rect(0, 0, 200, 150))
        {
            _teleportService = teleportService;
            _bossDatabase = bossDatabase;
            bossDropdownElements = _bossDatabase.All.Select((a) => (a.Boss.DisplayName, a.Boss.ID)).ToList();
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();

            PluginGUIElements.Dropdown(_bossDropdownState, bossDropdownElements, "Boss");

            GUI.enabled = _teleportService.CanTeleport();
            if (GUILayout.Button("Teleport", PluginGUIStyles.Button))
            {
                var bossPreset = _bossDatabase.All.First(a => a.Boss.ID == _bossDropdownState.SelectedOption);

                _teleportService.Teleport(bossPreset.Boss.ArenaSceneName, bossPreset.Boss.ArenaPosition, true);
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
            GUI.enabled = true;
        }
    }
}

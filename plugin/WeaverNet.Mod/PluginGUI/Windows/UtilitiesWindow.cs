using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
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
            bossDropdownElements = _bossDatabase.All.Select((a) => (a.DisplayName, a.ID)).ToList();
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();

            PluginGUIElements.Dropdown(_bossDropdownState, bossDropdownElements, "Boss");

            GUI.enabled = _teleportService.CanTeleport();
            if (GUILayout.Button("Teleport", PluginGUIStyles.Button))
            {
                var bossMetadata = _bossDatabase.All.Find(a => a.ID == _bossDropdownState.SelectedOption);
                bossMetadata.Behavior.Respawn();
                _teleportService.Teleport(bossMetadata, true);
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
            GUI.enabled = true;
        }

    }
}

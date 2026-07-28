//using BepInEx;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using WeaverNet.Core.Game;
//using WeaverNet.Core.Game.Interfaces;
//using WeaverNet.Mod.WeaverGUI.Elements;
//using WeaverNet.Mod.WeaverGUI.Styles;

//namespace WeaverNet.Mod.WeaverGUI.Windows
//{
//    public class UtilitiesWindow : __BaseWindow
//    {
//        private readonly ITeleportService _teleportService;
//        private readonly IBossRepository _bossRepository;

//        private readonly DropdownState<BossData> _bossDropdownState = new DropdownState<BossData>();
//        private readonly IReadOnlyList<BossData> _bossDropdownElements;

//        public UtilitiesWindow(
//            string name,
//            ITeleportService teleportService,
//            IBossRepository bossRepository)
//            : base(name, new Rect(0, 0, 200, 150))
//        {
//            _teleportService = teleportService;
//            _bossRepository = bossRepository;

//            _bossDropdownElements = _bossRepository.All.ToList();
//        }

//        public override bool CanEnable() => true;

//        public override void DrawContent()
//        {
//            GUILayout.BeginVertical();

//            //PluginGUI.Dropdown(_bossDropdownState, _bossDropdownElements, a=>a.DisplayName);

//            GUI.enabled = _teleportService.CanTeleport();

//            if (GUILayout.Button("Teleport", WeaverNetStyles.Button))
//            {
//                var bossData = _bossDropdownState.SelectedOption;

//                _teleportService.Teleport(
//                    bossData.ArenaSceneName,
//                    bossData.ArenaPosition,
//                    true);
//            }

//            GUI.enabled = true;

//            GUILayout.EndVertical();
//        }
//    }
//}

using System.Collections.ObjectModel;
using UnityEngine;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Popups;
using WeaverNet.Mod.WeaverGUI.Styles;
using WeaverNet.Mod.WeaverGUI.Views;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    internal class BossfightSessionWindow: MultiViewWindow
    {

        public BossfightSessionWindow(
            string name,

            StandardBossfightSetupView standardBossfightSetupView)
            : base(name, new Rect(0, 0, 350, 500))
        {
            AddView("Main", DrawMain, "Session Type Selection");
            AddView("StandardBossfightSetup", standardBossfightSetupView, "Player Fight Session Configuration");
        }
        public override bool CanEnable() => true;
        private void DrawMain()
        {

            if(PluginGUI.Button("Player Fight Session"))
            {
                SwitchView("StandardBossfightSetup");
            }
        }
    }
}

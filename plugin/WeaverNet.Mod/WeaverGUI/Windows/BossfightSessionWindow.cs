using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Views;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    internal class BossfightSessionWindow: MultiViewWindow
    {
        public BossfightSessionWindow(
            string name,
            StandardBossfightSetupView standardBossfightSetupView)
            : base(name, new Rect(0, 0, 350, 400))
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

using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class PlayerUpgradeSetDebugExtensions
    {
        public static string ToDebugString(this PlayerUpgradeSet upgrades)
        {
            return string.Format(
                "Upgrades:\n" +
                "  HP:            {0}\n" +
                "  Silk:          {1}\n" +
                "  SilkHearts:    {2}\n" +
                "  CraftingKits:  {3}\n" +
                "  ToolPouches:   {4}\n" +
                "  NailUpgrades:   {5}",
                upgrades.HP,
                upgrades.Silk,
                upgrades.SilkHearts,
                upgrades.CraftingKits,
                upgrades.ToolPouches,
                upgrades.NailUpgrades);
        }

        [Conditional("DEBUG")]
        public static void Print(this PlayerUpgradeSet upgrades)
        {
            PluginLog.Info(upgrades.ToDebugString());
        }
    }
}
using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class LoadoutDebugExtensions
    {
        public static string ToDebugString(this Loadout loadout)
        {
            return string.Format(
                "Loadout:\n{0}\n{1}\n{2}",
                loadout.Abilities.ToDebugString(),
                loadout.Tools.ToDebugString(),
                loadout.Upgrades.ToDebugString());
        }

        [Conditional("DEBUG")]
        public static void Print(this Loadout loadout)
        {
            PluginLog.Info(loadout.ToDebugString());
        }
    }
}
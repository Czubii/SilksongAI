using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class BossPresetDebugExtensions
    {
        public static string ToDebugString(this BossPreset preset)
        {
            return string.Format(
                "BossPreset:\n{0}\n{1}",
                preset.Boss.ToDebugString(),
                preset.Loadout.ToDebugString());
        }

        [Conditional("DEBUG")]
        public static void Print(this BossPreset preset)
        {
            PluginLog.Info(preset.ToDebugString());
        }
    }
}
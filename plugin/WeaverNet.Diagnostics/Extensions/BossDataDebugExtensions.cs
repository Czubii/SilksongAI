using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class BossDataDebugExtensions
    {
        public static string ToDebugString(this BossData boss)
        {
            return string.Format(
                "BossData:\n" +
                "  ID:                    {0}\n" +
                "  DisplayName:           {1}\n" +
                "  ArenaSceneName:        {2}\n" +
                "  ArenaPosition:         {3}\n" +
                "  CanRespawnOnArena:     {4}\n" +
                "  RequireHardSceneReload:{5}\n" +
                "{6}",
                boss.Id,
                boss.DisplayName,
                boss.ArenaSceneName,
                boss.ArenaPosition,
                boss.CanRespawnOnArena,
                boss.RequireHardSceneReload,
                boss.RespawnFlags.ToDebugString());
        }

        [Conditional("DEBUG")]
        public static void Print(this BossData boss)
        {
            PluginLog.Info(boss.ToDebugString());
        }
    }
}
using System.Diagnostics;
using System.Text;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class BossRespawnFlagsDebugExtensions
    {
        public static string ToDebugString(this BossRespawnFlags flags)
        {
            var sb = new StringBuilder();

            sb.AppendLine("  RespawnFlags:");

            sb.AppendLine("    PlayerDataBools:");
            foreach (var pair in flags.PlayerDataBools)
            {
                sb.AppendLine(
                    string.Format(
                        "      {0}: {1}",
                        pair.Key,
                        pair.Value));
            }

            sb.AppendLine("    SceneBools:");
            foreach (var sceneBool in flags.SceneBools)
            {
                sb.AppendLine(sceneBool.ToDebugString());
            }

            sb.AppendLine("    SceneInts:");
            foreach (var sceneInt in flags.SceneInts)
            {
                sb.AppendLine(sceneInt.ToDebugString());
            }

            return sb.ToString();
        }

        [Conditional("DEBUG")]
        public static void Print(this BossRespawnFlags flags)
        {
            PluginLog.Info(flags.ToDebugString());
        }
    }
}
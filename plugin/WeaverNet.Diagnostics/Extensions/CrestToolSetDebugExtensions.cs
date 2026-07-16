using System.Diagnostics;
using System.Text;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class CrestToolSetDebugExtensions
    {
        public static string ToDebugString(this CrestToolSet tools)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Tools:");
            sb.AppendLine("  CrestID: " + tools.CrestID);

            sb.AppendLine("  ToolNames:");
            foreach (var tool in tools.ToolNames)
            {
                sb.AppendLine("    - " + tool);
            }

            sb.AppendLine("  ExtraBlueSlotToolName: " + tools.ExtraBlueSlotToolName);
            sb.AppendLine("  ExtraYellowSlotToolName: " + tools.ExtraYellowSlotToolName);
            sb.AppendLine("  ExtraBlueSlotUnlocked: " + tools.ExtraBlueSlotUnlocked);
            sb.AppendLine("  ExtraYellowSlotUnlocked: " + tools.ExtraYellowSlotUnlocked);

            return sb.ToString();
        }

        [Conditional("DEBUG")]
        public static void Print(this CrestToolSet tools)
        {
            PluginLog.Info(tools.ToDebugString());
        }
    }
}
using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class SceneBoolDebugExtensions
    {
        public static string ToDebugString(this SceneBool sceneBool)
        {
            return string.Format(
                "      SceneBool:\n" +
                "        SceneName: {0}\n" +
                "        ID:        {1}\n" +
                "        Value:     {2}",
                sceneBool.SceneName,
                sceneBool.ID,
                sceneBool.Value);
        }

        [Conditional("DEBUG")]
        public static void Print(this SceneBool sceneBool)
        {
            PluginLog.Info(sceneBool.ToDebugString());
        }
    }
}
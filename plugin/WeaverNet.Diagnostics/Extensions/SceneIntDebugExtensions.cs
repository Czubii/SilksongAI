using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class SceneIntDebugExtensions
    {
        public static string ToDebugString(this SceneInt sceneInt)
        {
            return string.Format(
                "      SceneInt:\n" +
                "        SceneName: {0}\n" +
                "        ID:        {1}\n" +
                "        Value:     {2}\n" +
                "        Mutator:   {3}",
                sceneInt.SceneName,
                sceneInt.ID,
                sceneInt.Value,
                sceneInt.Mutator);
        }

        [Conditional("DEBUG")]
        public static void Print(this SceneInt sceneInt)
        {
            PluginLog.Info(sceneInt.ToDebugString());
        }
    }
}
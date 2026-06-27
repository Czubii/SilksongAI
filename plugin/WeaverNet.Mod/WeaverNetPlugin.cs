using BepInEx;
using UnityEngine;

namespace WeaverNet.Mod
{
    [BepInPlugin(
        "com.weavernet.mod",
        "WeaverNet Mod",
        "0.1.0"
    )]
    public class WeaverNetPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("WeaverNet Mod loading...");
            Logger.LogInfo("WeaverNet Core initialized.");
        }

        private void OnDestroy()
        {
            Logger.LogInfo("WeaverNet Mod shutdown.");
        }
    }
}
using BepInEx.Configuration;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
 
    public static class PluginConfigManager
    {
        public static void LoadConfig(ConfigFile configFile)
        {
            RewardConfig.LoadFromFile(configFile);
        }
    }
    public class ConfigurationManagerAttributes
    {
        public bool? IsAdvanced = null;
        public Action<ConfigEntryBase> CustomDrawer = null;
    }
    public static class SessionConfig
    {
        public enum RecordingOutputTypes
        {
            JSON, // Only for debugging. Python wont be able to load those recording as of now
            MSGPACK
        }
        public static readonly int CaptureFrameDelta = 10;
        public static readonly RecordingOutputTypes RecordingOutputType = RecordingOutputTypes.JSON;
        public static readonly int AwaitBossTimeoutFrames = 2000;
    }

    public static class RewardConfig
    {
        // the squaring of damage delt per single frame encourages the use of skills as they deal 
        // more instant damage, so some non-linearity is needed 
        public static ConfigEntry<int> DAMAGE_DELT;
        public static ConfigEntry<int> DAMAGE_TAKEN;
        public static ConfigEntry<int> TIME_PENALTY;
        public static ConfigEntry<int> LOW_HEALTH_PENALTY;
        public static ConfigEntry<int> LOW_HEALTH_THRESHOLD;
        public static ConfigEntry<int> HIGH_SLIK_PENALTY;
        public static ConfigEntry<int> HIGH_SLIK_THRESHOLD;

        //TODO IMPLEMENT:
        public static ConfigEntry<int> WIN_REWARD;
        public static ConfigEntry<int> LOSS_PENALTY;

        private static readonly string _configSection = "Rewards";
        private static readonly ConfigDescription _configDescription =
            new ConfigDescription("", null, new ConfigurationManagerAttributes
            {
                IsAdvanced = true,
            });
        public static void LoadFromFile(ConfigFile file)
        {
            DAMAGE_DELT =
                file.Bind(_configSection, "Damage Delt Reward", 2, _configDescription);
            DAMAGE_TAKEN =
                file.Bind(_configSection, "Damage Taken Penalty", -400, _configDescription);
            TIME_PENALTY =
                file.Bind(_configSection, "Time Penalty", -1, _configDescription);
            LOW_HEALTH_PENALTY =
                file.Bind(_configSection, "Low Health Penalty", -15, _configDescription);
            LOW_HEALTH_THRESHOLD =
                file.Bind(_configSection, "Low Health Threshold", 2, _configDescription);
            HIGH_SLIK_PENALTY =
                file.Bind(_configSection, "High Silk Penalty", -5, _configDescription);
            HIGH_SLIK_THRESHOLD =
                file.Bind(_configSection, "High Silk Threshold", 9, _configDescription);
            WIN_REWARD =
                file.Bind(_configSection, "Win Reward", 5000, _configDescription);
            LOSS_PENALTY =
                file.Bind(_configSection, "Loss Penalty", -5000, _configDescription);
        }
    }

}

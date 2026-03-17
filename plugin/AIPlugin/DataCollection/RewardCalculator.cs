using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace AIPlugin.Utilities
{
    public static class RewardCalculator
    {

        private static ConfigEntry<int> DAMAGE_DELT;
        private static ConfigEntry<int> DAMAGE_TAKEN;
        private static ConfigEntry<int> TIME_PENALTY;
        private static ConfigEntry<int> LOW_HEALTH_PENALTY;
        private static ConfigEntry<int> LOW_HEALTH_THRESHOLD;
        private static ConfigEntry<int> HIGH_SLIK_PENALTY;
        private static ConfigEntry<int> HIGH_SLIK_THRESHOLD;

        //TODO IMPLEMENT:
        private static ConfigEntry<int> WIN_REWARD;
        private static ConfigEntry<int> LOSS_PENALTY;

        private static readonly string _configSection = "Rewards";
        private static readonly ConfigDescription _configDescription =
            new ConfigDescription("", null, new ConfigurationManagerAttributes
            {
                IsAdvanced = true,
            });
        public static void Bind(ConfigFile file)
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

        public static int Calculate(RecordingFrame prevFrame,  RecordingFrame currFrame)
        {
            var prev = prevFrame.Data;
            var curr = currFrame.Data;

            int total = 0;

            int damageDelt = 0; 
            for (int i = 0; i < prev.Enemies.Length; i++)
            {
                damageDelt += prev.Enemies[i].HP - curr.Enemies[i].HP;
            }
            total += damageDelt * damageDelt * DAMAGE_DELT.Value;

            int damageTaken = prev.Hero.HP - curr.Hero.HP;
            total += damageTaken * DAMAGE_TAKEN.Value;

            total += TIME_PENALTY.Value;

            if (curr.Hero.HP <= LOW_HEALTH_THRESHOLD.Value) 
                total += LOW_HEALTH_PENALTY.Value;
            if (curr.Hero.Silk >= HIGH_SLIK_THRESHOLD.Value) 
                total += HIGH_SLIK_PENALTY.Value;
            return total;
        }
    }
}

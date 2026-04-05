using AIPlugin.BossfightSession;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    public static class RewardCalculator
    {

        private static ConfigEntry<int> DAMAGE_DELT;
        private static ConfigEntry<int> DAMAGE_TAKEN;
        private static ConfigEntry<int> HEAL;

        private static ConfigEntry<int> WIN;
        private static ConfigEntry<int> LOSE;

        private static readonly string _configSection = "Rewards";
        private static readonly ConfigDescription _configDescription =
            new ConfigDescription("", null, new ConfigurationManagerAttributes
            {
                IsAdvanced = true,
            });
        public static void Bind(ConfigFile file)
        {
            DAMAGE_DELT =   file.Bind(_configSection, "Damage Delt", 2, _configDescription);
            DAMAGE_TAKEN =  file.Bind(_configSection, "Damage Taken", -8, _configDescription);
            HEAL =          file.Bind(_configSection, "Heart Healed", 2, _configDescription);
            WIN =           file.Bind(_configSection, "Fight Won", 100, _configDescription);
            LOSE =          file.Bind(_configSection, "Fight Lost", -100, _configDescription);
        }

        public static int Calculate(RecordingFrame prevFrame,  RecordingFrame currFrame, AttemptResult? result = null)
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

            if (curr.Hero.HP < prev.Hero.HP)
            {
                int damageTaken = prev.Hero.HP - curr.Hero.HP;
                total += damageTaken * DAMAGE_TAKEN.Value;
            }
            else if (curr.Hero.HP > prev.Hero.HP)
            {
                int damageHealed = curr.Hero.HP - prev.Hero.HP;
                total += damageHealed * HEAL.Value;
            }

            if (result != null)
            {
                if (result == AttemptResult.Success)
                {
                    total += WIN.Value;
                }
                else
                {
                    total += LOSE.Value;
                }
            }

            return total;
        }
    }
}

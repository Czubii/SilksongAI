using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIPlugin.PluginConfig;

namespace AIPlugin.Utilities
{
    public static class RewardCalculator
    {
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
            total += damageDelt * damageDelt * ConfigEntries.Rewards.DAMAGE_DELT.Value;

            int damageTaken = prev.Hero.HP - curr.Hero.HP;
            total += damageTaken * ConfigEntries.Rewards.DAMAGE_TAKEN.Value;

            total += ConfigEntries.Rewards.TIME_PENALTY.Value;

            if (curr.Hero.HP <= ConfigEntries.Rewards.LOW_HEALTH_THRESHOLD.Value) 
                total += ConfigEntries.Rewards.LOW_HEALTH_PENALTY.Value;
            if (curr.Hero.Silk >= ConfigEntries.Rewards.HIGH_SLIK_THRESHOLD.Value) 
                total += ConfigEntries.Rewards.HIGH_SLIK_PENALTY.Value;
            return total;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            total += damageDelt * damageDelt * RewardConfig.DAMAGE_DELT.Value;

            int damageTaken = prev.Hero.HP - curr.Hero.HP;
            total += damageTaken * RewardConfig.DAMAGE_TAKEN.Value;

            total += RewardConfig.TIME_PENALTY.Value;

            if (curr.Hero.HP <= RewardConfig.LOW_HEALTH_THRESHOLD.Value) 
                total += RewardConfig.LOW_HEALTH_PENALTY.Value;
            if (curr.Hero.Silk >= RewardConfig.HIGH_SLIK_THRESHOLD.Value) 
                total += RewardConfig.HIGH_SLIK_PENALTY.Value;
            return total;
        }
    }
}

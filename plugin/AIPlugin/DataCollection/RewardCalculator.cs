using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    public static class RewardCalculator
    {
        public static int Calculate(RecordingFrameData prev,  RecordingFrameData curr)
        {
            int total = 0;

            int damageDelt = 0; 
            for (int i = 0; i < prev.Enemies.Length; i++)
            {
                damageDelt += prev.Enemies[i].hp - curr.Enemies[i].hp;
            }
            total += damageDelt * damageDelt * RewardConfig.DAMAGE_DELT.Value;

            int damageTaken = prev.Hero.hp - curr.Hero.hp;
            total += damageTaken * RewardConfig.DAMAGE_TAKEN.Value;

            total += RewardConfig.TIME_PENALTY.Value;

            if (curr.Hero.hp <= RewardConfig.LOW_HEALTH_THRESHOLD.Value) 
                total += RewardConfig.LOW_HEALTH_PENALTY.Value;
            if (curr.Hero.silk >= RewardConfig.HIGH_SLIK_THRESHOLD.Value) 
                total += RewardConfig.HIGH_SLIK_PENALTY.Value;
            return total;
        }
    }
}

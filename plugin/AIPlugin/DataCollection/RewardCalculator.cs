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

            int targetDamageDelt = prev.Enemies[0].hp - curr.Enemies[0].hp;
            total += targetDamageDelt * RewardConfig.TARGET_DAMAGE_DELT;

            int nonTargetDamageDelt = 0;
            for (int i = 1; i < prev.Enemies.Length; i++)
            {
                nonTargetDamageDelt += prev.Enemies[i].hp - curr.Enemies[i].hp;
            }
            total += nonTargetDamageDelt * RewardConfig.NON_TARGET_DAMAGE_DELT;

            int damageTaken = prev.Hero.hp - curr.Hero.hp;
            total += damageTaken * RewardConfig.DAMAGE_TAKEN;

            total += RewardConfig.TIME_PENALTY;

            if (curr.Hero.hp <= RewardConfig.LOW_HEALTH_THRESHOLD) total += RewardConfig.LOW_HEALTH_PENALTY;

            return total;
        }
    }
}

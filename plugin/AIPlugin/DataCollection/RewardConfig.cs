using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    public static class RewardConfig //TODO add loading from file
    {
        public static readonly int TARGET_DAMAGE_DELT = 20; // points per 1hp
        public static readonly int NON_TARGET_DAMAGE_DELT = 4; // points per 1hp
        public static readonly int DAMAGE_TAKEN = -600; // points per 1hp
        public static readonly int TIME_PENALTY = -1; // points per 1 frame
        public static readonly int LOW_HEALTH_PENALTY = -15; // points for having <= LOW_HEALTH_THRESHOLD hearts on frame
        public static readonly int LOW_HEALTH_THRESHOLD = 2;

        public static readonly int MISSED_ATACK_PENALTY = -40;


        //TODO IMPLEMENT:
        public static readonly int WIN_REWARD = 5000;
        public static readonly int LOSS_PENALTY = -5000;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game
{
    public class AbilitySet
    {
        public bool Dash { get; }
        public bool DoubleJump { get; }
        public bool WallJump { get; }
        public bool HarpoonDash { get; }
        public bool SuperJump { get; }
        public bool Brolly { get; }
        public bool ChargeSlash { get; }

        public int HP { get; }
        public int Silk { get; }
        public int SilkRegen { get; }

        public AbilitySet(
            bool dash,
            bool doubleJump,
            bool wallJump,
            bool harpoonDash,
            bool superJump,
            bool brolly,
            bool chargeSlash,
            int hp,
            int silk,
            int silkRegen)
        {
            Dash = dash;
            DoubleJump = doubleJump;
            WallJump = wallJump;
            HarpoonDash = harpoonDash;
            SuperJump = superJump;
            Brolly = brolly;
            ChargeSlash = chargeSlash;

            HP = hp;
            Silk = silk;
            SilkRegen = silkRegen;
        }
    }
}

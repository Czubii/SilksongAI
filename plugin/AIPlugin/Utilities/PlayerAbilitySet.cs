using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    public struct PlayerAbilitySet
    {
        public bool Dash;
        public bool DoubleJump;
        public bool WallJump;
        public bool HarpoonDash;
        public bool SuperJump;
        public bool Brolly;
        public bool ChargeSlash;

        public int HP;
        public int Silk;
        public int SilkRegen;


        public void Apply(PlayerData pd)
        {
            pd.hasDash = Dash;
            pd.hasDoubleJump = DoubleJump;
            pd.hasWalljump = WallJump;
            pd.hasHarpoonDash = HarpoonDash;
            pd.hasSuperJump = SuperJump;
            pd.hasBrolly = Brolly;
            pd.hasChargeSlash = ChargeSlash;

            //pd.maxHealth = HP;
            //pd.maxHealthBase = HP;
            //pd.silkMax = Silk;
            //pd.silkRegenMax = SilkRegen;
        }

        public void ApplyTemp(PlayerData pd, TemporaryStateModifier modifier)
        {
            modifier.ModifyVar((v) => pd.hasDash = v, pd.hasDash, Dash);
            modifier.ModifyVar((v) => pd.hasDoubleJump = v, pd.hasDoubleJump, DoubleJump);
            modifier.ModifyVar((v) => pd.hasWalljump = v, pd.hasWalljump, WallJump);
            modifier.ModifyVar((v) => pd.hasHarpoonDash = v, pd.hasHarpoonDash, HarpoonDash);
            modifier.ModifyVar((v) => pd.hasSuperJump = v, pd.hasSuperJump, SuperJump);
            modifier.ModifyVar((v) => pd.hasBrolly = v, pd.hasBrolly, Brolly);
            modifier.ModifyVar((v) => pd.hasChargeSlash = v, pd.hasChargeSlash, ChargeSlash);


            //modifier.ModifyVar((v) => pd.maxHealth = v, pd.maxHealth, HP);
            //modifier.ModifyVar((v) => pd.maxHealthBase = v, pd.maxHealthBase, HP);
            //modifier.ModifyVar((v) => pd.silkMax = v, pd.silkMax, Silk);
            //modifier.ModifyVar((v) => pd.silkRegenMax = v, pd.silkRegenMax, SilkRegen);
        }
    }
}

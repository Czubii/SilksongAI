using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game
{
    public class LoadoutManager : ILoadoutManager
    {
        private PlayerData CurrentPlayerData { 
            get 
            {
                if (PlayerData.instance == null) throw new InvalidOperationException("No Exisiting PlayerData instance");
                return PlayerData.instance; 
            }  
        }
        public AbilitySet GetAbilities()
        {
            var pd = CurrentPlayerData;

            return new AbilitySet(
               pd.hasDash,
               pd.hasDoubleJump,
               pd.hasWalljump,
               pd.hasHarpoonDash,
               pd.hasSuperJump,
               pd.hasBrolly,
               pd.hasChargeSlash,
               pd.maxHealth,
               pd.silkMax,
               pd.silkRegenMax
           );

        }
        public CrestToolSet GetCrestToolSet()
        {
            throw new NotImplementedException();
        }
        public Loadout GetLoadout()
        {
            throw new NotImplementedException();
        }
        public void SetAbilities(AbilitySet abilitySet)
        {
            if (abilitySet == null)
                throw new ArgumentNullException(nameof(abilitySet));

            var pd = CurrentPlayerData;

            pd.hasDash = abilitySet.Dash;
            pd.hasDoubleJump = abilitySet.DoubleJump;
            pd.hasWalljump = abilitySet.WallJump;
            pd.hasHarpoonDash = abilitySet.HarpoonDash;
            pd.hasSuperJump = abilitySet.SuperJump;
            pd.hasBrolly = abilitySet.Brolly;
            pd.hasChargeSlash = abilitySet.ChargeSlash;

            pd.maxHealth = abilitySet.HP;
            pd.maxHealthBase = abilitySet.HP;
            pd.silkMax = abilitySet.Silk;
            pd.silkRegenMax = abilitySet.SilkRegen;
        }
        public void SetAbilitiesTemporary(AbilitySet abilitySet, TemporaryStateModifier modifier)
        {
            if (abilitySet == null)
                throw new ArgumentNullException(nameof(abilitySet));

            var pd = CurrentPlayerData;

            var currentAbilities = GetAbilities();
            modifier.AddUndo(() =>
            {
                SetAbilities(currentAbilities);
            });

            SetAbilities(abilitySet);
        }
        public void SetCrestToolSet(CrestToolSet toolSet)
        {
            throw new NotImplementedException();
        }
        public void SetCrestToolSetTemporary(CrestToolSet toolSet, TemporaryStateModifier modifier)
        {
            throw new NotImplementedException();
        }
        public void SetLoadout(Loadout Loadout)
        {
            throw new NotImplementedException();
        }
        public void SetLoadoutTemporary(Loadout Loadout, TemporaryStateModifier modifier)
        {
            throw new NotImplementedException();
        }
    }
}

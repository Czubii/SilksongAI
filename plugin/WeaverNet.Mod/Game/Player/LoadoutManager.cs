using System;
using System.Linq;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game.Player
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
        private AbilitySet GetAbilities(PlayerData pd)
        {
            return new AbilitySet(
               pd.hasDash,
               pd.hasDoubleJump,
               pd.hasWalljump,
               pd.hasHarpoonDash,
               pd.hasSuperJump,
               pd.hasBrolly,
               pd.hasChargeSlash,
               pd.hasNeedolin,
               pd.HasBoundCrestUpgrader
           );

        }
        private CrestToolSet GetCrestToolSet(PlayerData pd)
        {
            string crestId = pd.CurrentCrestID;
            var tools = ToolItemManager.GetEquippedToolsForCrest(crestId);

            return new CrestToolSet(
                crestId,
                tools.Select(a => a?.name ?? "").ToList(),
                pd.ExtraToolEquips.GetData("Defend1").EquippedTool,
                pd.ExtraToolEquips.GetData("Explore1").EquippedTool,
                pd.UnlockedExtraBlueSlot,
                pd.UnlockedExtraYellowSlot
            );
        }
        private PlayerUpgradeSet GetPlayerUpgradeSet(PlayerData pd)
        {
            return new PlayerUpgradeSet(
                pd.maxHealthBase,
                pd.silkMax,
                pd.silkRegenMax,
                pd.ToolKitUpgrades,
                pd.ToolPouchUpgrades,
                pd.nailUpgrades
            );
        }
        public Loadout GetLoadout()
        {
            var pd = CurrentPlayerData;
            return new Loadout(
                GetAbilities(pd),
                GetCrestToolSet(pd),
                GetPlayerUpgradeSet(pd)
            );
        }
        private void SetAbilities(PlayerData pd, AbilitySet abilitySet)
        {

            pd.hasDash = abilitySet.Dash;
            pd.hasDoubleJump = abilitySet.DoubleJump;
            pd.hasWalljump = abilitySet.WallJump;
            pd.hasHarpoonDash = abilitySet.HarpoonDash;
            pd.hasSuperJump = abilitySet.SuperJump;
            pd.hasBrolly = abilitySet.Brolly;
            pd.hasChargeSlash = abilitySet.ChargeSlash;
            pd.hasNeedolin = abilitySet.Needolin;
            pd.HasBoundCrestUpgrader = abilitySet.Sylphsong;

        }
        private void SetCrestToolSet(PlayerData pd, CrestToolSet toolSet)
        {
            ToolItemManager.SetEquippedTools(toolSet.CrestID, toolSet.ToolNames.ToList());
            ToolItemManager.SetExtraEquippedTool("Defend1", toolSet.ExtraBlueSlotToolName);
            ToolItemManager.SetExtraEquippedTool("Explore1", toolSet.ExtraYellowSlotToolName);
            pd.UnlockedExtraBlueSlot = toolSet.ExtraBlueSlotUnlocked;
            pd.UnlockedExtraYellowSlot = toolSet.ExtraYellowSlotUnlocked;
            
            ToolItemManager.SetEquippedCrest(toolSet.CrestID);
        }
        private void SetPlayerUpgradeSet(PlayerData pd, PlayerUpgradeSet pus)
        {
            pd.maxHealthBase = pus.HP;
            pd.silkMax = pus.Silk;
            pd.silkRegenMax = pus.SilkHearts;
            pd.ToolKitUpgrades = pus.CraftingKits;
            pd.ToolPouchUpgrades = pus.ToolPouches;
            pd.nailUpgrades = pus.NailUpgrades;
        }

        public void SetLoadout(Loadout loadout)
        {
            var pd = CurrentPlayerData;
            
            SetCrestToolSet(pd, loadout.Tools);
            SetAbilities(pd, loadout.Abilities);
            SetPlayerUpgradeSet(pd, loadout.Upgrades);

            ToolItemManager.SendEquippedChangedEvent(true); // refreshes UI for health + silk + tools + crest (easiest method i found so far)
        }
        public void SetLoadoutTemporary(Loadout Loadout, TemporaryStateModifier modifier)
        {
            var currLoadout = GetLoadout();
            modifier.AddUndo(() =>
            {
                SetLoadout(currLoadout);
            });
            SetLoadout(Loadout);
        }
    }
}

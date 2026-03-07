using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    public interface IBossBehavior
    {
        void Respawn(TemporaryStateModifier modifier);
        void SetAbilities(TemporaryStateModifier modifier);
    }
    public class MossMotherBehavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.defeatedMossMother = v, pd.defeatedMossMother, false);
            modifier.ModifyVar((v) => pd.spinnerDefeated = v, pd.spinnerDefeated, false);

            SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
            {
                SceneName = "Tut_03",
                ID = "Battle Scene",
                Value = false
            });

            modifier.AddUndo(() => {
                SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
                {
                    SceneName = "Tut_03",
                    ID = "Battle Scene",
                    Value = true
                });
            });
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = false,
                DoubleJump = false,
                WallJump = false,
                HarpoonDash = false,
                SuperJump = false,
                Brolly = false,
                ChargeSlash = false,

                HP = 10,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }

    public class BellBeastBehavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.defeatedBellBeast = v, pd.defeatedBellBeast, false);
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = false,
                DoubleJump = false,
                WallJump = false,
                HarpoonDash = false,
                SuperJump = false,
                Brolly = false,
                ChargeSlash = false,

                HP = 5,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }

    public class Lace1Behavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.defeatedLace1 = v, pd.defeatedLace1, false);
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = true,
                DoubleJump = false,
                WallJump = false,
                HarpoonDash = false,
                SuperJump = false,
                Brolly = false,
                ChargeSlash = false,

                HP = 5,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }

    public class FourthChorusBehavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.defeatedSongGolem = v, pd.defeatedSongGolem, false);
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = true,
                Brolly = true,
                DoubleJump = false,
                WallJump = false,
                HarpoonDash = false,
                SuperJump = false,
                ChargeSlash = false,

                HP = 5,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }

    public class MoorwingBehavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.defeatedVampireGnatBoss = v, pd.defeatedVampireGnatBoss, false);
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = true,
                Brolly = true,
                DoubleJump = false,
                WallJump = false,
                HarpoonDash = false,
                SuperJump = false,
                ChargeSlash = false,

                HP = 5,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }
    public class SisterSplinterBehavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.defeatedSplinterQueen = v, pd.defeatedSplinterQueen, false);
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = true,
                Brolly = true,
                DoubleJump = false,
                WallJump = false,
                HarpoonDash = false,
                SuperJump = false,
                ChargeSlash = false,

                HP = 5,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }
    public class WidowBehavior : IBossBehavior
    {
        public void Respawn(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            modifier.ModifyVar((v) => pd.spinnerDefeated = v, pd.spinnerDefeated, false);
            modifier.ModifyVar((v) => pd.encounteredSpinner = v, pd.encounteredSpinner, true);
            modifier.ModifyVar((v) => pd.bellShrineBellhart = v, pd.bellShrineBellhart, false);
            SceneData.instance.PersistentInts.SetValue(new PersistentItemData<int> //disables bench in Widow fight while boss is active
            {
                SceneName = "Belltown_Shrine",
                ID = "Bellshrine Sequence Bellhart",
                Value = 0,
                Mutator = 0
            });

            modifier.AddUndo(() => {
                SceneData.instance.PersistentInts.SetValue(new PersistentItemData<int> //disables bench in Widow fight while boss is active
                {
                    SceneName = "Belltown_Shrine",
                    ID = "Bellshrine Sequence Bellhart",
                    Value = 1, //TODO verify this works
                    Mutator = 0
                });
            });
        }
        public void SetAbilities(TemporaryStateModifier modifier)
        {
            new PlayerAbilitySet()
            {
                Dash = true,
                Brolly = true,
                WallJump = true,
                HarpoonDash = false,
                ChargeSlash = false,
                DoubleJump = false,
                SuperJump = false,

                HP = 5,
                Silk = 8,
                SilkRegen = 0
            }.ApplyTemp(PlayerData.instance, modifier);
        }
    }
}

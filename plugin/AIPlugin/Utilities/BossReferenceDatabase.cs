using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{

    public class BossMetadata
    {
        public string DisplayName;
        public string InternalName;
        public string ArenaSceneName;
        public Vector3 ArenaPosition;
        public bool CanRespawnOnArena = true; // TODO: implement (for example false for fouth chorus)
        public bool RequireHardSceneReload = false;
        public Action<bool> SetDefeated = _ => AIPlugin.Log.LogWarning("Trying to respawn a boss with undefined SetDefeated action!");
        public Action SetExpectedPlayerAbilities = () => AIPlugin.Log.LogWarning("Trying to set player state with undefined SetExpectedPlayerResources action!");
    }
    public static class BossReferenceDatabase
    {
        private static void RemoveAllTools(string crestName) //TODO move to player utils
        {
            var p = PlayerData.instance;

            var crest = p.ToolEquips.GetData(crestName);

            for (int i = 0; i < crest.Slots.Count; i++)
            {
                var slot = crest.Slots[i];
                slot.EquippedTool = "";
                crest.Slots[i] = slot;  
            }
        }
        private static void PrintEquippedTools(string crestName)//TODO move to player utils
        {
            var p = PlayerData.instance;

            var crest = p.ToolEquips.GetData(crestName);

            for (int i = 0; i < crest.Slots.Count; i++)
            {
                AIPlugin.Log.LogMessage($"Slot {i}: {crest.Slots[i].EquippedTool}");
            }
        }
        private static void HunterEquipSilkSpear()//TODO move to player utils and make more usefill
        {
            var p = PlayerData.instance;

            var crest = p.ToolEquips.GetData("Hunter");

            var slot = crest.Slots[3];
            slot.EquippedTool = "Silk Spear";
            crest.Slots[3] = slot;
        }

        public static readonly List<BossMetadata> All = new List<BossMetadata>
        {
            new BossMetadata
            {
                DisplayName = "Moss Mother",
                InternalName = "Mossbone Mother",
                ArenaSceneName = "Tut_03",
                ArenaPosition = new Vector3(68f, 17.6f, 0),
                RequireHardSceneReload = true,
                SetDefeated = (val) => 
                {
                    PlayerData.instance.defeatedMossMother = val;
                    PlayerData.instance.spinnerDefeated = false; // otherwise the chapel maid may show up in the arena if we have beaten the widow
                    SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
                    {
                        SceneName = "Tut_03",
                        ID = "Battle Scene",
                        Value = val
                    });
                },
                SetExpectedPlayerAbilities = () => //TODO rework this
                {
                    
                    var p = PlayerData.instance;
                    p.hasDash = false;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = false;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter"; 
                    RemoveAllTools("Hunter");

                    p.silkRegenMax = 0;
                }
            },
            new BossMetadata
            {
                DisplayName = "Bell Beast",
                InternalName = "Bone Beast",
                ArenaSceneName = "Bone_05",
                ArenaPosition = new Vector3(78.59f, 3.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedBellBeast = val;
                },
                SetExpectedPlayerAbilities = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = false;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = false;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter";
                    RemoveAllTools("Hunter");
                    HunterEquipSilkSpear();

                    p.silkRegenMax = 0;

                }
            },
            new BossMetadata
            {
                DisplayName = "Lace 1",
                InternalName = "Lace Boss1",
                ArenaSceneName = "Bone_East_12",
                ArenaPosition = new Vector3(90f, 7.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedLace1 = val;
                },
                SetExpectedPlayerAbilities = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = true;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = false;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter";
                    RemoveAllTools("Hunter");
                    HunterEquipSilkSpear();

                    p.silkRegenMax = 0;

                }
            },
            new BossMetadata
            {
                DisplayName = "Fourth Chorus",
                InternalName = "SG_head",
                ArenaSceneName = "Bone_East_08",
                ArenaPosition = new Vector3(80.4f, 7.1f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedSongGolem = val;
                },
                SetExpectedPlayerAbilities = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = true;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = true;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter";
                    RemoveAllTools("Hunter");
                    HunterEquipSilkSpear();

                    p.silkRegenMax = 0;

                }
            },
            new BossMetadata
            {
                DisplayName = "Moorwing",
                InternalName = "Vampire Gnat",
                ArenaSceneName = "Greymoor_08",
                ArenaPosition = new Vector3(40.5f, 4.7f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedVampireGnatBoss = val;
                },
                SetExpectedPlayerAbilities = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = true;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = true;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter";
                    RemoveAllTools("Hunter");
                    HunterEquipSilkSpear();

                    p.silkRegenMax = 0;

                }
            },
            new BossMetadata
            {
                DisplayName = "Sister Splinter",
                InternalName = "Splinter Queen",
                ArenaSceneName = "Shellwood_18",
                ArenaPosition = new Vector3(52f, 8.6f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedSplinterQueen = val;
                },
                SetExpectedPlayerAbilities = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = true;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = true;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter";
                    RemoveAllTools("Hunter");
                    HunterEquipSilkSpear();

                    p.silkRegenMax = 0;

                }
            },
            new BossMetadata
            {                   
                DisplayName = "Widow",
                InternalName = "Spinner Boss",
                ArenaSceneName = "Belltown_Shrine",
                ArenaPosition = new Vector3(52.4f, 8.6f, 0),
                RequireHardSceneReload = true,
                SetDefeated = (val) =>
                {
                    PlayerData.instance.spinnerDefeated = val;
                    PlayerData.instance.encounteredSpinner = true;
                    PlayerData.instance.bellShrineBellhart = val;
                    SceneData.instance.PersistentInts.SetValue(new PersistentItemData<int> //disables bench in Widow fight while boss is active
                    {
                        SceneName = "Belltown_Shrine",
                        ID = "Bellshrine Sequence Bellhart",
                        Value = 0,
                        Mutator = 0
                    });
                },
                SetExpectedPlayerAbilities = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = true;
                    p.hasDoubleJump = false;
                    p.hasWalljump = true;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = true;
                    p.hasChargeSlash = false;
                    
                    p.CurrentCrestID = "Hunter";
                    RemoveAllTools("Hunter");
                    HunterEquipSilkSpear();

                    p.silkRegenMax = 0;

                }
            }

        };
    }
}

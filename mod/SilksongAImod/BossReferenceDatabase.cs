using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SilksongAI
{

    public class BossMetaData
    {
        public string DisplayName;
        public string InternalName;
        public string ArenaMapName;
        public Vector3 ArenaPosition;
        public Action<bool> SetDefeated = _ => SilksongAImod.Log.LogWarning("Trying to respawn a boss with undefined SetDefeated action!");
        public Action SetExpectedPlayerResources = () => SilksongAImod.Log.LogWarning("Trying to set player state with undefined SetExpectedPlayerResources action!");
    }
    public static class BossReferenceDatabase
    {

        public static readonly List<BossMetaData> All = new List<BossMetaData>
        {
            new BossMetaData
            {
                DisplayName = "Moss Mother", //TODO add fix for this one boss where it does not respaawn when teleporting from the same scene
                InternalName = "Mossbone Mother",
                ArenaMapName = "Tut_03",
                ArenaPosition = new Vector3(68f, 17.6f, 0),
                SetDefeated = (val) => 
                {
                    PlayerData.instance.defeatedMossMother = val;
                    SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
                    {
                        SceneName = "Tut_03",
                        ID = "Battle Scene",
                        Value = val
                    });
                },
                SetExpectedPlayerResources = () =>
                {
                    //TODO add silkspear
                    var p = PlayerData.instance;
                    p.hasDash = false;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = false;
                    p.hasChargeSlash = false;
                    //p.silkRegenMax = 0;
                }
            },
            new BossMetaData
            {
                DisplayName = "Bell Beast",
                InternalName = "Bone Beast",
                ArenaMapName = "Bone_05",
                ArenaPosition = new Vector3(78.59f, 3.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedBellBeast = val;
                },
                SetExpectedPlayerResources = () =>
                {
                    var p = PlayerData.instance;
                    p.hasDash = false;
                    p.hasDoubleJump = false;
                    p.hasWalljump = false;
                    p.hasHarpoonDash = false;
                    p.hasSuperJump = false;
                    p.hasBrolly = false;
                    p.hasChargeSlash = false;

                }
            },
            new BossMetaData
            {
                DisplayName = "Lace 1",
                InternalName = "Lace Boss1",
                ArenaMapName = "Bone_East_12",
                ArenaPosition = new Vector3(85f, 7.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedLace1 = val;
                }
            },
            new BossMetaData
            {
                DisplayName = "Fourth Chorus",
                InternalName = "SG_head",
                ArenaMapName = "Bone_East_08",
                ArenaPosition = new Vector3(80.4f, 7.1f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedSongGolem = val;
                }
            },
            new BossMetaData
            {
                DisplayName = "Moorwing",
                InternalName = "Vampire Gnat",
                ArenaMapName = "Greymoor_08",
                ArenaPosition = new Vector3(40.5f, 4.7f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedVampireGnatBoss = val;
                }
            },
            new BossMetaData
            {
                DisplayName = "Sister Splinter",
                InternalName = "Splinter Queen",
                ArenaMapName = "Shellwood_18",
                ArenaPosition = new Vector3(52f, 8.6f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedSplinterQueen = val;
                }
            },
            new BossMetaData //TODO fix bench when invoking fight from the same room
            {
                DisplayName = "Widow",
                InternalName = "Spinner Boss",
                ArenaMapName = "Belltown_Shrine",
                ArenaPosition = new Vector3(52.4f, 8.6f, 0),
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
                }
            }

        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game
{
    public class CrestToolSet
    {
        public string CrestID { get; }
        public IReadOnlyList<string> ToolNames { get; }
        public string ExtraBlueSlotToolName { get; }
        public string ExtraYellowSlotToolName { get; }
        public bool ExtraBlueSlotUnlocked { get; }
        public bool ExtraYellowSlotUnlocked { get; }
        public CrestToolSet(string crestID, IReadOnlyList<string> toolNames, string extraBlueSlotToolName, string extraYellowSlotToolName, bool extraBlueSlotUnlocked, bool extraYellowSlotUnlocked)
        {
            CrestID = crestID;
            ToolNames = toolNames;
            ExtraBlueSlotToolName = extraBlueSlotToolName;
            ExtraYellowSlotToolName = extraYellowSlotToolName;
            ExtraBlueSlotUnlocked = extraBlueSlotUnlocked;
            ExtraYellowSlotUnlocked = extraYellowSlotUnlocked;
        }
    }
    //Possible Crests:
    //  Hunter
    //  Wanderer
    //  Reaper
    //  Hunter_v2
    //  Cloakless
    //  Warrior
    //  Cursed
    //  Witch
    //  Toolmaster
    //  Hunter_v3
    //  Spell


    //Possible Tools:
    //  Bone Necklace
    //  Compass
    //  Silk Spear
    //  Rosary Magnet
    //  Mosscreep Tool 1
    //  Straight Pin
    //  Bell Bind
    //  Dead Mans Purse
    //  Flea Brew
    //  Thread Sphere
    //  Harpoon
    //  Sting Shard
    //  Extractor
    //  Lifeblood Syringe
    //  Pimpilo
    //  Weighted Anklet
    //  Poison Pouch
    //  Sprintmaster
    //  Barbed Wire
    //  Tack
    //  Flintstone
    //  Lava Charm
    //  WebShot Forge
    //  Multibind
    //  Dazzle Bind
    //  Screw Attack
    //  Silk Charge
    //  Cogwork Flier
    //  Parry
    //  White Ring
    //  Quickbind
    //  Revenge Crystal
    //  Thief Charm
    //  Wallcling
    //  Rosary Cannon
    //  Shakra Ring
    //  Reserve Bind
    //  Tri Pin
    //  Wisp Lantern
    //  Conch Drill
    //  Spool Extender
    //  Musician Charm
    //  Scuttlebrace
    //  Cogwork Saw
    //  Brolly Spike
    //  Thief Claw
    //  Maggot Charm
    //  Flea Charm
    //  Silk Snare
    //  Silk Bomb
    //  Longneedle
    //  Pinstress Tool
    //  Mosscreep Tool 2
    //  Silk Boss Needle
    //  Fractured Mask
    //  Curve Claws
    //  Curve Claws Upgraded
    //  Lightning Rod
    //  Magnetite Dice
    //  Zap Imbuement
    //  Quick Sling
    //  Dazzle Bind Upgraded

}

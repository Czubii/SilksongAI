using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Game
{
    public class Loadout
    {
        public AbilitySet Abilities { get; }
        public CrestToolSet Tools { get; }
        public Loadout(AbilitySet abilities, CrestToolSet tools)
        {
            Abilities = abilities;
            Tools = tools;
        }
    }
}

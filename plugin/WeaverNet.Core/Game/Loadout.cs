using System;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Game
{
    public class Loadout
    {
        public AbilitySet Abilities { get; }
        public CrestToolSet Tools { get; }
        public PlayerUpgradeSet Upgrades { get; }
        public Loadout(AbilitySet abilities, CrestToolSet tools, PlayerUpgradeSet upgrades)
        {
            Abilities = abilities ?? throw new ArgumentNullException(nameof(abilities)); //TODO implement such pattern in the remaining data classes
            Tools = tools ?? throw new ArgumentNullException(nameof(tools));
            Upgrades = upgrades ?? throw new ArgumentNullException(nameof(upgrades));
        }
    }
}

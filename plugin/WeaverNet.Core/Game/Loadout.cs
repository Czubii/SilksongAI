using System;

namespace WeaverNet.Core.Game
{
    public class Loadout
    {
        public string Name { get; }
        public AbilitySet Abilities { get; }
        public CrestToolSet Tools { get; }
        public PlayerUpgradeSet Upgrades { get; }
        public Loadout(string name, AbilitySet abilities, CrestToolSet tools, PlayerUpgradeSet upgrades)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Abilities = abilities ?? throw new ArgumentNullException(nameof(abilities)); //TODO implement such pattern in the remaining data classes
            Tools = tools ?? throw new ArgumentNullException(nameof(tools));
            Upgrades = upgrades ?? throw new ArgumentNullException(nameof(upgrades));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using WeaverNet.Core.Game;

namespace WeaverNet.Core.Infrastructure
{
    public class LoadoutValueComparer : IEqualityComparer<Loadout>
    {
        public bool Equals(Loadout x, Loadout y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return string.Equals(x.Name, y.Name, StringComparison.Ordinal) &&
                   AbilitiesEquals(x.Abilities, y.Abilities) &&
                   ToolsEquals(x.Tools, y.Tools) &&
                   UpgradesEquals(x.Upgrades, y.Upgrades);
        }

        public int GetHashCode(Loadout obj)
        {
            if (obj is null) return 0;

            var hash = new HashCode();

            hash.Add(obj.Name, StringComparer.Ordinal);
            AddAbilitiesToHash(ref hash, obj.Abilities);
            AddToolsToHash(ref hash, obj.Tools);
            AddUpgradesToHash(ref hash, obj.Upgrades);

            return hash.ToHashCode();
        }

        private static bool AbilitiesEquals(AbilitySet x, AbilitySet y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.Dash == y.Dash &&
                   x.DoubleJump == y.DoubleJump &&
                   x.WallJump == y.WallJump &&
                   x.HarpoonDash == y.HarpoonDash &&
                   x.SuperJump == y.SuperJump &&
                   x.Brolly == y.Brolly &&
                   x.ChargeSlash == y.ChargeSlash &&
                   x.Needolin == y.Needolin &&
                   x.Sylphsong == y.Sylphsong;
        }

        private static bool ToolsEquals(CrestToolSet x, CrestToolSet y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return string.Equals(x.CrestID, y.CrestID, StringComparison.Ordinal) &&
                   string.Equals(x.ExtraBlueSlotToolName, y.ExtraBlueSlotToolName, StringComparison.Ordinal) &&
                   string.Equals(x.ExtraYellowSlotToolName, y.ExtraYellowSlotToolName, StringComparison.Ordinal) &&
                   x.ExtraBlueSlotUnlocked == y.ExtraBlueSlotUnlocked &&
                   x.ExtraYellowSlotUnlocked == y.ExtraYellowSlotUnlocked &&
                   SequenceEqualsIgnoreOrder(x.ToolNames, y.ToolNames);
        }

        private static bool UpgradesEquals(PlayerUpgradeSet x, PlayerUpgradeSet y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.HP == y.HP &&
                   x.Silk == y.Silk &&
                   x.SilkHearts == y.SilkHearts &&
                   x.CraftingKits == y.CraftingKits &&
                   x.ToolPouches == y.ToolPouches &&
                   x.NailUpgrades == y.NailUpgrades;
        }

        private static void AddAbilitiesToHash(ref HashCode hash, AbilitySet abilities)
        {
            if (abilities is null) return;

            hash.Add(abilities.Dash);
            hash.Add(abilities.DoubleJump);
            hash.Add(abilities.WallJump);
            hash.Add(abilities.HarpoonDash);
            hash.Add(abilities.SuperJump);
            hash.Add(abilities.Brolly);
            hash.Add(abilities.ChargeSlash);
            hash.Add(abilities.Needolin);
            hash.Add(abilities.Sylphsong);
        }

        private static void AddToolsToHash(ref HashCode hash, CrestToolSet tools)
        {
            if (tools is null) return;

            hash.Add(tools.CrestID, StringComparer.Ordinal);
            hash.Add(tools.ExtraBlueSlotToolName, StringComparer.Ordinal);
            hash.Add(tools.ExtraYellowSlotToolName, StringComparer.Ordinal);
            hash.Add(tools.ExtraBlueSlotUnlocked);
            hash.Add(tools.ExtraYellowSlotUnlocked);

            // Hash tools in a order-independent manner so tool list order doesn't affect hash
            if (tools.ToolNames != null)
            {
                int toolsCombinedHash = 0;
                foreach (var tool in tools.ToolNames)
                {
                    if (tool != null)
                    {
                        toolsCombinedHash ^= StringComparer.Ordinal.GetHashCode(tool);
                    }
                }
                hash.Add(toolsCombinedHash);
            }
        }

        private static void AddUpgradesToHash(ref HashCode hash, PlayerUpgradeSet upgrades)
        {
            if (upgrades is null) return;

            hash.Add(upgrades.HP);
            hash.Add(upgrades.Silk);
            hash.Add(upgrades.SilkHearts);
            hash.Add(upgrades.CraftingKits);
            hash.Add(upgrades.ToolPouches);
            hash.Add(upgrades.NailUpgrades);
        }

        private static bool SequenceEqualsIgnoreOrder(IReadOnlyList<string> first, IReadOnlyList<string> second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (first is null || second is null) return false;
            if (first.Count != second.Count) return false;

            // Fast path for exact sequential match
            if (first.SequenceEqual(second, StringComparer.Ordinal)) return true;

            // Set comparison if order equipped is arbitrary
            var firstOrdered = first.Where(s => s != null).OrderBy(s => s, StringComparer.Ordinal);
            var secondOrdered = second.Where(s => s != null).OrderBy(s => s, StringComparer.Ordinal);

            return firstOrdered.SequenceEqual(secondOrdered, StringComparer.Ordinal);
        }
    }
}

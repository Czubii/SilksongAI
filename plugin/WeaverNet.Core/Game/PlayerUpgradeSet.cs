namespace WeaverNet.Core.Game
{
    public class PlayerUpgradeSet
    {
        public int HP { get; }
        public int Silk { get; }
        public int SilkHearts { get; }
        public int CraftingKits { get; }
        public int ToolPouches { get; }
        public int NailUpgrades { get; }
        //TODO add nail damage
        public PlayerUpgradeSet(int hp, int silk, int silkHearts, int craftingKits, int toolPouches, int nailUpgrades)
        {
            HP = hp;
            Silk = silk;
            SilkHearts = silkHearts;
            CraftingKits = craftingKits;
            ToolPouches = toolPouches;
            NailUpgrades = nailUpgrades;
        }
    }
}

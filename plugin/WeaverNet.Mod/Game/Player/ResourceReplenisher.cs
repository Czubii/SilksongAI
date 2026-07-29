using System.Collections.Generic;
using System.Linq;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game.Player
{
    public class ResourceReplenisher : IResourceReplenisher
    {
        public void ReplenishHP()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                PluginLog.Warning("no HeroController.instance on scene");
                return;
            }
            heroController.RefillHealthToMax();
        }

        public void ReplenishSilk()
        {
            HeroController heroController = HeroController.instance;
            PlayerData pd = PlayerData.instance;
            if (heroController == null || pd == null)
            {
                PluginLog.Warning("no HeroController.instance on scene");
                return;
            }
            pd.IsSilkSpoolBroken = false;
            heroController.RefillSilkToMaxSilent();
        }

        public void ReplenishTools()
        {
            if (string.IsNullOrEmpty(PlayerData.instance.CurrentCrestID))
            {
                return;
            }

            List<ToolItem> currentEquippedTools = GetCurrentEquippedTools();

            if (currentEquippedTools == null)
            {
                return;
            }

            foreach (ToolItem item in currentEquippedTools)
            {
                if (item == null || !item.IsAutoReplenished())
                    continue;

                ToolItemsData.Data toolData = PlayerData.instance.GetToolData(item.name);
                int maxAmount = ToolItemManager.GetToolStorageAmount(item);

                if (toolData.AmountLeft >= maxAmount)
                    continue;

                toolData.AmountLeft = maxAmount;
                PlayerData.instance.SetToolData(item.name, toolData);
            }

            ToolItemManager.ReportAllBoundAttackToolsUpdated();
            ToolItemManager.SendEquippedChangedEvent(force: true);
        }

        private List<ToolItem> GetCurrentEquippedTools()
        {
            List<ToolItem> obj = ToolItemManager.GetEquippedToolsForCrest(PlayerData.instance.CurrentCrestID) ?? new List<ToolItem>();
            IEnumerable<ToolItem> collection = from data in PlayerData.instance.ExtraToolEquips.GetValidDatas((ToolCrestsData.SlotData data) => !string.IsNullOrEmpty(data.EquippedTool))
                                               select ToolItemManager.GetToolByName(data.EquippedTool);
            obj.AddRange(collection);
            return obj;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin
{
    public static class GameStateController
    {
        public static void SetFullHP()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillHealthToMax();
        }

        public static void SetFullSilk()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillSilkToMaxSilent();

        }
        public static void RemoveCocoon()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.CocoonBroken();

        }
    }
}

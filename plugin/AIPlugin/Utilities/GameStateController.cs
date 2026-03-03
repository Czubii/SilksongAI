namespace AIPlugin
{
    public class GameStateController
    {
        public void SetFullHP()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillHealthToMax();
        }

        public void SetFullSilk()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillSilkToMaxSilent();

        }
        public void RemoveCocoon()
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

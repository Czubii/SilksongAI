namespace WeaverNet.Mod.DataCollection.Recording
{
    public class FrameObservation
    {
        public HeroObservation Hero { get; }
        public EnemyObservation Boss { get; }
        public FrameObservation(HeroObservation hero, EnemyObservation boss)
        {
            Hero = hero;
            Boss = boss;
        }
    }
}

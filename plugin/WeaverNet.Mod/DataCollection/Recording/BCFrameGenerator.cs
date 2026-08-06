using System.Linq;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins;
using WeaverNet.Core.Plugins.FrameCapture;
using WeaverNet.Core.Plugins.Recording;
using WeaverNet.Mod.DataCollection.Recording;
using WeaverNet.Mod.Game;

namespace WeaverNet.Mod.DataCollection
{
    public class BCFrameGenerator : IFrameGenerator<BCFrame>
    {
        private readonly ICombatEntityQuery _combatEntityQuerry;
        public BCFrameGenerator(ICombatEntityQuery combatEntityQuerry)
        {
            _combatEntityQuerry = combatEntityQuerry;
        }
        public bool TryGenerate(FightContext context, out BCFrame frame)
        {
            frame = null;
            if (context == null) return false;

            var boss = _combatEntityQuerry.GetEnemiesByName(context.Boss.Id).First(); // TODO: this sucks a bit

            frame = new BCFrame
                    (
                        new FrameObservation
                        (
                            new EnemyObservation(boss.HealthManager.hp)
                        )
                    );

            return true;
        }
    }
}

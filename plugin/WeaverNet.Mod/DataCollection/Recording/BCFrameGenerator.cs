using System.Collections.Generic;
using System.Linq;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins;
using WeaverNet.Core.Plugins.FrameCapture;
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
            var hero = HeroController.instance;
            var inputHandle = InputHandler.Instance;

            if (boss == null || hero == null || inputHandle == null) return false;

            var playmakers = new List<FsmObservation>();

            for (int i = 0; i < boss.PlayMakers.Length; i++)
            {
                playmakers.Add(new FsmObservation
                    (
                     boss.PlayMakers[i].FsmName,
                     boss.PlayMakers[i].ActiveStateName
                    ));
            }

            frame = new BCFrame
                    (
                        new FrameObservation
                        (
                            new HeroObservation(
                                hero.transform.position.x,
                                hero.transform.position.y,     
                                hero.playerData.health,
                                hero.playerData.silk,
                                hero.IsStunned,
                                hero.CanJump(),
                                hero.CanDoubleJump(),
                                hero.CanAttack(),
                                hero.CanSprint(),
                                hero.CanBind(),
                                hero.CanCast(),
                                hero.CanNailArt(),
                                hero.CanTryHarpoonDash(),
                                hero.CanBackDash()
                                ),
                            new EnemyObservation(
                                context.Boss.Id,
                                boss.HealthManager.hp,
                                boss.GameObject.transform.position.x,
                                boss.GameObject.transform.position.y,
                                boss.GameObject.transform.localScale.x >= 0.0 ? true : false,
                                boss.Rigidbody2D.linearVelocityX,
                                boss.Rigidbody2D.linearVelocityY,
                                playmakers)
                        ),
                        new FrameAction
                        (
                            inputHandle.inputActions.MoveVector.Vector.x,
                            inputHandle.inputActions.MoveVector.Vector.y,
                            inputHandle.inputActions.Jump,
                            inputHandle.inputActions.Attack,
                            inputHandle.inputActions.Cast,
                            inputHandle.inputActions.QuickCast,
                            inputHandle.inputActions.Dash,
                            inputHandle.inputActions.SuperDash
                        )
                    );

            return true;
        }
    }
}

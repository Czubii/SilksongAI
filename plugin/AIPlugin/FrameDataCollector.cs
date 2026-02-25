using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin
{
    public static class FrameDataCollector
    {
        public static FrameData? GetAll(EnemyInstance boss, List<EnemyInstance> enemies)
        {
            FrameEnemyData? bossData = GetEnemyData(boss.GameObject);
            if (bossData == null)
            {
                AIPlugin.Log.LogError("FrameDataCollector: bossData missing.");
                return null;
            }

            FrameHeroData? heroData = GetHeroData();
            if (heroData == null)
            {
                AIPlugin.Log.LogError("FrameDataCollector: heroData missing.");
                return null;
            }

            var IH = InputHandler.Instance;
            if (IH == null)
            {
                AIPlugin.Log.LogError("FrameDataCollector: InputHandler.Instance missing.");
                return null;
            }

            FrameUserInputs userInputs = GetInputs(IH);

            FrameData frameData = new FrameData()
            {
                Boss = (FrameEnemyData)bossData,
                Hero = (FrameHeroData)heroData,
                UserInputs = userInputs
            };

            frameData.Enemies = new FrameEnemyData[enemies.Count];
            for (int i = 0; i < enemies.Count; i++)
            {
                FrameEnemyData? enemyData = GetEnemyData(enemies[i].GameObject);
                if (enemyData == null)
                {
                    AIPlugin.Log.LogWarning("BossFightRecorder: enemyData missing");
                    continue;
                }

                frameData.Enemies[i] = (FrameEnemyData)enemyData;
            }

            return frameData;
        }
        public static FrameEnemyData? GetEnemyData(GameObject enemy)
        {
            if (enemy == null) return null;

            var hm = enemy.GetComponent<HealthManager>();
            if (hm == null) return null;

            var fsms = enemy.GetComponents<PlayMakerFSM>();
            if (fsms == null) return null;

            var rigidbody = enemy.GetComponent<Rigidbody2D>();
            if (rigidbody == null) return null;

            FrameEnemyData output = new FrameEnemyData
            {
                posX = enemy.transform.position.x,
                posY = enemy.transform.position.y,
                facing = (int)enemy.transform.GetScaleX(),
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hm.hp
            };

            output.playMakers = new PlayMaker[fsms.Length];

            for (int i = 0; i < fsms.Length; i++)
            {
                output.playMakers[i].Name = fsms[i].FsmName;
                output.playMakers[i].StateName = fsms[i].ActiveStateName;
            }

            return output;
        }

        public static FrameHeroData? GetHeroData()
        {
            var hero = HeroController.instance;
            var rigidbody = hero.GetComponent<Rigidbody2D>();

            if (rigidbody == null) return null;

            FrameHeroData output = new FrameHeroData
            {

                posX = hero.transform.position.x,
                posY = hero.transform.position.y,
                facing = (int)hero.transform.GetScaleX(),
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hero.playerData.health,
                silk = hero.playerData.silk,
                canJump = hero.CanJump(),
                canDoubleJump = hero.CanDoubleJump(),
                canAttack = hero.CanAttack(),
                canSprint = hero.CanSprint(),
                canBind = hero.CanBind(),
                canCast = hero.CanCast(),
                canNailArt = hero.CanNailArt(),
                canTryHarpoon = hero.CanTryHarpoonDash(),
                canInput = hero.CanInput(),
                canBackDash = hero.CanBackDash(),

            };

            return output;
        }

        public static FrameUserInputs GetInputs(InputHandler IH)
        {
            FrameUserInputs controls = new FrameUserInputs()
            {
                jump = IH.inputActions.Jump,
                left = IH.inputActions.Left.RawValue,
                right = IH.inputActions.Right.RawValue,
                up = IH.inputActions.Up.RawValue,
                down = IH.inputActions.Down.RawValue,

                attack = IH.inputActions.Attack,
                heal = IH.inputActions.Cast,
                skill = IH.inputActions.QuickCast,
                dash = IH.inputActions.Dash,
                harpoon = IH.inputActions.SuperDash
            };
            return controls;
        }
    }
}

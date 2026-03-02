using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{
    public static class FrameDataCollector
    {
        public static FrameData GetAll(EnemyInstance boss, List<EnemyInstance> enemies)
        {

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
                Hero = (FrameHeroData)heroData,
                UserInputs = userInputs
            };

            frameData.Enemies = new FrameEnemyData[enemies.Count + 1];

            FrameEnemyData? bossData = GetEnemyData(boss);
            if (bossData == null)
            {
                AIPlugin.Log.LogError("FrameDataCollector: bossData missing.");
                return null;
            }

            frameData.Enemies[0] = (FrameEnemyData)bossData;

            for (int i = 0; i < enemies.Count; i++)
            {
                FrameEnemyData? enemyData = GetEnemyData(enemies[i]);
                if (enemyData == null)
                {
                    AIPlugin.Log.LogWarning("BossFightRecorder: enemyData missing");
                    continue;
                }

                frameData.Enemies[i+1] = (FrameEnemyData)enemyData;
            }

            return frameData;
        }
        public static FrameEnemyData? GetEnemyData(EnemyInstance enemy)
        {
            if (enemy == null) return null;
            if (enemy.GameObject == null) return null;
            if (enemy.HealthManager == null) return null;
            if (enemy.PlayMakers == null) return null;
            if (enemy.Rigidbody2D == null) return null;

            FrameEnemyData output = new FrameEnemyData
            {
                posX = enemy.GameObject.transform.position.x,
                posY = enemy.GameObject.transform.position.y,
                facing = enemy.GameObject.transform.localScale.x >= 0.0 ? 1 : -1,
                velX = enemy.Rigidbody2D.linearVelocity.x,
                velY = enemy.Rigidbody2D.linearVelocity.y,
                hp = enemy.HealthManager.hp
            };

            output.playMakers = new PlayMakerData[enemy.PlayMakers.Length];
            for (int i = 0; i < enemy.PlayMakers.Length; i++)
            {
                output.playMakers[i] = new PlayMakerData
                {
                    Name = enemy.PlayMakers[i].FsmName,
                    StateName = enemy.PlayMakers[i].ActiveStateName
                };
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

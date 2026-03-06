using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{
    public static class FrameDataCollector
    {
        public static LivePredictionFrameData GetLive(EnemyInstance boss, List<EnemyInstance> enemies) //TODO avoid repetition with functino bellow
        {
            FrameHeroData? heroData = GetHeroData(boss);
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

            LivePredictionFrameData frameData = new LivePredictionFrameData()
            {
                Hero = (FrameHeroData)heroData,
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

                frameData.Enemies[i + 1] = (FrameEnemyData)enemyData;
            }

            return frameData;
        }
        public static RecordingFrameData GetAll(EnemyInstance boss, List<EnemyInstance> enemies)
        {

            FrameHeroData? heroData = GetHeroData(boss);
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

            RecordingFrameData frameData = new RecordingFrameData()
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
            var hero = HeroController.instance;
            if (enemy == null) return null;
            if (enemy.GameObject == null) return null;
            if (enemy.HealthManager == null) return null;
            if (enemy.PlayMakers == null) return null;
            if (enemy.Rigidbody2D == null) return null;

            FrameEnemyData output = new FrameEnemyData
            {
                posX = enemy.GameObject.transform.position.x,
                posY = enemy.GameObject.transform.position.y,
                RelPosX = enemy.GameObject.transform.position.x - hero.transform.position.x,
                RelPosY = enemy.GameObject.transform.position.y - hero.transform.position.y,
                facing = enemy.GameObject.transform.localScale.x >= 0.0 ? true : false,
                velX = enemy.Rigidbody2D.linearVelocity.x,
                velY = enemy.Rigidbody2D.linearVelocity.y,
                hp = enemy.HealthManager.hp,
                Name = enemy.Name,
                
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
        public static FrameHeroData? GetHeroData(HeroController heroInstance, EnemyInstance targetEnemy) //TODO make those take hero etc as parameters
        {
            var hero = HeroController.instance;
            var rigidbody = hero.GetComponent<Rigidbody2D>();
            if (rigidbody == null) return null;

            FrameHeroData output = new FrameHeroData
            {
                posX = hero.transform.position.x,
                posY = hero.transform.position.y,
                RelPosX = hero.transform.position.x - targetEnemy.GameObject.transform.position.x,
                RelPosY = hero.transform.position.y - targetEnemy.GameObject.transform.position.y,

                hp = hero.playerData.health,
                silk = hero.playerData.silk,
                IsStunned = hero.IsStunned,
                canJump = hero.CanJump(),
                canDoubleJump = hero.CanDoubleJump(),
                canAttack = hero.CanAttack(),
                canSprint = hero.CanSprint(),
                canBind = hero.CanBind(),
                canCast = hero.CanCast(),
                canNailArt = hero.CanNailArt(),
                canTryHarpoon = hero.CanTryHarpoonDash(),
                canBackDash = hero.CanBackDash(),
            };

            return output;
        }

        public static FrameUserInputs GetInputs(InputHandler IH)
        {
            FrameUserInputs controls = new FrameUserInputs()
            {
                jump = IH.inputActions.Jump,
                horizontal = IH.inputActions.MoveVector.Vector.x,
                vertical = IH.inputActions.MoveVector.Vector.y,

                attack = IH.inputActions.Attack,
                heal = IH.inputActions.Cast,
                skill = IH.inputActions.QuickCast,
                dash = IH.inputActions.Dash,
                harpoon = IH.inputActions.SuperDash
            };
            return controls;
        }

        public static class GetDataUtils
        {

            public static GameObject[] GetAllDamageSources()
            {
                var damageSources = GameObject.FindObjectsByType<DamageHero>(FindObjectsSortMode.None);
                GameObject[] damageSourcesGo = new GameObject[damageSources.Length];

                for (int i = 0; i < damageSources.Length; i++)
                {
                    damageSourcesGo[i] = damageSources[i].gameObject;
                }

                return damageSourcesGo;
            }

        }
    }
}

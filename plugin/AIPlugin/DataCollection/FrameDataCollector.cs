using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{
    public static class FrameDataCollector
    {
        public static LivePredictionFrameData GetLive(EnemyInstance boss, List<EnemyInstance> enemies) //TODO avoid repetition with functino bellow
        {
            HeroController hero = HeroController.instance;
            FrameHeroData heroData = GetHeroData(hero, boss);

            LivePredictionFrameData frameData = new LivePredictionFrameData()
            {
                Hero = heroData,
            };

            frameData.Enemies = new FrameEnemyData[enemies.Count + 1];
            frameData.Enemies[0] = GetEnemyData(boss, hero);

            for (int i = 0; i < enemies.Count; i++)
            {
                frameData.Enemies[i + 1] = GetEnemyData(enemies[i], hero);
            }

            return frameData;
        }
        public static RecordingFrameData GetRecording(EnemyInstance boss, List<EnemyInstance> enemies)
        {
            HeroController hero = HeroController.instance;
            FrameHeroData heroData = GetHeroData(hero, boss);
            FrameUserInputs userInputs = GetInputs();

            RecordingFrameData frameData = new RecordingFrameData()
            {
                Hero = heroData,
                UserInputs = userInputs
            };

            frameData.Enemies = new FrameEnemyData[enemies.Count + 1];
            frameData.Enemies[0] = GetEnemyData(boss, hero);

            for (int i = 0; i < enemies.Count; i++)
            {
                frameData.Enemies[i+1] = GetEnemyData(enemies[i], hero);
            }

            return frameData;
        }
        public static FrameEnemyData GetEnemyData(EnemyInstance enemy, HeroController hero)
        {
            if (enemy == null || enemy.GameObject == null || enemy.PlayMakers == null || enemy.HealthManager == null
                || enemy.Rigidbody2D == null)
                throw new ArgumentNullException($"GetEnemyData: One or more component(s) of enemy: {enemy.Name} is null");

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
        public static FrameHeroData GetHeroData(HeroController hero, EnemyInstance targetEnemy) //TODO make those take hero etc as parameters
        {
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

        public static FrameUserInputs GetInputs()
        {
            var IH = InputHandler.Instance; // This one is always in game so it should not cause any trouble accessing it this way
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

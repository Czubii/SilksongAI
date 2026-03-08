using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace AIPlugin
{
    public static class FrameDataCollector
    {
        public static RecordingFrame GetRecording(EnemyInstance boss, List<EnemyInstance> enemies)
        {
            HeroController hero = HeroController.instance;
            FrameHeroData heroData = GetHeroData(hero, boss);
            FrameUserInputs userInputs = GetInputs();

            FrameData frameData = new FrameData()
            {
                Hero = heroData,
            };

            frameData.Enemies = new FrameEnemyData[enemies.Count + 1];
            frameData.Enemies[0] = GetEnemyData(boss, hero);

            for (int i = 0; i < enemies.Count; i++)
            {
                frameData.Enemies[i+1] = GetEnemyData(enemies[i], hero);
            }

            return new RecordingFrame()
            {
                Data = frameData,
                UserInputs = userInputs
            };
        }
        public static FrameEnemyData GetEnemyData(EnemyInstance enemy, HeroController hero)
        {
            if (enemy == null || enemy.GameObject == null || enemy.PlayMakers == null || enemy.HealthManager == null
                || enemy.Rigidbody2D == null)
                throw new ArgumentNullException($"GetEnemyData: One or more component(s) of enemy: {enemy.Name} is null");

            FrameEnemyData output = new FrameEnemyData
            {
                PosX = enemy.GameObject.transform.position.x,
                PosY = enemy.GameObject.transform.position.y,
                RelPosX = enemy.GameObject.transform.position.x - hero.transform.position.x,
                RelPosY = enemy.GameObject.transform.position.y - hero.transform.position.y,
                Facing = enemy.GameObject.transform.localScale.x >= 0.0 ? true : false,
                VelX = enemy.Rigidbody2D.linearVelocity.x,
                VelY = enemy.Rigidbody2D.linearVelocity.y,
                HP = enemy.HealthManager.hp,
            };
            output.PlayMakers = new NamedStatesContainer(); 
            output.PlayMakers.ParentName = enemy.Name;
            output.PlayMakers.NamedStates = new NamedState[enemy.PlayMakers.Length];

            for (int i = 0; i < enemy.PlayMakers.Length; i++)
            {
                output.PlayMakers.NamedStates[i] = new NamedState
                {
                    Name = enemy.PlayMakers[i].FsmName,
                    StateName = enemy.PlayMakers[i].ActiveStateName
                };
            }

            return output;
        }
        public static FrameHeroData GetHeroData(HeroController hero, EnemyInstance targetEnemy)
        {
            FrameHeroData output = new FrameHeroData
            {
                PosX = hero.transform.position.x,
                PosY = hero.transform.position.y,
                RelPosX = hero.transform.position.x - targetEnemy.GameObject.transform.position.x,
                RelPosY = hero.transform.position.y - targetEnemy.GameObject.transform.position.y,

                HP = hero.playerData.health,
                Silk = hero.playerData.silk,
                IsStunned = hero.IsStunned,
                Facing = hero.transform.localScale.x >= 0.0 ? true : false,
                CanJump = hero.CanJump(),
                CanDoubleJump = hero.CanDoubleJump(),
                CanAttack = hero.CanAttack(),
                CanSprint = hero.CanSprint(),
                CanBind = hero.CanBind(),
                CanCast = hero.CanCast(),
                CanNailArt = hero.CanNailArt(),
                CanTryHarpoon = hero.CanTryHarpoonDash(),
                CanBackDash = hero.CanBackDash(),
            };

            return output;
        }

        public static FrameUserInputs GetInputs()
        {
            var IH = InputHandler.Instance; // This one is always in game so it should not cause any trouble accessing it this way
            FrameUserInputs controls = new FrameUserInputs()
            {
                Jump = IH.inputActions.Jump,
                Horizontal = IH.inputActions.MoveVector.Vector.x,
                Vertical = IH.inputActions.MoveVector.Vector.y,

                Attack = IH.inputActions.Attack,
                Heal = IH.inputActions.Cast,
                Skill = IH.inputActions.QuickCast,
                Dash = IH.inputActions.Dash,
                Harpoon = IH.inputActions.SuperDash
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

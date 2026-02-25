using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;
using Steamworks;
using TeamCherry.SharedUtils;
using static DamageReference;
using static GameManager;
using InControl;

namespace AIPlugin
{
    //TODO get rid of all of this by moving to some better places
   

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

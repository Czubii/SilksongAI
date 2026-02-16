using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace SilksongAI
{
    public static class TeleportUtils
    {

        private static Vector3 pendingPos;
        private static string pendingScene;
        private static bool pendingTeleport;
        public static void TeleportTo(string sceneName, Vector3 pos)
        {
            pendingScene = sceneName;
            pendingPos = pos;
            pendingTeleport = true;

            GameManager.instance.ChangeToScene(sceneName, "top1", 0.5f);
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!pendingTeleport) return;
            if (scene.name != pendingScene) return;

            pendingTeleport = false;

            HeroController.instance.transform.position = pendingPos;
            GameManager.instance.cameraCtrl.PositionToHeroInstant(false);
        }
    }
}

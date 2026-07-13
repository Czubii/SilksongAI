using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using static GameManager;

namespace WeaverNet.Mod.Game
{
    public class TeleportService : MonoBehaviour, ITeleportService
    {
        public void Teleport(BossDefinition boss, bool requireSceneReload)
        {
            Teleport(boss.ArenaSceneName, boss.ArenaPosition, requireSceneReload);
        }
        public void Teleport(string targetSceneName, Vector3 targetPos, bool requireSceneReload)
        {
            if (!TryStartTeleport()) return;

            PluginRuntime.State.TeleportInProgress = true;
            StartCoroutine(TeleportRutine(targetSceneName, targetPos, requireSceneReload));
        }
        public void TeleportToBench()
        {
            if (!TryStartTeleport()) return;

            PluginRuntime.State.TeleportInProgress = true;
            StartCoroutine(TeleportToBenchRutine());
        }
        public bool CanTeleport()
        {
            if (PluginRuntime.State.TeleportInProgress)
                return false;

            if (PlayerData.instance != null && PlayerData.instance.health <= 0)
                return false;

            if (PlayerData.instance != null && PlayerData.instance.atBench)
                return false;

            if (PlayerData.instance != null && !PlayerData.instance.bindCutscenePlayed)
                return false;

            if (GameManager.instance != null && GameManager.instance.RespawningHero)
                return false;

            return true;
        }
        private IEnumerator TeleportRutine(string targetSceneName, Vector3 targetPos, bool requireSceneReload)
        {
            try
            {
                string currentSceneName = SceneManager.GetActiveScene().name;

                if (currentSceneName != targetSceneName || requireSceneReload) // if we are not in the same scene or we do require reload
                {
                    GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo // change the scene
                    {
                        SceneName = targetSceneName,
                        EntryGateName = "left1",
                        EntrySkip = CanSkipEntry(),
                        HeroLeaveDirection = GlobalEnums.GatePosition.unknown,
                        EntryDelay = 0f,
                        Visualization = GameManager.SceneLoadVisualizations.Default,
                        AlwaysUnloadUnusedAssets = true,
                        WaitForSceneTransitionCameraFade = true,
                    });
                    yield return null; // Wait one frame so GameManager sets transition flags internally
                }

                yield return AwaitTransitionFinished(); // Make sure we are ready to teleport hero

                TeleportHero(targetPos);

                var gm = GameManager.instance;

                if (gm == null) yield break;

                for (int i = 0; i < 10; i++) // this delay is needed as in some larger rooms the camera would not snap to player if there was no delay 
                {
                    yield return null;
                }
                gm.cameraCtrl.PositionToHeroInstant(true);

            }
            finally { PluginRuntime.State.TeleportInProgress = false; }
        }
        private IEnumerator TeleportToBenchRutine()
        {
            try
            {
                var gm = GameManager.instance;

                gm.RespawningHero = true;
                GetRespawnInfo(out var scene, out var marker);

                gm.BeginSceneTransition(new SceneLoadInfo
                {
                    SceneName = scene,
                    EntryGateName = marker,
                    EntrySkip = CanSkipEntry(),
                    HeroLeaveDirection = GlobalEnums.GatePosition.unknown,
                    EntryDelay = 0f,
                    Visualization = SceneLoadVisualizations.Default,
                    AlwaysUnloadUnusedAssets = true,
                    WaitForSceneTransitionCameraFade = true,
                });

                yield return AwaitTransitionFinished();
            }
            finally { PluginRuntime.State.TeleportInProgress = false; }
        }
        private void TeleportHero(Vector3 pos)
        {
            var hc = HeroController.instance;
            if (hc == null)
            {
                PluginLog.Error("TeleportHero(): Cannot teleport, no HeroController.instance on scene");
                return;
            }
            hc.transform.position = pos;
            var HeroRigidbody2D = HeroController.instance.GetComponent<Rigidbody2D>();
            if (HeroRigidbody2D != null)
            {
                HeroRigidbody2D.linearVelocity = Vector2.zero;
            }
            if (HeroController.instance.cState != null)
            {
                HeroController.instance.cState.recoiling = false;
                HeroController.instance.cState.transitioning = false;
            }
        }
        private bool TryStartTeleport()
        {
            if (PluginRuntime.State.TeleportInProgress)
            {
                PluginLog.Error("Cannot teleport, the old one is still in progress");
                return false;
            }
            if (!CanTeleport())
            {
                PluginLog.Error("Cannot teleport.");
                return false;
            }
            return true;
        }
        private IEnumerator AwaitTransitionFinished()
        {
            yield return new WaitWhile(() =>
            {

                var gm = GameManager.instance;
                var hc = HeroController.instance;

                if (gm == null || hc == null) return true;

                return !hc.isHeroInPosition || hc.cState.transitioning || gm.IsInSceneTransition;

            });
        }
        private bool CanSkipEntry()
        {
            HeroController hc = HeroController.instance;
            if (hc == null) return false;

            if (hc.cState.dashing || hc.cState.isSprinting || hc.sprintFSM.GetFsmBoolIfExists("Is Sprinting"))
                return false;

            return true;
        }
        private void GetRespawnInfo(out string scene, out string marker)
        {
            var playerData = PlayerData.instance;
            string savedRespawnScene;
            string savedRespawnMarker;
            if (!string.IsNullOrEmpty(playerData.tempRespawnScene))
            {
                savedRespawnScene = playerData.tempRespawnScene;
                savedRespawnMarker = playerData.tempRespawnMarker;
            }
            else
            {
                savedRespawnScene = playerData.respawnScene;
                savedRespawnMarker = playerData.respawnMarkerName;
            }

            Dictionary<string, SceneTeleportMap.SceneInfo> teleportMap = SceneTeleportMap.GetTeleportMap();
            if (Application.isEditor)
            {
                scene = savedRespawnScene;
                marker = savedRespawnMarker;
                if (teleportMap.TryGetValue(savedRespawnScene, out var value) && !value.RespawnPoints.Contains(savedRespawnMarker))
                {
                    teleportMap.Where((KeyValuePair<string, SceneTeleportMap.SceneInfo> kvp) => kvp.Key.StartsWith(savedRespawnScene)).Any((KeyValuePair<string, SceneTeleportMap.SceneInfo> kvp) => kvp.Value.RespawnPoints.Contains(savedRespawnMarker));
                }

                return;
            }

            if (teleportMap.TryGetValue(savedRespawnScene, out var value2))
            {
                if (value2.RespawnPoints.Contains(savedRespawnMarker))
                {
                    scene = savedRespawnScene;
                    marker = savedRespawnMarker;
                    return;
                }

                if (teleportMap.Where((KeyValuePair<string, SceneTeleportMap.SceneInfo> kvp) => kvp.Key.StartsWith(savedRespawnScene)).Any((KeyValuePair<string, SceneTeleportMap.SceneInfo> kvp) => kvp.Value.RespawnPoints.Contains(savedRespawnMarker)))
                {
                    scene = savedRespawnScene;
                    marker = savedRespawnMarker;
                    return;
                }
            }

            scene = "Tut_01";
            marker = "Death Respawn Marker Init";
            playerData.ResetTempRespawn();
        }
    }
}

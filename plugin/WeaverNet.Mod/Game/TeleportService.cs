using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using static DamageReference;
using static GameManager;

namespace WeaverNet.Mod.Game
{
    public class TeleportService : ITeleportService
    {
        private Task _runningTask;
        private readonly object _startLock = new object();
        public Task TeleportAsync(string targetSceneName, Vector3 targetPos, bool requireSceneReload) // no cancellation token as those cannot realistically be cancelled
        {
            lock (_startLock)
            {
                EnsureCanTeleport();
                _runningTask = ExecuteTeleportAsync(targetSceneName, targetPos, requireSceneReload);
            }
            return _runningTask;
        }
        public Task TeleportToBenchAsync() // no cancellation token as those cannot realistically be cancelled
        {
            lock (_startLock)
            {
                EnsureCanTeleport();
                _runningTask = ExecuteTeleportToBenchAsync();
            }
            return _runningTask;
        }
        public void Teleport(string targetSceneName, Vector3 targetPos, bool requireSceneReload) => _ = TeleportAsync(targetSceneName, targetPos, requireSceneReload);
        public void TeleportToBench() => _ = TeleportToBenchAsync();
        public bool CanTeleport()
        {
            try
            {
                EnsureCanTeleport();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool TeleportInProgress() => _runningTask != null && !_runningTask.IsCompleted;
        private void EnsureCanTeleport()
        {
            if (TeleportInProgress())
                throw new InvalidOperationException("A teleport is already in progress.");

            var playerData = PlayerData.instance;
            if (playerData != null)
            {
                if (playerData.health <= 0)
                    throw new InvalidOperationException("Cannot teleport while dead.");

                if (playerData.atBench)
                    throw new InvalidOperationException("Cannot teleport while resting at a bench.");

                if (!playerData.bindCutscenePlayed)
                    throw new InvalidOperationException("Cannot teleport before the bind cutscene has played.");
            }

            var gameManager = GameManager.instance;
            if (gameManager != null && gameManager.RespawningHero)
                throw new InvalidOperationException("Cannot teleport while the hero is respawning.");
        }
        private async Task ExecuteTeleportAsync(string targetSceneName, Vector3 targetPos, bool requireSceneReload)
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
                    await Task.Yield(); // Wait one frame so GameManager sets transition flags internally
                }

                await AwaitTransitionFinished(); // Make sure we are ready to teleport hero

                TeleportHero(targetPos);

                var gm = GameManager.instance;

                if (gm == null) return;

                for (int i = 0; i < 10; i++) // this delay is needed as in some larger rooms the camera would not snap to player if there was no delay 
                {
                    await Task.Yield();
                }
                gm.cameraCtrl.PositionToHeroInstant(true);

            }
            finally { PluginRuntime.State.TeleportInProgress = false; }
        }
        private async Task ExecuteTeleportToBenchAsync()
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

            await AwaitTransitionFinished();
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
        private async Task AwaitTransitionFinished()
        {
            while (true)
            {
                var gm = GameManager.instance;
                var hc = HeroController.instance;

                if (gm != null &&
                    hc != null &&
                    hc.isHeroInPosition &&
                    !hc.cState.transitioning &&
                    !gm.IsInSceneTransition)
                {
                    return;
                }

                await Task.Yield();
            }
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

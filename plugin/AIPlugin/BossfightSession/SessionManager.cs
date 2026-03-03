using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using static AIPlugin.BossfightSession.SessionManager;

namespace AIPlugin.BossfightSession
{
    public interface ISessionListener
    {
        void OnFightStarted(SessionEnemyManager enemyManager);
        void OnFightFinished(FightResults results, bool forced);
    }
    public class SessionManager : MonoBehaviour
    {   
        public enum SessionState
        {
            Idle,
            StartingNewFight,
            AwaitingBoss,
            Fighting,
            Stopping
        }

        public class SessionSettings // TODO connect somehow with FightInfo
        {
            public bool keepAbilities;
            public bool keepTools;

            public SessionSettings()
            {
                keepAbilities = false;
                keepTools = false;
            }
        }
        private TeleportService _teleportService;
        public BossMetadata TargetBossMetadata { get; private set; }
        public int RemainingFights { get; private set; } = 0;
        public int TotalFights { get; private set; } = 0;

        private CustomRespawnPoint _spawnPoint;
        public SessionState State { get; private set; } = SessionState.Idle;

        private bool _bossFound;

        private bool _heroDied;

        private bool _forceStopFlag;
        private bool ForceStop() => _forceStopFlag;
        public readonly struct FightResults
        {
            public readonly bool Success;
            public FightResults(bool success)
            {
                Success = success;
            }
        }

        private SessionEnemyManager _enemyManager = new SessionEnemyManager();

        private readonly List<ISessionListener> _listeners = new List<ISessionListener>();

        public void Initialize(TeleportService teleport, List<ISessionListener> listeners)
        {
            _listeners.AddRange(listeners);
            _teleportService = teleport;
        }
        private void RaiseFightStarted()
        {
            foreach (var r in _listeners) r.OnFightStarted(_enemyManager);
        }
        private void RaiseFightFinished(FightResults results)
        {
            foreach (var r in _listeners) r.OnFightFinished(results, ForceStop());
        }
        public void StartSession(BossMetadata boss, int numFights, SessionSettings settings = null)
        {
            if (State != SessionState.Idle)
            {
                AIPlugin.Log.LogWarning("Cannot Start Recording Session! One is still running");
                return;
            }
            if (numFights <= 0)
            {
                AIPlugin.Log.LogError("BossFightSession.StartSession(): numFights has to be positive");
                return;
            }

            State = SessionState.StartingNewFight;

            RemainingFights = numFights;
            TotalFights = numFights;
            TargetBossMetadata = boss;
            
            _forceStopFlag = false;

            if (settings == null)
                settings = new SessionSettings();

            //CacheOldPlayerData(); // TODO

            if (!settings.keepAbilities) TargetBossMetadata.SetExpectedPlayerAbilities();
            //if (!settings.keepTools) TargetBoss.SetExpectedPlayerTools(); //TODO

            _spawnPoint = new CustomRespawnPoint("BossFightSessionRespawn" + TargetBossMetadata.InternalName, 
                TargetBossMetadata.ArenaSceneName, 
                TargetBossMetadata.ArenaPosition);

            _enemyManager.SetTargetEnemy(TargetBossMetadata);

            AIPlugin.Log.LogDebug($"BossFightSession: Starting new bossfight session, target: {TargetBossMetadata.DisplayName}");
            StartCoroutine(SessionLoop());
        }
        public void StopSession()
        {
            if (State != SessionState.Idle)
            {
                State = SessionState.Stopping;
                _forceStopFlag = true;
            }
        }
        private IEnumerator SessionLoop()
        {
            GameStateController.RemoveCocoon();
            _heroDied = false;

            while (RemainingFights > 0 && !ForceStop())
            {
                _spawnPoint.UseAsTemporary(0); // refresh spawnpoint
                RemainingFights--;
                
                State = SessionState.StartingNewFight;
                AIPlugin.Log.LogDebug($"BossFightSession: Starting new fight");
                yield return PrepareHero();
                if (ForceStop()) break;

                State = SessionState.AwaitingBoss;
                AIPlugin.Log.LogDebug($"BossFightSession: Awaiting boss");
                _bossFound = false;
                yield return _enemyManager.AwaitTargetOnScene((r) => _bossFound = r, ForceStop);
                if (ForceStop()) break;

                if (_bossFound)
                {
                    State = SessionState.Fighting;
                    AIPlugin.Log.LogDebug($"BossFightSession: Fighting");
                }
                else
                {
                    AIPlugin.Log.LogDebug($"BossFightSession: Boss not found. Retrying");
                    continue;
                }

                _heroDied = false;
                RaiseFightStarted();
                yield return WaitForAttemptFinished();
                RaiseFightFinished(new FightResults(!_heroDied));

                if(_heroDied) StartCoroutine(AwaitCocoonAndRemove());

                AIPlugin.Log.LogDebug($"BossFightSession: Attempt Ended, hero died: {_heroDied}");

            }
            State = SessionState.Stopping;
            AIPlugin.Log.LogDebug($"BossFightSession: Finalizing Session");
            yield return FinalizeSession();
            State = SessionState.Idle;
        }
        private IEnumerator FinalizeSession()
        {
            if (_heroDied)
            {
                _spawnPoint?.Dispose();
                _spawnPoint = null;
                CustomRespawnPoint.ResetTemporary();
                yield break;
            }
            else
            {
                yield return _teleportService.AwaitCanTeleport();
                _spawnPoint?.Dispose();
                _spawnPoint = null;
                CustomRespawnPoint.ResetTemporary();
                _teleportService.TeleportToBench();
            }
        }
        private IEnumerator PrepareHero()
        {
            var ts = _teleportService;

            yield return ts.AwaitCanTeleport(() => ForceStop());

            if (ForceStop()) yield break;

            if (!_heroDied)
            {
                GameStateController.SetFullHP();
                GameStateController.SetFullSilk();

                if (!TargetBossMetadata.RequireHardSceneReload)
                {
                    TargetBossMetadata.SetDefeated(false);

                    ts.TeleportTo(TargetBossMetadata, true);
                    yield return ts.AwaitCanTeleport();
                }
                else
                {
                    ts.TeleportTo("Tut_01", Vector3.zero, true); // any room different than the bossfight would do
                    yield return ts.AwaitCanTeleport();

                    TargetBossMetadata.SetDefeated(false);
                    ts.TeleportTo(TargetBossMetadata, true);
                    yield return ts.AwaitCanTeleport();
                }
            }
            else
            {
                GameStateController.SetFullHP();
                GameStateController.SetFullSilk();

                yield return new WaitUntil(() => // wait untill can input or stop flag
                {
                    if (ForceStop()) return true;

                    if (HeroController.instance == null)
                        return false;

                    return HeroController.instance.acceptingInput;
                });
            }

            GameCameras.instance.HUDIn(); // turn on HUD in case some boss disables it after death (for example widow does that)
        }
        private IEnumerator WaitForAttemptFinished()
        {
            var boss = _enemyManager.GetTargetInstance();
            var hm = boss.HealthManager;
            var pd = PlayerData.instance;

            yield return new WaitUntil(() =>
            {
                if (boss == null || hm == null)
                {
                    _heroDied = false;
                    return true;
                }
                if (hm.isDead)
                {
                    _heroDied = false;
                    return true;
                }
                if (pd.health <= 0)
                {
                    _heroDied = true;
                    return true;
                }
                if (ForceStop())
                {
                    return true;
                }
                return false;
            });
        }
        private static IEnumerator AwaitCocoonAndRemove() // TODO add some timeout //TODO move ot PlayerUtils or something better
        {
            yield return new WaitUntil(() =>
            {
                var pd = PlayerData.instance;

                if (pd == null) return false;

                return pd.HeroCorpseMarkerGuid != null;
            });
            AIPlugin.Log.LogDebug("REMOVING COCOON");
            GameStateController.RemoveCocoon();
        }
    }
}

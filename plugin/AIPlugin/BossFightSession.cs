using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin
{
    public class BossFightSession : MonoBehaviour
    {   
        public static BossFightSession instance;

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

        public BossMetaData TargetBoss { get; private set; }

        public int RemainingFights { get; private set; } = 0;

        public int TotalFights { get; private set; } = 0;

        public int AwaitBossTimeoutFrames = 2500;

        private CustomRespawnPoint _spawnPoint;

        public SessionState State { get; private set; } = SessionState.Idle;

        private bool _bossFound;

        private bool _heroDied;

        private bool _forceStopFlag;
        public readonly struct SessionInfo
        {
            public readonly BossMetaData TargetBoss;
            public readonly int TotalFights;

            public SessionInfo(BossMetaData boss, int totalFights)
            {
                TargetBoss = boss;
                TotalFights = totalFights;
            }
        }
        public readonly struct FightResults
        {
            public readonly bool Success;
            public FightResults(bool success)
            {
                Success = success;
            }
        }
        public static event Action<SessionInfo> OnSessionStarted;
        public static event Action<bool> OnSessionStopped; // <bool> - was the session stopped forcibly (true - yes, false - no)
        public static event Action OnFightStarted;
        public static event Action<FightResults> OnFightFinished;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public static void EnsureExists()
        {
            if (instance != null) return;

            var go = new GameObject("BossFightSession");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BossFightSession>();
        }
        public void StartSession(BossMetaData boss, int numFights, SessionSettings settings = null)
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

            OnSessionStarted?.Invoke(new SessionInfo(boss, numFights));

            RemainingFights = numFights;
            TotalFights = numFights;
            TargetBoss = boss;
            
            _forceStopFlag = false;

            if (settings == null)
                settings = new SessionSettings();

            //CacheOldPlayerData(); // TODO

            if (!settings.keepAbilities) TargetBoss.SetExpectedPlayerAbilities();
            //if (!settings.keepTools) TargetBoss.SetExpectedPlayerTools(); //TODO

            _spawnPoint = new CustomRespawnPoint("BossFightSessionRespawn" + TargetBoss.InternalName, 
                TargetBoss.ArenaSceneName, 
                TargetBoss.ArenaPosition);

            AIPlugin.Log.LogDebug($"BossFightSession: Starting new bossfight session, target: {TargetBoss.DisplayName}");
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
            PlayerUtils.RemoveCocoon();
            _heroDied = false;

            while (RemainingFights > 0 && !IsStopping())
            {
                _spawnPoint.UseAsTemporary(0); // refresh spawnpoint
                RemainingFights--;
                
                State = SessionState.StartingNewFight;
                AIPlugin.Log.LogDebug($"BossFightSession: Starting new fight");
                yield return PrepareHero();
                _heroDied = false;
                if (IsStopping()) break;

                State = SessionState.AwaitingBoss;
                AIPlugin.Log.LogDebug($"BossFightSession: Awaiting boss");
                _bossFound = false;
                yield return AwaitBoss();
                if (IsStopping()) break;

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

                OnFightStarted?.Invoke();
                yield return WaitForAttemptFinished();
                OnFightFinished?.Invoke(new FightResults(!_heroDied));
                if(_heroDied) StartCoroutine(AwaitCocoonAndRemove());

                AIPlugin.Log.LogDebug($"BossFightSession: Attempt Ended, hero died: {_heroDied}");

            }
            State = SessionState.Stopping;
            AIPlugin.Log.LogDebug($"BossFightSession: Finalizing Session");
            yield return FinalizeSession();
            State = SessionState.Idle;
            OnSessionStopped?.Invoke(_forceStopFlag);
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
                yield return TeleportService.instance.AwaitCanTeleport();
                _spawnPoint?.Dispose();
                _spawnPoint = null;
                CustomRespawnPoint.ResetTemporary();
                TeleportService.instance.TeleportToBench();
            }
        }
        private IEnumerator PrepareHero()
        {
            var ts = TeleportService.instance;

            yield return ts.AwaitCanTeleport(() => IsStopping());

            if (IsStopping()) yield break;

            if (!_heroDied)
            {
                PlayerUtils.SetFullHP();
                PlayerUtils.SetFullSilk();

                if (!TargetBoss.RequireHardSceneReload)
                {
                    TargetBoss.SetDefeated(false);

                    ts.TeleportTo(TargetBoss, true);
                    yield return ts.AwaitCanTeleport();
                }
                else
                {
                    ts.TeleportTo("Tut_01", Vector3.zero, true); // any room different than the bossfight would do
                    yield return ts.AwaitCanTeleport();

                    TargetBoss.SetDefeated(false);
                    ts.TeleportTo(TargetBoss, true);
                    yield return ts.AwaitCanTeleport();
                }
            }
            else
            {
                PlayerUtils.SetFullHP();
                PlayerUtils.SetFullSilk();

                yield return new WaitUntil(() => // wait untill can input or stop flag
                {
                    if (IsStopping()) return true;

                    if (HeroController.instance == null)
                        return false;

                    return HeroController.instance.acceptingInput;
                });
            }

            GameCameras.instance.HUDIn(); // turn on HUD in case some boss disables it after death (for example widow does that)
        }

        private IEnumerator AwaitBoss()
        {
            int i = 0;
            yield return new WaitUntil(() =>
            {
                if (IsStopping())
                {
                    _bossFound = false;
                    return true;
                }
                if (i >= AwaitBossTimeoutFrames)
                {
                    AIPlugin.Log.LogWarning($"AwaitBossAndStartRecording(): Timeout hit when awaiting {TargetBoss.InternalName} " +
                        $"(timeout frames setting: {AwaitBossTimeoutFrames} ");
                    _bossFound = false;
                    return true;
                }
                var boss = EnemyTracker
                    .GetAll()
                    .FirstOrDefault(e => e.Name == TargetBoss.InternalName);
                
                if (boss != null)
                {
                    _bossFound = true;
                    return true;
                }
                
                i++;
                return false;
            });
        }
        private IEnumerator WaitForAttemptFinished()
        {
            var boss = EnemyTracker
                    .GetAll()
                    .FirstOrDefault(e => e.Name == TargetBoss.InternalName);
            var hm = boss.GameObject.GetComponent<HealthManager>();
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
                if (IsStopping())
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
            PlayerUtils.RemoveCocoon();
        }
        private bool IsStopping()
        {
            return (_forceStopFlag);
        }
    }
}

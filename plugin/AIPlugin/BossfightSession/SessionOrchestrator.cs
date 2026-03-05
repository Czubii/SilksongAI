using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AIPlugin.BossfightSession.BossfightRecorder;

namespace AIPlugin.BossfightSession
{
    public class SessionOrchestrator : MonoBehaviour
    {
        public enum SessionState
        {
            Idle,
            StartingNewFight,
            AwaitingBoss,
            Fighting,
            FinalizingSession
        }

        private SessionContext _sessionContext;
        private SessionRuntime _sessionRuntime;

        private SessionEvents _events;
        private GameStateController _gameStateController;
        private TeleportService _teleport;
        private SessionEnemyManager _enemyManager;

        private CancellationHandle _cancel;
        public SessionState State { get; private set; } = SessionState.Idle;

        public void Initialize(SessionEvents events, GameStateController gameStateController, TeleportService teleport, SessionEnemyManager enemyManager)
        {
            _events = events;
            _gameStateController = gameStateController;
            _teleport = teleport;
            _enemyManager = enemyManager;
        }
        public bool TryStart(SessionContext sessionContext)
        {
            if (State != SessionState.Idle) return false;

            _sessionContext = sessionContext;
            _sessionRuntime = SessionRuntime.Start(_sessionContext);
            _enemyManager.SetTargetEnemy(_sessionContext.Boss);
            _cancel = new CancellationHandle();
            _sessionContext.Boss.SetExpectedPlayerAbilities();//TODO MOVE SOMWHERE ELSE AND MAKE COMPATIBLE WITH CONFIG
            StartCoroutine(SessionLoop());
            return true;
        }
        
        public void RequestStop(string reason = "user-stop") => _cancel.RequestStop(reason);

        private void OnDisable()
        {
            if (State != SessionState.Idle)
            {
                RequestStop("component-disabled");
            }
            _sessionRuntime.Dispose();
        }

        private IEnumerator SessionLoop()
        {
            try
            {
                _gameStateController.RemoveCocoon();

                while (_sessionRuntime.RemainingFights > 0 && !_cancel.IsRequested)
                {
                    _sessionRuntime.RespawnPoint.UseAsTemporary(0);

                    Transition(SessionState.StartingNewFight, "Starting new fight");
                    yield return PrepareHero();
                    if (_cancel.IsRequested) break;

                    Transition(SessionState.AwaitingBoss, "Awaiting boss on scene");
                    var targetFound = false;
                    yield return _enemyManager.AwaitTargetOnScene((r) => targetFound = r, () => _cancel.IsRequested);
                    if (_cancel.IsRequested) break;
                    if (!targetFound)
                    {
                        _sessionRuntime.LastAttempt = AttemptResult.BossMissing;
                        continue;
                    }

                    _sessionRuntime.StartAttempt();

                    Transition(SessionState.Fighting, "Fight has begun");
                    NotifyStarted();
                    yield return WaitForAttemptFinished();
                    NotifyFinished(_sessionRuntime.LastAttempt);

                    if (_sessionRuntime.LastAttempt == AttemptResult.HeroDied)
                        yield return AwaitCocoonAndRemove();
                }

                Transition(SessionState.FinalizingSession, "Finalizing session");
                yield return FinalizeSession();
                Transition(SessionState.Idle, "Session Concluded");
            }
            finally
            {
                _sessionRuntime.Dispose();
            }
        }

        private void Transition(SessionState next, string reason)
        {
            AIPlugin.Log.LogDebug($"Session transition: {State} -> {next} ({reason})");
            State = next;
        }
        private void NotifyStarted() => _events?.RaiseStarted();
        private void NotifyFinished(AttemptResult result) => _events?.RaiseFinished(result);

        private IEnumerator PrepareHero()
        {
            yield return _teleport.AwaitCanTeleport(() => _cancel.IsRequested);

            if (_cancel.IsRequested) yield break;

            _gameStateController.SetFullHP();
            _gameStateController.SetFullSilk();

            if (_sessionRuntime.LastAttempt == AttemptResult.HeroDied)
            {
                yield return new WaitUntil(() => // wait untill can input or stop flag
                {
                    if (_cancel.IsRequested) return true;

                    if (HeroController.instance == null)
                        return false;

                    return HeroController.instance.acceptingInput;
                });
            }
            else
            {
                if (_sessionContext.Boss.RequireHardSceneReload)
                {
                    _teleport.TeleportTo("Tut_01", Vector3.zero, true); // any room different than the bossfight would do
                    yield return _teleport.AwaitCanTeleport();
                }

                _sessionContext.Boss.SetDefeated(false);
                _teleport.TeleportTo(_sessionContext.Boss, true);
                yield return _teleport.AwaitCanTeleport();

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
                    _sessionRuntime.LastAttempt = AttemptResult.BossMissing;
                    return true;
                }
                if (hm.isDead)
                {
                    _sessionRuntime.LastAttempt = AttemptResult.Success;
                    return true;
                }
                if (pd.health <= 0)
                {
                    _sessionRuntime.LastAttempt = AttemptResult.HeroDied;
                    return true;
                }
                if (_cancel.IsRequested)
                {
                    _sessionRuntime.LastAttempt = AttemptResult.ForcedStop;
                    return true;
                }
                return false;
            });
        }
        private IEnumerator AwaitCocoonAndRemove() // TODO add some timeout //TODO move ot PlayerUtils or something better
        {
            yield return new WaitUntil(() =>
            {
                var pd = PlayerData.instance;

                if (pd == null) return false;

                return pd.HeroCorpseMarkerGuid != null;
            });

            _gameStateController.RemoveCocoon();
            AIPlugin.Log.LogDebug("Cocoon Removed");
        }
        private IEnumerator FinalizeSession()
        {
            yield return _teleport.AwaitCanTeleport();
            
            _sessionRuntime.Dispose();
            _teleport.TeleportToBench();
            
        }
        public BossMetadata GetTarget() => _sessionContext.Boss;
        public int GetCurrentFightIdx() => _sessionRuntime.AttemptIndex;
        public int GetTotalFightCount() => _sessionRuntime.TotalFights;

    }
}

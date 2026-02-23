using BepInEx;
using MessagePack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Steamworks;
using System.Xml.Linq;
using UnityEngine.Playables;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using TeamCherry.SharedUtils;
using MessagePack.Resolvers;

namespace SilksongAI
{
    public static class BossFightRecordingSession
    {
        public static int RemainingFights { get; private set; } = 0;
        public static int TotalFights { get; private set; } = 0;
        public static bool SessionActive { get; private set; } = false;

        private static bool _attemptStarting = false;
        private static bool _stopCorutinesSafe = false;

        private static bool _heroDied;

        public static BossMetaData TargetBoss { get; private set; }

        private static MonoBehaviour _runner;

        private static CustomRespawnPoint _spawnPoint;

        
        

        private static void EnsureRunner()
        {
            if (_runner != null) return;

            var go = new GameObject("BossFightRecordingSessionRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<CoroutineRunner>();
        }

        private class CoroutineRunner : MonoBehaviour { }

        public static void StartSession(BossMetaData boss, int numFights)
        {
            if (SessionActive)
            {
                SilksongAImod.Log.LogWarning("Cannot Start Recording Session! One is still running");
                return;
            }
            if (numFights <= 0)
            {
                SilksongAImod.Log.LogError("BossFightRecordingSession.StartSession(): numFights has to be positive");
                return;
            }


            RemainingFights = numFights;
            TotalFights = numFights;

            TargetBoss = boss;

            _attemptStarting = false;

            _heroDied = false;
            _stopCorutinesSafe = false;
            SessionActive = true;

            TargetBoss.SetExpectedPlayerAbilities();

            _spawnPoint?.Dispose();
            _spawnPoint = new CustomRespawnPoint("BossFightRecordingSessionRespawn"+TargetBoss.InternalName, TargetBoss.ArenaSceneName, TargetBoss.ArenaPosition);
            _spawnPoint.UseAsTemporary(0);

            PlayerUtils.RemoveCocoon();

        }

        public static void Update()
        {
            if (!SessionActive || _attemptStarting || _stopCorutinesSafe) return;


            if (!BossFightRecorder.IsRecording)
            {
                RemainingFights--;

                _attemptStarting = true;

                EnsureRunner();
                _runner.StartCoroutine(BeginNextAttempt());
            }
            else
            {
                bool recordingJustEnded = BossFightRecorder.RecordFrame();

                if (recordingJustEnded) 
                {
                    if (PlayerData.instance != null && PlayerData.instance.health <= 0)
                    {
                        _heroDied = true;
                        _runner.StartCoroutine(
                            AwaitCocoonAndRemove());
                    }
                    else
                    {
                        _heroDied = false;
                    }

                    if (RemainingFights == 0)
                    {
                        RecordingSessionFinished();
                        return;
                    }
                }

            }
        }
        private static IEnumerator BeginNextAttempt()
        {
            try
            {
                yield return TeleportUtils.AwaitCanTeleport(() => _stopCorutinesSafe);

                if (_stopCorutinesSafe)
                    yield break;

                if (!_heroDied)
                {
                    PlayerUtils.SetFullHP();
                    PlayerUtils.SetFullSilk();

                    if (!TargetBoss.RequireHardSceneReload)
                    {
                        TargetBoss.SetDefeated(false);
                        
                        TeleportUtils.TeleportTo(TargetBoss, true);
                        yield return TeleportUtils.AwaitCanTeleport();
                    }
                    else
                    {
                        TeleportUtils.TeleportTo("Tut_01", Vector3.zero, true); // any room different than the bossfight would do
                        yield return TeleportUtils.AwaitCanTeleport();

                        TargetBoss.SetDefeated(false);
                        TeleportUtils.TeleportTo(TargetBoss, true);
                        yield return TeleportUtils.AwaitCanTeleport();
                    }
                }
                else
                {
                    _spawnPoint.UseAsTemporary(0); // refresh spawnpoint

                    PlayerUtils.SetFullHP();
                    PlayerUtils.SetFullSilk();
                }

                GameCameras.instance.HUDIn(); // turn on HUD in case some boss disables it after death (for example widow does that)

                yield return AwaitBoss(TargetBoss.InternalName, 2500);

                if (!_stopCorutinesSafe)
                    BossFightRecorder.StartRecording(TargetBoss);

            }
            finally
            {
                _attemptStarting = false;
            }

        }
        private static IEnumerator AwaitCocoonAndRemove()
        {
            yield return new WaitUntil(() =>
            {
                var pd = PlayerData.instance;

                if(pd == null) return false;

                return pd.HeroCorpseMarkerGuid != null; 
            });

            PlayerUtils.RemoveCocoon();
        }
        private static IEnumerator AwaitBoss(string awaitedEnemyName, int timeoutFrames)
        {
            int i = 0;
            yield return new WaitUntil(() =>
            {
                if (_stopCorutinesSafe) return true;
                if (i >= timeoutFrames)
                {
                    SilksongAImod.Log.LogWarning($"AwaitBossAndStartRecording(): Timeout hit when awaiting {awaitedEnemyName}");
                    return true;
                }
                var boss = EnemyTracker
                    .GetAll()
                    .FirstOrDefault(e => e.Name == awaitedEnemyName);
                i++;
                return boss != null;
            });
        }

        private static void RecordingSessionFinished()
        {
            if (!SessionActive) return;
            if (_attemptStarting)
            {
                ForceStopRecordingSession();
                return;
            }

            CustomRespawnPoint.ResetTemporary();
            _spawnPoint?.Dispose();

            RemainingFights = 0;
            TotalFights = 0;
            SessionActive = false;

            TeleportUtils.TeleportToBench();
        }
        public static void ForceStopRecordingSession()
        {
            if(!SessionActive || _stopCorutinesSafe) return;

            _stopCorutinesSafe = true;
            EnsureRunner();
            _runner.StartCoroutine(StopSessionGracefully());
        }

        private static IEnumerator StopSessionGracefully()
        {
            TargetBoss.SetDefeated(true);//TODO add pre boss recording game state tracking

            BossFightRecorder.StopRecording();

            yield return new WaitWhile(() =>
            {
                return TeleportUtils.TeleportInProgress || _attemptStarting || BossFightRecorder.IsRecording;
            });

            yield return TeleportUtils.AwaitCanTeleport();

            CustomRespawnPoint.ResetTemporary();
            _spawnPoint?.Dispose(); // remove it after the _attemptStarting is false in case we were just teleporting to it which would freeze the game
            
            TeleportUtils.TeleportToBench();

            RemainingFights = 0;
            TotalFights = 0;
            SessionActive = false;
        }

        private static class BossFightRecorder 
        {

            public static bool IsRecording = false;

            private static Enemy _boss;
            private static List<Enemy> _enemyList = new List<Enemy>(); // every enemy except boss present on scene

            private static StreamWriter _outputFileJSON;
            private static Stream _outputFileBIN;
            private static RecordingInfo _recordingInfo;
            private static int _frameCount = 0;
            private static string _baseFileName;
            private static string _path;
            public static void StartRecording(BossMetaData bossMetaData)
            {
                if (IsRecording)
                {
                    SilksongAImod.Log.LogWarning("BossFightRecorder: Already recording");
                    return;
                }

                _enemyList.Clear();
                _enemyList = EnemyTracker.GetAll().ToList();

                _boss = _enemyList.FirstOrDefault(e => e.Name == bossMetaData.InternalName);
                if (_boss == null)
                {
                    SilksongAImod.Log.LogError($"BossFightRecorder: Boss '{bossMetaData.InternalName}' not found in scene");
                    return;
                }

                _enemyList.Remove( _boss );


                _recordingInfo = new RecordingInfo()
                {
                    BossInternalName = bossMetaData.InternalName,
                    PlayerName = SilksongAImod.SteamUserName
                };

                try
                {
                    var path = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings", bossMetaData.InternalName);
                    Directory.CreateDirectory(path);

                    var baseFileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}";

                    var json = new StreamWriter(Path.Combine(path, baseFileName + ".json")); //TODO: remove the JSON once no longer needed for debuging
                    var bin = new FileStream(Path.Combine(path, baseFileName), 
                        FileMode.Create, FileAccess.Write, FileShare.Read, 
                        bufferSize: 64 * 1024,
                        useAsync: false);

                    // Assign only after success
                    _path = path;
                    _baseFileName = baseFileName;
                    _outputFileJSON = json;
                    _outputFileBIN = bin;
                }
                catch (Exception ex)
                {
                    _outputFileBIN?.Dispose();
                    _outputFileJSON?.Dispose();

                    SilksongAImod.Log.LogError($"Error: BossFightRecorder.StartRecording: {ex}");
                    return;
                }

                IsRecording = true;
                _frameCount = 0;
            }
    
            /// <summary>
            /// 
            /// </summary>
            /// <returns>true if the recording has been stopped this frame</returns>
            public static bool RecordFrame()
            {
                if (!IsRecording)
                {
                    SilksongAImod.Log.LogWarning("BossFightRecorder: Cannot Record Frame because recording has not been started or already ended");
                    return true;
                }

                _frameCount++;

                TrainingEnemyData? bossData = GetDataUtils.GetTrainingEnemyData(_boss.GameObject);
                if (bossData == null)
                {
                    SilksongAImod.Log.LogError("BossFightRecorder: bossData missing. Stopping Recoroding");
                    StopRecording(false);
                    return true;
                }
                
                TrainingHeroData? heroData = GetDataUtils.GetHeroData();
                if (heroData == null)
                {
                    SilksongAImod.Log.LogError("BossFightRecorder: heroData missing. Stopping Recoroding");
                    StopRecording(false);
                    return true;
                }

                var IH = InputHandler.Instance;
                if (IH == null)
                {
                    SilksongAImod.Log.LogError("BossFightRecorder: InputHandler.Instance missing. Stopping Recoroding");
                    StopRecording(false);
                    return true;
                }
                TrainingUserInputs userInputs = inputTracker.GetInputs(IH);

                TrainingFrameData frameData = new TrainingFrameData()
                {
                    Boss = (TrainingEnemyData)bossData,
                    Hero = (TrainingHeroData)heroData,
                    UserInputs = userInputs
                };

                frameData.Enemies = new TrainingEnemyData[_enemyList.Count];
                for (int i = 0; i < _enemyList.Count; i++)
                {
                    TrainingEnemyData? enemyData = GetDataUtils.GetTrainingEnemyData(_enemyList[i].GameObject); 
                    if (enemyData == null)
                    {
                        SilksongAImod.Log.LogWarning("BossFightRecorder: enemyData missing");
                        continue;
                    }

                    frameData.Enemies[i] = (TrainingEnemyData)enemyData;
                }

                var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputFileJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));

                MessagePackSerializer.Serialize(_outputFileBIN, frameData); // save the binary frame data
                
                var HM = _boss.GameObject.GetComponent<HealthManager>();
                if (HM.isDead)
                {
                    StopRecording(true);
                    return true;
                }
                if (PlayerData.instance != null && PlayerData.instance.health <= 0)
                {
                    StopRecording(false);
                    return true;
                }

                return false;

            }

            private static void StopRecording(bool success)
            {
                if (!IsRecording) return;

                IsRecording = false;
                _outputFileJSON.Dispose();
                _outputFileBIN.Dispose();

                _recordingInfo.Success = success;
                _recordingInfo.FrameCount = _frameCount;

                using (StreamWriter outputInfoFile = new StreamWriter(Path.Combine(_path, _baseFileName + "_info.json")))
                {
                    var dataBinWithKeys = MessagePackSerializer.Serialize(_recordingInfo);
                    outputInfoFile.Write(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
                }
            }

            public static void StopRecording()
            {
                StopRecording(false);
            }

        }
    }
    
}

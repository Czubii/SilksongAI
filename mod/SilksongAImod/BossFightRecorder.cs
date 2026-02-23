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

namespace SilksongAI
{
    public static class BossFightRecordingSession
    {
        public static int RemainingFights { get; private set; } = 0;
        public static int TotalFights { get; private set; } = 0;
        public static bool SessionActive { get; private set; } = false;

        private static bool _arenaReloaded = false;
        private static bool _heroDied;

        public static BossMetaData TargetBoss { get; private set; }

        private static bool _awaitingBoss = false;
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

            _arenaReloaded = false;
            _heroDied = false;
            SessionActive = true;

            TargetBoss.SetExpectedPlayerAbilities();

            _spawnPoint = new CustomRespawnPoint("BossFightRecordingSessionRespawn", TargetBoss.ArenaSceneName, TargetBoss.ArenaPosition);

        }

        public static void Update()
        {
            if (!SessionActive || _awaitingBoss) return;


            if (!BossFightRecorder.IsRecording)
            {
                if (RemainingFights == 0)
                {
                    StopRecording();
                    return;
                }

                if (!TeleportUtils.TeleportInProgress && !_arenaReloaded && TeleportUtils.CanPerformTeleportOperations())
                {
                    PlayerUtils.SetFullHP();
                    PlayerUtils.SetFullSilk();
                    

                    _spawnPoint.UseAsTemporary(0);

                    if (!_heroDied)
                    {
                        TargetBoss.SetDefeated(false);
                        TeleportUtils.TeleportTo(TargetBoss);
                    }

                    _arenaReloaded = true;
                }
                if (!TeleportUtils.TeleportInProgress && _arenaReloaded)
                {
                    RemainingFights--;

                    GameCameras.instance.HUDIn(); // turn on HUD in case some boss disables it after death (for example widow does that)

                    _awaitingBoss = true;
                    EnsureRunner();
                    _runner.StartCoroutine(
                        AwaitBossAndStartRecording(TargetBoss.InternalName, 2500));
                    _arenaReloaded = false;
                }
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
                }

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
        private static IEnumerator AwaitBossAndStartRecording(string awaitedEnemyName, int timeoutFrames)
        {
            int i = 0;
            yield return new WaitUntil(() =>
            {
                if (i >= timeoutFrames)
                {
                    SilksongAImod.Log.LogWarning($"AwaitBossAndStartRecording(): Timeout hit when awaiting {awaitedEnemyName}");
                    return true;
                }
                    

                i++;

                var enemies = EnemyTracker.GetAll();

                var boss = EnemyTracker
                    .GetAll()
                    .FirstOrDefault(e => e.Name == awaitedEnemyName);

                return boss != null;
            });

            _awaitingBoss = false;
            BossFightRecorder.StartRecording(TargetBoss);
        }

        public static void StopRecording()
        {
            if(!SessionActive) return;

            if (_awaitingBoss)
            {
                _runner.StopAllCoroutines();
                _awaitingBoss = false;
            }

            CustomRespawnPoint.ResetTemporary();

            RemainingFights = 0;
            TotalFights = 0;
            SessionActive = false;
            BossFightRecorder.StopRecording();

            TargetBoss.SetDefeated(true);
            TeleportUtils.TeleportTo(TargetBoss);

            TargetBoss = null;
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

                    var json = new StreamWriter(Path.Combine(path, baseFileName + ".JSON")); //TODO: remove the JSON once no longer needed for debuging
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

                using (StreamWriter outputInfoFile = new StreamWriter(Path.Combine(_path, _baseFileName + "_info.JSON")))
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

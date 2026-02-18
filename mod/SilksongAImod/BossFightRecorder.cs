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

namespace SilksongAI
{
    public static class BossFightRecordingSession
    {
        private static int _remainingFights = 0;
        public static bool SessionActive { get; private set; } = false;
        private static bool _ArenaReloaded = false;
        private static BossReference _targetBoss;
        public static void StartSession(BossReference boss, int numFights)
        {
            if (SessionActive)
            {
                SilksongAImod.Log.LogWarning("Cannot Start Recording Session! One is still running");
                return;
            }

            _remainingFights = numFights;
            _targetBoss = boss;

            _ArenaReloaded = false;

            SessionActive = true;
        }
        public static void Update()
        {
            if (!SessionActive) return;


            if (!BossFightRecorder.IsRecording)
            {
                if (_remainingFights == 0)
                {
                    SessionActive = false;
                    return;
                }

                if (!TeleportUtils.TeleportInProgress && !_ArenaReloaded && TeleportUtils.CanPerformTeleportOperations())
                {
                    _targetBoss.SetDefeated(false);
                    TeleportUtils.TeleportTo(_targetBoss);
                    _ArenaReloaded = true;
                }
                if (!TeleportUtils.TeleportInProgress && _ArenaReloaded)
                {
                    _remainingFights--;
                    BossFightRecorder.StartRecording(_targetBoss);
                    _ArenaReloaded = false;
                }
            }
            else
            {
                BossFightRecorder.RecordFrame();
            }
        }

        private static class BossFightRecorder
        {
            public static bool IsRecording = false;
            private static GameObject _bossGo = null;
            private static HealthManager _bossHm = null;
            private static StreamWriter _outputFileJSON;
            private static Stream _outputFileBIN;
            private static RecordingInfo _recordingInfo;
            private static int _FrameCount = 0;
            private static string _BaseFileName;
            private static readonly string _Path = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings");
            public static void StartRecording(BossReference boss)
            {
                if (IsRecording)
                {
                    SilksongAImod.Log.LogWarning("BossFightRecorder: Already recording");
                    return;
                }

                _bossGo = GameObject.Find(boss.InternalName);
                if (_bossGo == null)
                {
                    SilksongAImod.Log.LogError($"BossFightRecorder: Boss '{boss.InternalName}' not found in scene");
                    return;
                }

                _bossHm = _bossGo.GetComponent<HealthManager>();
                if (_bossHm == null)
                {
                    SilksongAImod.Log.LogError($"BossFightRecorder: Boss '{boss.InternalName}' game object does not contain HealthManager");
                    return;
                }

                _recordingInfo = new RecordingInfo()
                {
                    BossInternalName = boss.InternalName,
                    PlayerName = SilksongAImod.SteamUserName
                };


                Directory.CreateDirectory(_Path);
                _BaseFileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}";
                _outputFileJSON = new StreamWriter(Path.Combine(_Path, _BaseFileName + ".JSON")); //TODO: remove the JSON once no longer needed for debuging
                _outputFileBIN = new FileStream(Path.Combine(_Path, _BaseFileName), FileMode.Create, FileAccess.Write, FileShare.Read);
                
                IsRecording = true;
                _FrameCount = 0;
            }
 
            public static void RecordFrame()
            {
                if (!IsRecording)
                {
                    SilksongAImod.Log.LogWarning("BossFightRecorder: Cannot Record Frame because recording has not been started or already ended");
                    return;
                }

                HeroController HC = HeroController.instance;
                if (HC == null)
                {
                    SilksongAImod.Log.LogWarning("BossFightRecorder: Hero controller missing");
                    return;
                }

                _FrameCount++;

                EnemyData? enemyData = GetDataUtils.getEnemyData(_bossGo); // TODO make those three take the enemy etc. as an argument
                HeroData? heroData = GetDataUtils.getHeroData();
                UserInputs? userInputs = inputTracker.GetInputs();

                FrameData frameData = new FrameData()
                {
                    enemy = (EnemyData)enemyData,
                    hero = (HeroData)heroData,
                    userInputs = (UserInputs)userInputs
                };

                var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputFileJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));

                MessagePackSerializer.Serialize(_outputFileBIN, frameData); // save the binary frame data


                if (_bossHm.isDead)
                {
                    StopRecording(true);
                }
                if (PlayerData.instance != null && PlayerData.instance.health <= 0)
                {
                    StopRecording(false);
                }

            }

            private static void StopRecording(bool success)
            {
                if (!IsRecording) return;

                IsRecording = false;
                _outputFileJSON.Close();
                _outputFileBIN.Close();


                _recordingInfo.Success = success;
                _recordingInfo.FrameCount = _FrameCount;

                using (StreamWriter outputInfoFile = new StreamWriter(Path.Combine(_Path, _BaseFileName + "_info.JSON")))
                {
                    var dataBinWithKeys = MessagePackSerializer.Serialize(_recordingInfo);
                    outputInfoFile.Write(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
                }


            }
        }
    }
    
}

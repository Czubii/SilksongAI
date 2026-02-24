//using BepInEx;
//using MessagePack;
//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using Steamworks;
//using System.Xml.Linq;
//using UnityEngine.Playables;
//using HutongGames.PlayMaker.Actions;
//using System.Collections;
//using TeamCherry.SharedUtils;
//using MessagePack.Resolvers;

using BepInEx;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SilksongAI
{
    public class BossFightRecorder : MonoBehaviour
    {
        public static BossFightRecorder instance;
        public enum RecordingState
        {
            Idle,
            Recording
        }
        public enum OutputType
        {
            JSON,
            BIN
        }
        private OutputType _outputType;

        private bool _sessionActive = false;
        public RecordingState State {  get; private set; } = RecordingState.Idle;

        private BossMetaData _bossMetaData;

        private EnemyInstance _boss;

        private List<EnemyInstance> _enemies = new List<EnemyInstance>(); // every enemy except boss present on scene

        //private List<DamageSource> _damageSources; // TODO

        private StreamWriter _outputJSON;

        private Stream _outputBIN;

        private string _path;

        private string _baseFileName;

        private RecordingInfo _recordingInfo;

        private int _frameCount = 0;

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

            var go = new GameObject("BossFightRecorder");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<BossFightRecorder>();
        }
        private void OnEnable()
        {
            BossFightSession.OnSessionStarted += OnSessionStarted;
            BossFightSession.OnSessionStopped += OnSessionStopped;
            BossFightSession.OnFightStarted += StartRecording;
            BossFightSession.OnFightFinished += StopRecording;
        }
        private void OnDisable()
        {
            BossFightSession.OnSessionStarted -= OnSessionStarted;
            BossFightSession.OnSessionStopped -= OnSessionStopped;
            BossFightSession.OnFightStarted -= StartRecording;
            BossFightSession.OnFightFinished -= StopRecording;
        }
        private void OnSessionStarted(BossFightSession.SessionInfo sessionInfo)
        {
            if (_sessionActive || State != RecordingState.Idle) return;

            _bossMetaData = sessionInfo.TargetBoss;

            _sessionActive = true;
        }
        private void OnSessionStopped(bool forcedStop)
        {
            if(!_sessionActive) return;
            if(State == RecordingState.Recording) StopRecordingPrematurely();

            _path = null;
            _baseFileName = null;

            _sessionActive = false;
        }
        private void StartRecording()
        {
            if(!_sessionActive || State != RecordingState.Idle) return;

            var path = GetOutputPath();
            var baseFilename = GetBaseOutputFilename();

            try // initialize directory and files
            {
                Directory.CreateDirectory(path);

                if (_outputType == OutputType.JSON)
                {
                    var json = new StreamWriter(Path.Combine(path, baseFilename + ".json")); //TODO: remove the JSON once no longer needed for debuging

                    _outputJSON = json;
                    _outputBIN = null;
                }
                else if (_outputType == OutputType.BIN)
                {
                    var bin = new FileStream(Path.Combine(path, baseFilename),
                    FileMode.Create, FileAccess.Write, FileShare.Read,
                    bufferSize: 64 * 1024,
                    useAsync: false);

                    var buffered = new BufferedStream(bin, 64 * 1024);

                    _outputJSON = null;
                    _outputBIN = buffered;
                }
            }
            catch (Exception ex)
            {
                _outputBIN?.Dispose();
                _outputJSON?.Dispose();

                SilksongAImod.Log.LogError($"Error: BossFightRecorder.StartRecording(): {ex}");
                return;
            }

            _enemies.Clear();
            _enemies = EnemyTracker.GetAll().ToList();

            _boss = _enemies.FirstOrDefault(e => e.Name == _bossMetaData.InternalName);
            if (_boss == null)
            {
                SilksongAImod.Log.LogError($"BossFightRecorder(): Boss '{_bossMetaData.InternalName}' not found in scene");
                return;
            }

            _enemies.Remove(_boss);

            _recordingInfo = new RecordingInfo()
            {
                BossInternalName = _bossMetaData.InternalName,
                PlayerName = SilksongAImod.SteamUserName
            };

            _frameCount = 0;
            State = RecordingState.Recording;
        }
        private void StopRecording(BossFightSession.FightResults fightResults) // close files write the info about recording
        {
            if (!_sessionActive || State != RecordingState.Recording) return;

            State = RecordingState.Idle;

            _outputJSON?.Dispose();
            _outputBIN?.Dispose();

            _recordingInfo.Success = fightResults.Success;
            _recordingInfo.FrameCount = _frameCount;

            var path = GetOutputPath();
            var baseFileName = GetBaseOutputFilename();

            using (StreamWriter outputInfoFile = new StreamWriter(Path.Combine(path, baseFileName + "_info.json")))
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(_recordingInfo);
                outputInfoFile.Write(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }

            _baseFileName = null;
        }
        private void StopRecordingPrematurely()
        {
            StopRecording(new BossFightSession.FightResults(false));
        }
        private string GetOutputPath()
        {
            if (_path == null)
                _path = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings", _bossMetaData.InternalName);

            return _path;
        }
        private string GetBaseOutputFilename()
        {
            if (_baseFileName == null)
                _baseFileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}";

            return _baseFileName;
        }
        private void Update()
        {
            if (State != RecordingState.Recording) return;

            RecordFrame();
        }
        public void RecordFrame()
        {
            _frameCount++;

            TrainingEnemyData? bossData = GetDataUtils.GetTrainingEnemyData(_boss.GameObject);
            if (bossData == null)
            {
                SilksongAImod.Log.LogError("BossFightRecorder: bossData missing. Stopping Recoroding");
                StopRecordingPrematurely();
                return;
            }

            TrainingHeroData? heroData = GetDataUtils.GetHeroData();
            if (heroData == null)
            {
                SilksongAImod.Log.LogError("BossFightRecorder: heroData missing. Stopping Recoroding");
                StopRecordingPrematurely();
                return;
            }

            var IH = InputHandler.Instance;
            if (IH == null)
            {
                SilksongAImod.Log.LogError("BossFightRecorder: InputHandler.Instance missing. Stopping Recoroding");
                StopRecordingPrematurely();
                return;
            }

            TrainingUserInputs userInputs = inputTracker.GetInputs(IH);

            TrainingFrameData frameData = new TrainingFrameData()
            {
                Boss = (TrainingEnemyData)bossData,
                Hero = (TrainingHeroData)heroData,
                UserInputs = userInputs
            };

            frameData.Enemies = new TrainingEnemyData[_enemies.Count];
            for (int i = 0; i < _enemies.Count; i++)
            {
                TrainingEnemyData? enemyData = GetDataUtils.GetTrainingEnemyData(_enemies[i].GameObject);
                if (enemyData == null)
                {
                    SilksongAImod.Log.LogWarning("BossFightRecorder: enemyData missing");
                    continue;
                }

                frameData.Enemies[i] = (TrainingEnemyData)enemyData;
            }

            if (_outputType == OutputType.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (_outputType == OutputType.BIN)
            {
                MessagePackSerializer.Serialize(_outputBIN, frameData); // save the binary frame data
            }

            return;
        }
        public void SetOutputType(OutputType type)
        {
            if(State != RecordingState.Idle || _sessionActive)
            {
                SilksongAImod.Log.LogError("BossFightRecorder.SetOutputType(): Cannot change the output type while recording");
                return;
            }
            _outputType = type;
        }

    }
}

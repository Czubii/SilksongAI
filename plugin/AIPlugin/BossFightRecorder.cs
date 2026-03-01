using BepInEx;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using static AIPlugin.BossFightSession;

namespace AIPlugin
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
            MSGPACK
        }
        private OutputType _outputType = OutputType.MSGPACK;

        private int _recordEvertNFrame = 10;
        private int _framesToNextRecord = 0;

        private bool _sessionActive = false;
        public RecordingState State {  get; private set; } = RecordingState.Idle;

        private BossMetaData _bossMetaData;

        private EnemyInstance _boss;

        private List<EnemyInstance> _enemies = new List<EnemyInstance>(); // every enemy except boss present on scene

        //private List<DamageSource> _damageSources; // TODO

        private StreamWriter _outputJSON;

        private Stream _outputBIN;

        private string _directory;

        private string _baseFileName;

        private string _tempFilePath;

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

            _directory = null;
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

                _tempFilePath = Path.Combine(path, baseFilename + ".tmp");

                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                if (_outputType == OutputType.JSON)
                {
                    var json = new StreamWriter(_tempFilePath);

                    _outputJSON = json;
                    _outputBIN = null;
                }
                else if (_outputType == OutputType.MSGPACK)
                {
                    var bin = new FileStream(_tempFilePath,
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

                AIPlugin.Log.LogError($"Error: BossFightRecorder.StartRecording(): {ex}");
                return;
            }

            _enemies.Clear();
            _enemies = EnemyTracker.GetAll().ToList();

            _boss = _enemies.FirstOrDefault(e => e.Name == _bossMetaData.InternalName);
            if (_boss == null)
            {
                AIPlugin.Log.LogError($"BossFightRecorder(): Boss '{_bossMetaData.InternalName}' not found in scene");
                return;
            }

            _enemies.Remove(_boss);

            WriteHeader();

            _frameCount = 0;
            _framesToNextRecord = 0;
            State = RecordingState.Recording;
        }

        private void WriteHeader()
        {
            RecordingHeader header = new RecordingHeader 
            { 
                PlayerName = AIPlugin.SteamUserName,
                BossName = _bossMetaData.InternalName,
            };

            header.EnemyNames = new string[_enemies.Count + 1];
            header.EnemyNames[0] = _boss.Name;

            for (int i = 0; i < _enemies.Count; i++)
            {
                header.EnemyNames[i+1] = _enemies[i].Name;
            }

            // Write the header:
            if (_outputType == OutputType.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(header, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (_outputType == OutputType.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, header); // save the binary frame data
            }
        }

        private void WriteFooter(FightResults fightResults)
        {
            RecordingFooter footer = new RecordingFooter
            {
                Success = fightResults.Success,
                FrameCount = _frameCount
            };

            // Write the footer:
            if (_outputType == OutputType.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(footer, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (_outputType == OutputType.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, footer); // save the binary frame data
            }
        }

        private void StopRecording(FightResults fightResults) // close files write the info about recording
        {
            if (!_sessionActive || State != RecordingState.Recording) return;

            WriteFooter(fightResults);

            State = RecordingState.Idle;

            _outputJSON?.Dispose();
            _outputBIN?.Dispose();

            // rename the output:

            var path = GetOutputPath();
            var baseFilename = GetBaseOutputFilename();

            string newPath = "";

            if (_outputType == OutputType.JSON)
                newPath = Path.Combine(path, baseFilename + ".json");
            else
                newPath = Path.Combine(path, baseFilename + ".msgpack");

            if (File.Exists(_tempFilePath))
            {
                File.Move(_tempFilePath, newPath);
            }

            _tempFilePath = null;
            _baseFileName = null;
        }
        private void StopRecordingPrematurely()
        {
            StopRecording(new BossFightSession.FightResults(false));
        }
        private string GetOutputPath()
        {
            if (_directory == null)
                _directory = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings", _bossMetaData.InternalName);

            return _directory;
        }
        private string GetBaseOutputFilename()
        {
            if (_baseFileName == null)
                _baseFileName = $"session_{DateTime.Now:yyyyMMdd_HHmmss}";

            return _baseFileName;
        }
        private void Update()
        {
            if (State != RecordingState.Recording 
                || (GameManager.instance?.IsGamePaused() ?? true)) return;

            _framesToNextRecord--;

            if (_framesToNextRecord <= 0)
            {
                RecordFrame();
                _framesToNextRecord = _recordEvertNFrame;
            }
        }
        public void RecordFrame()
        {
            _frameCount++;

            FrameData frameData = FrameDataCollector.GetAll(_boss, _enemies);

            if (frameData == null)
            {
                AIPlugin.Log.LogError("BossFightRecorder: No frame data. Stoping Recording");
                StopRecordingPrematurely();
                return;
            }

            if (_outputType == OutputType.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (_outputType == OutputType.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, frameData); // save the binary frame data
            }

            return;
        }
        public void SetOutputType(OutputType type)
        {
            if(State != RecordingState.Idle || _sessionActive)
            {
                AIPlugin.Log.LogError("BossFightRecorder.SetOutputType(): Cannot change the output type while recording");
                return;
            }
            _outputType = type;
        }

    }
}

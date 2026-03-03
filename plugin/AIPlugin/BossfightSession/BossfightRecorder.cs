using BepInEx;
using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AIPlugin.BossfightSession
{
    public class BossfightRecorder : MonoBehaviour, ISessionListener
    {
        public enum RecordingState
        {
            Idle,
            Recording
        }

        private int _framesToNextRecord = 0;
        public RecordingState State {  get; private set; } = RecordingState.Idle;

        //private List<DamageSource> _damageSources; // TODO

        private StreamWriter _outputJSON;

        private Stream _outputBIN;

        private string _tempFilePath;

        private int _frameCount = 0;

        private SessionEnemyManager _enemyManager;

        public void Initialize(SessionEnemyManager enemyManager)
        {
            _enemyManager = enemyManager;
        }
        private void OnDisable()
        {
            if (State == RecordingState.Recording)
            {
                StopRecordingPrematurely(); // closes streams, writes footer as failure, renames temp
            }
        }
        public void OnFightStarted()
        {
            if(State != RecordingState.Idle || !enabled) return;

            var path = GetOutputPath(_enemyManager.GetTargetInstance().Name);
            var baseFilename = GetBaseOutputFilename();

            try // initialize directory and files
            {
                Directory.CreateDirectory(path);

                _tempFilePath = Path.Combine(path, baseFilename + ".tmp");

                if (File.Exists(_tempFilePath))
                    File.Delete(_tempFilePath);

                if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.JSON)
                {
                    var json = new StreamWriter(_tempFilePath);

                    _outputJSON = json;
                    _outputBIN = null;
                }
                else if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.MSGPACK)
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

            WriteHeader();

            _frameCount = 0;
            _framesToNextRecord = 0;
            State = RecordingState.Recording;
        }

        private void WriteHeader()
        {
            EnemyInstance boss = _enemyManager.GetTargetInstance();
            List<EnemyInstance> enemies = _enemyManager.GetNonTargetInstances();

            RecordingHeader header = new RecordingHeader 
            { 
                PlayerName = AIPlugin.SteamUserName,
                BossName = boss.Name,
            };

            header.EnemyNames = new string[enemies.Count + 1];
            header.EnemyNames[0] = boss.Name;

            for (int i = 0; i < enemies.Count; i++)
            {
                header.EnemyNames[i+1] = enemies[i].Name;
            }

            // Write the header:
            if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(header, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, header); // save the binary frame data
            }
        }

        private void WriteFooter(AttemptResult result)
        {
            RecordingFooter footer = new RecordingFooter
            {
                Success = result == AttemptResult.Success,
                FrameCount = _frameCount
            };

            // Write the footer:
            if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(footer, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, footer); // save the binary frame data
            }
        }

        public void OnFightFinished(AttemptResult result) // close files write the info about recording
        {
            if (State != RecordingState.Recording) return;

            WriteFooter(result);

            State = RecordingState.Idle;

            _outputJSON?.Dispose();
            _outputBIN?.Dispose();

            if (File.Exists(_tempFilePath))
            {
                if (result == AttemptResult.Success || result == AttemptResult.HeroDied)
                {
                    string newPath;

                    if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.JSON)
                        newPath = Path.ChangeExtension(_tempFilePath, ".json");
                    else
                        newPath = Path.ChangeExtension(_tempFilePath, ".msgpack");

                    File.Move(_tempFilePath, newPath);
                }
                else
                {
                    File.Delete(_tempFilePath);
                }
            }

            _tempFilePath = null;
        }
        private void StopRecordingPrematurely()
        {
            OnFightFinished(AttemptResult.ForcedStop);
        }
        private string GetOutputPath(string bossName)
        {
            return Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings", bossName);
        }
        private string GetBaseOutputFilename()
        {
            return $"session_{DateTime.Now:yyyyMMdd_HHmmss}";
        }
        private void Update()
        {
            if (State != RecordingState.Recording 
                || (GameManager.instance?.IsGamePaused() ?? true)) return;

            _framesToNextRecord--;

            if (_framesToNextRecord <= 0)
            {
                RecordFrame();
                _framesToNextRecord = SessionConfig.RecordFrameDelta;
            }
        }
        public void RecordFrame()
        {
            _frameCount++;

            EnemyInstance boss = _enemyManager?.GetTargetInstance() ?? null;
            List<EnemyInstance> enemies = _enemyManager?.GetNonTargetInstances() ?? null;

            RecordingFrameData frameData = FrameDataCollector.GetAll(boss, enemies);

            if (frameData == null)
            {
                AIPlugin.Log.LogError("BossFightRecorder: No frame data. Stoping Recording");
                StopRecordingPrematurely();
                return;
            }

            if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, frameData); // save the binary frame data
            }

            return;
        }
    }
}

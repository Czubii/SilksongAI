using AIPlugin.Utilities;
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
    public class BossfightRecorder : MonoBehaviour, ISessionListener, IFrameCaptureListener
    {
        private bool _isRecording = false;

        private int _frameCount = 0;

        private int _accumulatedReward = 0;

        private StreamWriter _outputJSON;

        private Stream _outputBIN;

        private string _tempFilePath;

        private SessionEnemyTracker _enemyManager; 

        public void Initialize(SessionEnemyTracker enemyManager)
        {
            _enemyManager = enemyManager; // TODO try to remove this somehow
        }
        private void OnDisable()
        {
            if (_isRecording)
            {
                StopRecordingPrematurely(); // closes streams, writes footer as failure, renames temp
            }
        }
        public void OnFightStarted()
        {
            if(_isRecording || !enabled) return;

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
            _accumulatedReward = 0;
            _isRecording = true;
        }
        public void OnFightFinished(AttemptResult result) // close files write the info about recording
        {
            if (!_isRecording) return;
            _isRecording = false;
            WriteFooter(result);

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
        public void OnFrameCaptured(RecordingFrameData frameData) 
        {
            if(!_isRecording || !enabled) return;

            _frameCount++;
            _accumulatedReward += frameData.Reward;

            if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.JSON)
            {
                var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                _outputJSON.WriteLine(MessagePackSerializer.ConvertToJson(dataBinWithKeys));
            }
            else if (SessionConfig.RecordingOutputType == SessionConfig.RecordingOutputTypes.MSGPACK)
            {
                MessagePackSerializer.Serialize(_outputBIN, frameData); // save the binary frame data
            }
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
                header.EnemyNames[i + 1] = enemies[i].Name;
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
                FrameCount = _frameCount,
                TotalReward = _accumulatedReward
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
    }
}

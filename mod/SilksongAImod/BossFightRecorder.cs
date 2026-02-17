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

namespace SilksongAI
{
    public class BossFightRecorder
    {
        private bool _isRecording = false;
        private GameObject _bossGo = null;
        private HealthManager _bossHm = null;
        private StreamWriter _outputFileJSON;
        private Stream _outputFileBIN;
        public void StartRecording(string bossName)
        {
            if (_isRecording) return;

            _bossGo = GameObject.Find(bossName);
            if (_bossGo == null) return;

            _bossHm = _bossGo.GetComponent<HealthManager>();
            if(_bossHm == null) return;

            _isRecording = true;

            string path = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings");
            _outputFileJSON = new StreamWriter(Path.Combine(path, $"session_{DateTime.Now:yyyyMMdd_HHmmss}.JSON")); //TODO: remove the JSON once no longer needed for debuging
            _outputFileBIN = new FileStream(Path.Combine(path, $"session_{DateTime.Now:yyyyMMdd_HHmmss}"), FileMode.Create, FileAccess.Write, FileShare.Read);

        }
        public void RecordFrame()
        {
            if (!_isRecording) return;

            EnemyData? enemyData = GetDataUtils.getEnemyData(_bossGo);
            HeroData? heroData = GetDataUtils.getHeroData();
            UserInputs? userInputs = inputTracker.GetInputs();

            FrameData frameData = new FrameData()
            {
                enemy = (EnemyData)enemyData,
                hero = (HeroData)heroData,
                userInputs = (UserInputs)userInputs
            };

            var dataBinWithKeys = MessagePackSerializer.Serialize(frameData, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            _outputFileJSON.WriteLineAsync(MessagePackSerializer.ConvertToJson(dataBinWithKeys));

            MessagePackSerializer.Serialize(_outputFileBIN, frameData); // save the binary frame data


            if (_bossHm.isDead)
            {
                _isRecording = false;
                _outputFileJSON.Close();
                return;
            }

        }
    }
}

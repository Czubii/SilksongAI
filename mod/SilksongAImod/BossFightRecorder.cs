using BepInEx;
using MessagePack;
using System;
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
        private StreamWriter _outputFile;

        public void StartRecording(string bossName)
        {
            if (_isRecording) return;

            _bossGo = GameObject.Find(bossName);
            if (_bossGo == null) return;

            _bossHm = _bossGo.GetComponent<HealthManager>();
            if(_bossHm == null) return;

            _isRecording = true;

            string path = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings");
            _outputFile = new StreamWriter(Path.Combine(path, $"session_{DateTime.Now:yyyyMMdd_HHmmss}"));

        }
        public void RecordFrame()
        {
            if (!_isRecording) return;

            EnemyData? enemyData = GetDataUtils.getEnemyData(_bossGo);
            HeroData? heroData = GetDataUtils.getHeroData();


            FrameData frameData = new FrameData()
            {
                enemy = (EnemyData)enemyData,
                hero = (HeroData)heroData
            };

            var dataBin = MessagePackSerializer.Serialize(frameData);
            _outputFile.WriteLineAsync(MessagePackSerializer.ConvertToJson(dataBin));

            if (_bossHm.isDead)
            {
                _isRecording = false;
                _outputFile.Close();
                return;
            }

        }
    }
}

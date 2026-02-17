using BepInEx;
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
        private int i = 0;

        public void StartRecording(string bossName)
        {
            if (_isRecording) return;

            _bossGo = GameObject.Find(bossName);
            if (_bossGo == null) return;

            _bossHm = _bossGo.GetComponent<HealthManager>();
            if(_bossHm == null) return;

            string path = Path.Combine(Paths.PluginPath, "SilksongAI", "Recordings");

            _outputFile = new StreamWriter(Path.Combine(path, $"session_{DateTime.Now:yyyyMMdd_HHmmss}"));

            _isRecording = true;
            i = 0;

        }
        public void RecordFrame()
        {
            if (!_isRecording) return;

            if (_bossHm.isDead)
            {
                _isRecording = false;

                _outputFile.WriteLineAsync($"Frame: {i}, HP: {_bossHm.hp}");

                _outputFile.Close();
                return;
            }

            _outputFile.WriteLineAsync($"Frame: {i}, HP: {_bossHm.hp}");
            i++;
        }

    }
}

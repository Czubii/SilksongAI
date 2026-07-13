using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;


namespace WeaverNet.Mod.Game.Bosses
{
   
    public class DataDrivenBossDatabase : IBossDatabase
    {
        private readonly string _metadataRootPath;

        private List<BossPreset> _allBosses = null;
        private bool _loaded = false;
        public IReadOnlyList<BossPreset> All
        {
            get
            {
                if (!_loaded) Load();
                return _allBosses;
            }
        }
        public DataDrivenBossDatabase(string metadataRootPath)
        {
            _metadataRootPath = metadataRootPath;
        }
        public void Load()
        {
            try
            {
                string[] files = Directory.GetFiles(_metadataRootPath, "*.json", SearchOption.AllDirectories);
                _allBosses = new List<BossPreset>();

                foreach (string file in files)
                {
                    PluginLog.Info($"Loading Boss Metadata File: {Path.GetFileName(file)}");

                    string jsonContent = File.ReadAllText(file);

                    RawBossMetadata boss = JsonConvert.DeserializeObject<RawBossMetadata>(jsonContent);

                    _allBosses.Add(boss.Build());
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.Message);
            }

            if ((_allBosses?.Count ?? 0) == 0)
            {
                PluginLog.Warning("No Boss Metadata Files Found!");
            }

            _loaded = true;
        }
    }
}

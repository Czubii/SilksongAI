using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;


namespace WeaverNet.Infrastructure.Databases
{
   
    public class DataDrivenBossDatabase : IBossDatabase
    {
        private readonly string _metadataRootPath;

        private List<BossData> _allBosses = null;
        private bool _loaded = false;
        public IReadOnlyList<BossData> All
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
                _allBosses = new List<BossData>();

                foreach (string file in files)
                {
#if DEBUG
                    PluginLog.Info($"Loading Boss Metadata File: {Path.GetFileName(file)}");
#endif

                    string jsonContent = File.ReadAllText(file);

                    BossData boss = JsonConvert.DeserializeObject<BossData>(jsonContent);

                    if (boss == null)
                    {
                        PluginLog.Warning($"Failed loading loadout: {file}");
                        continue;
                    }

                    _allBosses.Add(boss);
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

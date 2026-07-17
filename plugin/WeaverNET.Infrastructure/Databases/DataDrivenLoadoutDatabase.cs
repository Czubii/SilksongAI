using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Infrastructure.Interfaces;


namespace WeaverNet.Infrastructure.Databases
{

    public class DataDrivenLoadoutDatabase : ILoadoutDatabase //TODO in the future add a base class for readonly and modifiable databases for clarity 
    {
        private readonly string _metadataRootPath;

        private List<LoadoutData> _allLoadouts = null;
        private bool _loaded = false;
        public IReadOnlyList<LoadoutData> All
        {
            get
            {
                if (!_loaded) Load();
                return _allLoadouts;
            }
        }
        public DataDrivenLoadoutDatabase(string metadataRootPath)
        {
            _metadataRootPath = metadataRootPath;
        }
        public void Load()
        {
            try
            {
                string[] files = Directory.GetFiles(_metadataRootPath, "*.json", SearchOption.AllDirectories);
                _allLoadouts = new List<LoadoutData>();

                foreach (string file in files)
                {
#if DEBUG
                    PluginLog.Info($"Loading Loadout File: {Path.GetFileName(file)}");
#endif
                    string jsonContent = File.ReadAllText(file);

                    LoadoutData loadout = JsonConvert.DeserializeObject<LoadoutData>(jsonContent);

                    if (loadout == null)
                    {
                        PluginLog.Warning($"Failed loading loadout: {file}");
                        continue;
                    }

                    _allLoadouts.Add(loadout);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.Message);
            }

            if ((_allLoadouts?.Count ?? 0) == 0)
            {
                PluginLog.Warning("No Loadout Files Found!");
            }

            _loaded = true;
        }
        private bool NameInDatabase(string name)
        {
            return _allLoadouts.Any(a => a.Name == name);
        }
        /// <summary>
        /// throws DuplicateNameException() if an entry with the same name already exisits
        /// </summary>
        public void Add(LoadoutData newEntry)
        {
            if (NameInDatabase(newEntry.Name)) throw new DuplicateNameException(newEntry.Name);
            SaveToFile(newEntry);
            _allLoadouts.Add(newEntry);
        }
        public void AddOrReplace(LoadoutData entry)
        {
            if (NameInDatabase(entry.Name)) RemoveEntriesAndFiles(entry.Name);

            SaveToFile(entry);
            _allLoadouts.Add(entry);
        }
        private void RemoveEntriesAndFiles(string entryName)
        {
            string[] files = Directory.GetFiles(_metadataRootPath, "*.json", SearchOption.AllDirectories);

            _allLoadouts.RemoveAll(a=>a.Name== entryName); // remove entries
            foreach (string file in files) // remove the file
            {
                string jsonContent = File.ReadAllText(file);

                LoadoutData loadout = JsonConvert.DeserializeObject<LoadoutData>(jsonContent);

                if (loadout == null || loadout.Name != entryName) continue;

                File.Delete(file);
            }
        }
        private void SaveToFile(LoadoutData newEntry)
        {
            string jsonString = JsonConvert.SerializeObject(newEntry);
            var fileName = String.Concat(newEntry.Name, ".json");
            var filePath = Path.Combine(_metadataRootPath, fileName);

            File.WriteAllText(filePath, jsonString);
        }
    }
}

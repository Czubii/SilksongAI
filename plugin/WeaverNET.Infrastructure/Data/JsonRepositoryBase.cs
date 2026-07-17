using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNET.Infrastructure.Data
{
    public class JsonRepositoryBase<TData>
    {
        protected readonly List<JsonRepositoryEntry<TData>> _entries = new List<JsonRepositoryEntry<TData>>();
        protected IReadOnlyCollection<TData> _data = Array.Empty<TData>();
        protected readonly Dictionary<string, JsonRepositoryEntry<TData>> _entriesById =
            new Dictionary<string, JsonRepositoryEntry<TData>>(StringComparer.OrdinalIgnoreCase);

        private bool _loaded = false;
        protected string _repositoryRoot { get; }
        protected readonly Func<TData, string> _idSelector;
        protected readonly object _loadLock = new object();

        protected JsonRepositoryBase(string repositoryRoot, Func<TData, string> idSelector)
        {
            _repositoryRoot = repositoryRoot ?? throw new ArgumentNullException(nameof(repositoryRoot));
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        protected void EnsureLoaded()
        {
            if (_loaded)
                return;

            lock (_loadLock)
            {
                if (!_loaded)
                    Load();
            }
        }

        protected JsonRepositoryEntry<TData> GetByIdInternal(string id)
        {
            EnsureLoaded();

            JsonRepositoryEntry<TData> entry;

            if (_entriesById.TryGetValue(id, out entry))
                return entry;

            return null;
        }

        protected virtual void Load()
        {
            _entries.Clear();
            _entriesById.Clear();

            try
            {
                string[] files = Directory.GetFiles(
                    _repositoryRoot,
                    "*.json",
                    SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    try
                    {
#if DEBUG
                        PluginLog.Info(
                            $"Loading {typeof(TData).Name} database entry. File: {Path.GetFileName(file)}");
#endif

                        string jsonContent = File.ReadAllText(file);

                        TData entry = JsonConvert.DeserializeObject<TData>(jsonContent);

                        if (entry == null)
                        {
                            PluginLog.Warning($"Failed loading: {file}");
                            continue;
                        }

                        string id = _idSelector(entry);

                        if (string.IsNullOrEmpty(id))
                        {
                            PluginLog.Warning(
                                $"Skipping {file}: {typeof(TData).Name} has an invalid ID.");
                            continue;
                        }

                        if (_entriesById.ContainsKey(id))
                        {
                            PluginLog.Error(
                                $"Duplicate {typeof(TData).Name} ID '{id}' found in {file}");
                            continue;
                        }

                        JsonRepositoryEntry<TData> repositoryEntry =
                            new JsonRepositoryEntry<TData>(entry, file);

                        _entries.Add(repositoryEntry);
                        _entriesById.Add(id, repositoryEntry);
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"Failed loading {file}: {ex.Message}");
                    }
                }

                _data = _entries.Select(x => x.Data).ToList();

                if (_entries.Count == 0)
                {
                    PluginLog.Warning(
                        $"No {typeof(TData).Name} database entries found!");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(
                    $"An error occured when loading {typeof(TData).Name} database\n{ex.Message}");

                _data = Array.Empty<TData>();
            }
            finally
            {
                _loaded = true;
            }
        }

        protected void UpdateEntry(TData data, string filePath)
        {
            string id = _idSelector(data);

            JsonRepositoryEntry<TData> newEntry =
                new JsonRepositoryEntry<TData>(data, filePath);

            _entriesById[id] = newEntry;

            int index = _entries.FindIndex(
                x => _idSelector(x.Data) == id);

            if (index >= 0)
                _entries[index] = newEntry;
            else
                _entries.Add(newEntry);

            _data = _entries.Select(x => x.Data).ToList();
        }
    }
}

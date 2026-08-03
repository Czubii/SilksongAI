using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeaverNet.Core.Infrastructure.Interfaces;

namespace WeaverNET.Infrastructure.Data.Json
{
    public class JsonRepository<TData> : JsonRepositoryBase<TData>, IRepository<TData, string>
    {
        public event Action RepositoryChanged;
        public JsonRepository(string repositoryRoot, Func<TData, string> idSelector) : base(repositoryRoot, idSelector) { }
        public IReadOnlyCollection<TData> All
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }
        public TData GetById(string id)
        {
            var entry = GetByIdInternal(id);
            if (entry == null)
                throw new KeyNotFoundException($"Entry '{id}' was not found.");

            return entry.Data;
        }
        public void Add(TData data)
        {
            EnsureLoaded();

            string id = _idSelector(data);

            if (_entriesById.ContainsKey(id))
                throw new InvalidOperationException(
                    $"An entry with ID '{id}' already exists.");

            string filePath = Path.Combine(_repositoryRoot, id + ".json");

            SaveEntry(data, filePath);
            UpdateEntry(data, filePath);
            NotifyChanged();
        }
        public void AddOrReplace(TData data)
        {
            EnsureLoaded();

            string id = _idSelector(data);
            string filePath = Path.Combine(_repositoryRoot, id + ".json");

            SaveEntry(data, filePath);
            UpdateEntry(data, filePath);
            NotifyChanged();
        }
        public void Remove(string id)
        {
            EnsureLoaded();
            var entry = GetByIdInternal(id);
            if (entry == null) return;

            File.Delete(entry.SourcePath);
            _entries.Remove(entry);
            _entriesById.Remove(id);

            _data = _entries.Select(x => x.Data).ToList();
            NotifyChanged();
        }
        private void SaveEntry(TData data, string filePath)
        {
            Directory.CreateDirectory(_repositoryRoot);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        private void NotifyChanged()
        {
            RepositoryChanged?.Invoke();
        }
        
    }
}

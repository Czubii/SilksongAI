using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNET.Infrastructure.Data.Interfaces;

namespace WeaverNET.Infrastructure.Data
{
    public class JsonRepository<TData> : JsonRepositoryBase<TData>, IRepository<TData>
    {
        public JsonRepository(string repositoryRoot, Func<TData, string> idSelector) : base(repositoryRoot, idSelector) { }
        public IReadOnlyCollection<TData> GetAll
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }
        public TData GetById(string id)
        {
            EnsureLoaded();
            return GetByIdInternal(id);
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
        }
        public void AddOrReplace(TData data)
        {
            EnsureLoaded();

            string id = _idSelector(data);
            string filePath = Path.Combine(_repositoryRoot, id + ".json");

            SaveEntry(data, filePath);
            UpdateEntry(data, filePath);
        }
        private void SaveEntry(TData data, string filePath)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}

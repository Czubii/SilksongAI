using System;
using System.Collections.Generic;
using WeaverNet.Core.Infrastructure.Interfaces;

namespace WeaverNET.Infrastructure.Data.Json
{
    public class JsonReadOnlyRepository<TData>: JsonRepositoryBase<TData>, IReadOnlyRepository<TData, string>
    {
        public JsonReadOnlyRepository(string repositoryRoot, Func<TData, string> idSelector) : base(repositoryRoot, idSelector) { }
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
    }
}

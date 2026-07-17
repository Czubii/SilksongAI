using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WeaverNet.Core.Infrastructure;
using WeaverNET.Infrastructure.Data.Interfaces;

namespace WeaverNET.Infrastructure.Data
{
    public class JsonReadOnlyRepository<TData>: JsonRepositoryBase<TData>, IReadOnlyRepository<TData>
    {
        public JsonReadOnlyRepository(string repositoryRoot, Func<TData, string> idSelector) : base(repositoryRoot, idSelector) { }
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
    }
}

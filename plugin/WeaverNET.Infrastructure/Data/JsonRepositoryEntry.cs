using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNET.Infrastructure.Data
{
    public class JsonRepositoryEntry<TData>
    {
        public TData Data { get; }
        public string SourcePath { get; }
        public JsonRepositoryEntry(TData data, string sourcePath)
        {
            Data = data;
            SourcePath = sourcePath;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    public interface IRepository<TData, TId> : IReadOnlyRepository<TData, TId>
    {
        event Action RepositoryChanged;
        void Add(TData data);
        void AddOrReplace(TData data);
        void Remove(TId id);
    }
}

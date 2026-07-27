using System.Collections.Generic;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    public interface IReadOnlyRepository<TData, TId>
    {
        IReadOnlyCollection<TData> All { get; }
        TData GetById(TId id);
    }
}

using System.Collections.Generic;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    public interface IReadOnlyRepository<TData>
    {
        IReadOnlyCollection<TData> All { get; }
        TData GetById(string id);
    }
}

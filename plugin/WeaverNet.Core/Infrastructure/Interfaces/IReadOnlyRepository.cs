using System.Collections.Generic;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    public interface IReadOnlyRepository<TData>
    {
        IReadOnlyCollection<TData> GetAll { get; }
        TData GetById(string id);
    }
}

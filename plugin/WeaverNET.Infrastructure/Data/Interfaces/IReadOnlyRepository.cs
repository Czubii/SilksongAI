using System.Collections.Generic;

namespace WeaverNET.Infrastructure.Data.Interfaces
{
    public interface IReadOnlyRepository<TData>
    {
        IReadOnlyCollection<TData> GetAll { get; }
        TData GetById(string id);
    }
}

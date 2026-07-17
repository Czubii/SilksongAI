using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    public interface IRepository<TData>: IReadOnlyRepository<TData>
    {
        void Add(TData data);
        void AddOrReplace(TData data);
    }
}

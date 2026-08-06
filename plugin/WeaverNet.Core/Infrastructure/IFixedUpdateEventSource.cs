using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure
{
    public interface IFixedUpdateEventSource
    {
        void Register(IFixedUpdateListener listener);
        void Unregister(IFixedUpdateListener listener);
    }
}

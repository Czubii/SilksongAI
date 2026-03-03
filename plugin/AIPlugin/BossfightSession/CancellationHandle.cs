using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public class CancellationHandle
    {
        private volatile bool _stop;
        public bool IsRequested => _stop;
        public string Reason { get; private set; }
        public void RequestStop(string reason)
        {
            _stop = true;
            Reason = reason;    
        }

    }
}

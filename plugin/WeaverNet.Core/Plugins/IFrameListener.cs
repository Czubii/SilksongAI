using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Plugins
{
    public interface IFrameListener
    {
        void OnFrameCaptured();
    }
}

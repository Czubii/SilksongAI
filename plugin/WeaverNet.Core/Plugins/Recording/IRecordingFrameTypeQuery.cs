using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Plugins.Recording
{
    public interface IRecordingFrameTypeQuery
    {
        bool TryGet(string id, out RecordingFrameTypeDefinition definition);
    }
}

using System.Collections.Generic;

namespace WeaverNet.Core.DataCollection
{
    public interface IRaycastScanner //TODO: integrate into recording
    {
        bool TryScan(int Mask, out IReadOnlyList<RayData> rays);
    }
}

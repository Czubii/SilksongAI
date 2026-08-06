using System.Collections.Generic;


namespace WeaverNet.Core.DataCollection
{
    public interface IRaycastScanner
    {
        bool TryScan(int Mask, out IReadOnlyList<RayData> rays);
    }
}

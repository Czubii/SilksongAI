using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.DataCollection
{
    public interface IRaycastScanner
    {
        bool TryScanTerrain(out IReadOnlyList<RayData> rays);
        bool TryScan(int Mask, out IReadOnlyList<RayData> rays);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public class SessionContext
    {
        public class SessionSettings
        {
            public readonly bool KeepAbilities;
            public readonly bool KeepTools;
            public readonly bool ReturnToBench;

            public SessionSettings() 
            {
                KeepAbilities = false;
                KeepTools = false;
                ReturnToBench = true;
            }
            public SessionSettings(bool keepAbilities = false, bool keepTools = false, bool returnToBench = true)
            {
                KeepAbilities = keepAbilities;
                KeepTools = keepTools;
                ReturnToBench = returnToBench;
            }
        }

        public readonly BossMetadata Boss;
        public readonly int TotalFights;
        public readonly SessionSettings Settings;

        public SessionContext(BossMetadata boss, int totalFights, SessionSettings settings = default)
        {
            Boss = boss;
            TotalFights = totalFights;
            Settings = settings;
        }
    }
}

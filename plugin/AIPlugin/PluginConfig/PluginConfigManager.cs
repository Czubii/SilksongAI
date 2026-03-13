using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.PluginConfig
{
    public static class PluginConfigManager
    {
        public static void Bind(ConfigFile configFile)
        {
            ConfigEntries.Rewards.Bind(configFile);
            ConfigEntries.KeyBinds.Bind(configFile);
        }
    }
}

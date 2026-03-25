using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.PluginGUI
{
    public class EnemyTrackerScreenLabel: BaseScreenLabel
    {
        private static readonly GUIStyle _style = new GUIStyle
        {
            fontSize = 18,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft
        };

        private ConfigFile _configFile;
        private ConfigEntry<bool> _enabledInConfig;

        public void Initialize(ConfigFile config)
        {
            _enabledInConfig = config.Bind("Labels", "Show enemies", true);
        }
        public override bool EnabledInConfig() => _enabledInConfig.Value;
        private void OnGUI()
        {
            int numEnemies = EnemyTracker.GetEnemyCount();
            int labelEnemiesStartY = Screen.height - ((numEnemies + 2) * _lineOffsetY);
            Rect labelEnemiesRect0 = new Rect(20, labelEnemiesStartY, 180, 9);


            if (numEnemies > 0)
                GUI.Label(labelEnemiesRect0, $"Enemies: ", _style);
            else
                GUI.Label(labelEnemiesRect0, $"Enemies: None", _style);

            foreach (var enemy in EnemyTracker.GetAllEnemies())
            {
                labelEnemiesRect0.y += _lineOffsetY;

                GUI.Label(labelEnemiesRect0, $"{enemy.Name}", _style);

            }

            int numDmgSources = EnemyTracker.GetDmgSourceCount();
            int labelDmgStartY = Screen.height - ((numDmgSources + numEnemies + 4) * _lineOffsetY);
            Rect labelDmgRect0 = new Rect(20, labelDmgStartY, 180, 9);

            if (numDmgSources > 0)
                GUI.Label(labelDmgRect0, $"Damage Sources: ", _style);
            else
                GUI.Label(labelDmgRect0, $"Damage Sources: None", _style);

            foreach (var dmg in EnemyTracker.GetAllDmgSources())
            {
                labelDmgRect0.y += _lineOffsetY;

                GUI.Label(labelDmgRect0, $"{dmg.Name}", _style);

            }
        }
    }
}

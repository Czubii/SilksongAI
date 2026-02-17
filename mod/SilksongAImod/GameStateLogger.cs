using BepInEx;
using HutongGames.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilksongAI
{
    public static class GameStateLogger
    {
        public static void DeleteGameStateLogFiles() //TODO implement
        {

        }
        public static void LogGameStateToFiles()
        {
            string path = Path.Combine(Paths.PluginPath, "SilksongAI", "Logs", $"session_{DateTime.Now:yyyyMMdd_HHmmss}");

            Directory.CreateDirectory(path);

            
            var enemies = GetDataUtils.GetAllEnemies();
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, "Enemies.txt")))
            {
                foreach (var enemy in enemies) 
                    outputFile.WriteLine(enemy.name);
            }

            var damageSources = GetDataUtils.GetAllDamageSources();
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, "Projectiles.txt")))
            {
                foreach (var damageSource in damageSources)
                    outputFile.WriteLine(damageSource.name);
            }
        }

    }
}

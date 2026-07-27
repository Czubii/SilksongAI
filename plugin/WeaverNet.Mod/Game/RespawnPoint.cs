using System;
using TeamCherry.SharedUtils;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game
{
    public class RespawnPoint : IDisposable, IRespawnPoint 
    {
        private RespawnMarker _marker;
        private string _scene;
        private GameObject _go;
        private bool _disposed;
        public RespawnPoint(string scene, Vector3 position, string name = "WeaverNetSpawnPoint")
        {
            _scene = scene;
            _go = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(_go);
            _marker = _go.AddComponent<RespawnMarker>();

            // Initialize minimal required fields
            _marker.customWakeUp = false;
            _marker.customFadeDuration = new OverrideFloat
            {
                Value = 0f,
                IsEnabled = false
            };

            _marker.transform.position = position;
            SceneTeleportMap.AddRespawnPoint(scene, name);
        }
        public void Dispose() // TODO this is smelly right now. we are removing the marker but not resetting the player data
        {
            if (_disposed) return;
            _disposed = true;

            if (_go != null)
            {
                UnityEngine.Object.Destroy(_go);
                _go = null;
            }

            _marker = null;

        }
        public void UseAsTemporary(TemporaryStateModifier modifier, int type = 0)
        {
            var pd = PlayerData.instance;
            if (pd == null)
            {
                PluginLog.Error("CustomRespawnPoint.UseAsTemporary(): no PlayerData.instance");
                return;
            }

            var prevMarkerName = pd.respawnMarkerName;
            var prewScene = pd.respawnScene;
            var prewType = pd.respawnType;

            pd.respawnMarkerName = _marker.name;
            pd.respawnScene = _scene;
            pd.respawnType = type;

            modifier.AddUndo(() =>
            {
                pd.respawnMarkerName = prevMarkerName;
                pd.respawnScene = prewScene;
                pd.respawnType = prewType;
            });
        }
        //public static void ResetTemporary() TODO remove when verify the new version works
        //{
        //    var pd = PlayerData.instance;
        //    if (pd == null)
        //    {
        //        PluginLog.Error("CustomRespawnPoint.ResetTemporary(): no PlayerData.instance");
        //        return;
        //    }

        //    pd.ResetTempRespawn();

        //}
    }
}

using TeamCherry.SharedUtils;
using UnityEngine;

namespace AIPlugin
{
    public class CustomRespawnPoint
    {
        private RespawnMarker _marker;
        private string _scene;
        private GameObject _go;
        private bool _disposed;
        public CustomRespawnPoint(string name, string scene, Vector3 position)
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
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            ResetTemporary();

            if (_go != null)
            {
                UnityEngine.Object.Destroy(_go);
                _go = null;
            }

            _marker = null;

        }
        public void UseAsTemporary(int type = 0)
        {
            var pd = PlayerData.instance;
            if (pd == null)
            {
                AIPlugin.Log.LogError("CustomRespawnPoint.UseAsTemporary(): no PlayerData.instance");
                return;
            }

            pd.tempRespawnMarker = _marker.name;
            pd.tempRespawnScene = _scene;
            pd.tempRespawnType = type;

        }
        public static void ResetTemporary()
        {
            var pd = PlayerData.instance;
            if (pd == null)
            {
                AIPlugin.Log.LogError("CustomRespawnPoint.ResetTemporary(): no PlayerData.instance");
                return;
            }

            pd.ResetTempRespawn();

        }
    }
}

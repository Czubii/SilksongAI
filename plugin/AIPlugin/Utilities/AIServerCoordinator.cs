using AIPlugin.Networking;
using Microsoft.Win32;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    /// <summary>
    /// This class manages the internal server settings like model selection
    /// </summary>
    public class AIServerCoordinator
    {
        private AiService _service;
        private Dictionary<string, List<string>> _serverModels = new Dictionary<string, List<string>>();
        private Tuple<string, string> _selectedModel = null;
        private Task _requestModelsTask = null;
        public AIServerCoordinator(AiService service) 
        { 
            _service = service;
            service.OnConnected += OnConnected; // TODO unsubsribe
            service.OnDisconnected += OnDisconnected;
        }
        private void OnConnected()
        { 
            _requestModelsTask = Task.Run(ReqestModels);
        }
        private void OnDisconnected()
        {
            _serverModels.Clear();
            _selectedModel = null;  
        }

        private async Task ReqestModels()
        {
            try
            {
                _serverModels = await _service.Gateway.ListModelsAsync();
            }
            finally
            {
                _requestModelsTask = null;
            }
            AIPlugin.Log.LogError(_serverModels.ToString());
        }

        public bool TryGetModels(string bossName, out List<string> models)
        {
            return _serverModels.TryGetValue(bossName, out models);
        }
        public string GetSelectedModelName()
        {
            if (_selectedModel == null) return null;
            return _selectedModel.Item2;
        }
        public void SelectModel(string bossName, string modelName)
        {
            _selectedModel = new Tuple<string, string>(bossName, modelName);
        }
        public void ClearModelSelection()
        {
            _selectedModel = null;
        }
        public void EnsureValidSelection(string bossName)
        {
            if (!TryGetModels(bossName, out var models) || models.Count == 0)
            {
                _selectedModel = null;
                return;
            }

            if (GetSelectedModelName() == null)
            {
                SelectModel(bossName, models[0]);
            }
        }
    }
}

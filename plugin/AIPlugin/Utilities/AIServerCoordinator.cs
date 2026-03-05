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
        private Dictionary<string, List<string>> _availableModels = new Dictionary<string, List<string>>();
        private (string BossName, string ModelName) _selectedModel = ("", "");
        private Task _requestModels = null;
        private Task _requestModelSelection = null;

        public AIServerCoordinator(AiService service) 
        { 
            _service = service;
            service.OnConnected += OnConnected; // TODO unsubsribe
            service.OnDisconnected += OnDisconnected;
        }
        private void OnConnected()
        { 
            _requestModels = Task.Run(RequestModels);
        }
        private void OnDisconnected()
        {
            _availableModels.Clear();
            _selectedModel = ("", "");  
        }

        private async Task RequestModels()
        {
            try
            {
                var response = await _service.Gateway.ListModelsAsync();
                if (response != null)
                {
                    _selectedModel = (response.SelectedModel[0], response.SelectedModel[1]);
                    _availableModels = response.Models;
                }
            }
            finally { _requestModels = null; }
        }

        private async Task RequestModelSelection(string bossName, string modelName)
        {
            try
            {
                var response = await _service.Gateway.SelectModel(bossName, modelName);
                if (response == null)
                {
                    _selectedModel = ("", "");
                    return;
                }

                _selectedModel = (response.SelectedModel[0], response.SelectedModel[1]);
                _availableModels = response.Models;

            }
            finally { _requestModelSelection = null; }
        }

        public List<string> GetAvailableModels(string bossName)
        {
            if (_availableModels.TryGetValue(bossName, out var models))
                return models;

            return new List<string>(); 
        }
        public string GetSelectedModelName()
        {
            return _selectedModel.ModelName;
        }
        public void SelectModel(string bossName, string modelName)
        {
            _selectedModel = (bossName, modelName);

            if (_requestModelSelection == null) _requestModelSelection = RequestModelSelection(bossName, modelName);
        }
        public void EnsureValidSelection(string bossName)
        {
            var models = GetAvailableModels(bossName);

            if (models.Count == 0 && (_selectedModel.BossName != "" || _selectedModel.ModelName != ""))
            {
                _selectedModel = ("","");
                SelectModel("", "");
                return;
            }

            if (models.Count != 0 && (_selectedModel.BossName == "" || _selectedModel.ModelName == ""))
            {
                SelectModel(bossName, models[0]);
            }
        }
    }
}

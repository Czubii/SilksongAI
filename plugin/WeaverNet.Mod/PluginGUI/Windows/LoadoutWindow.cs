using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Mod.PluginGUI.Elements;
using WeaverNet.Mod.PluginGUI.Styles;

namespace WeaverNet.Mod.PluginGUI.Windows
{
    public class LoadoutWindow : BaseWindow
    {
        private enum Views
        {
            Main,
            CreateFromCurrent,
            CreateFromExisting,
            Manage,
        }

        private Views _currentView = Views.Main;

        private readonly ILoadoutManager _loadoutManager;
        private readonly ILoadoutRepository _loadoutRepository;

        private string _newLoadoutName = "";

        private Vector2 _managerScroll = new Vector2(0,0);

        private IReadOnlyList<Loadout> _loadoutCache = new List<Loadout>();

        public LoadoutWindow(
            string name,
            ILoadoutManager loadoutManager,
            ILoadoutRepository loadoutRepository)
            : base(name, new Rect(0, 0, 250, 170))
        {
            _loadoutManager = loadoutManager;
            _loadoutRepository = loadoutRepository;
            loadoutRepository.RepositoryChanged += UpdateLoadoutCache;
            UpdateLoadoutCache();
        }

        public override bool CanEnable() => true;

        private void UpdateLoadoutCache()
        {
            _loadoutCache = _loadoutRepository.All.ToList();
        }
        public override void DrawContent()
        {
            switch (_currentView)
            {
                case Views.Main:
                    DrawMain();
                    break;

                case Views.CreateFromCurrent:
                    DrawCreateFromCurrent();
                    break;

                case Views.CreateFromExisting:
                    DrawCreateFromExisting();
                    break;

                case Views.Manage:
                    DrawManage();
                    break;

                default:
                    _currentView = Views.Main;
                    break;
            }
        }
        
        private void DrawMain()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Create New from Current Equipment", PluginGUIStyles.Button))
            {
                _newLoadoutName = "";
                _currentView = Views.CreateFromCurrent;
            }

            if (GUILayout.Button("Create New from Existing", PluginGUIStyles.Button))
            {
                _currentView = Views.CreateFromExisting;
            }

            if (GUILayout.Button("Manage Loadouts", PluginGUIStyles.Button))
            {
                _currentView = Views.Manage;
            }

            GUILayout.EndVertical();
        }

        private void DrawCreateFromCurrent()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Creating new loadout repository entry based on current player data");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name: ");
            _newLoadoutName = GUILayout.TextField(_newLoadoutName, PluginGUIStyles.TextField);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", PluginGUIStyles.Button))
            {
                _newLoadoutName = "";
                _currentView = Views.Main;
            }

            GUI.enabled = !string.IsNullOrWhiteSpace(_newLoadoutName);

            if (GUILayout.Button("Save", PluginGUIStyles.Button))
            {
                try
                {
                    var loadout = _loadoutManager.BuildLoadout(_newLoadoutName);

                    _loadoutRepository.Add(loadout);

                    _newLoadoutName = "";
                    Notify("Loadout saved successfully");
                }
                catch (Exception ex)
                {
                    NotifyError(ex.Message);
                }

            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawCreateFromExisting()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Not implemented yet.");

            if (GUILayout.Button("Back", PluginGUIStyles.Button))
            {
                _currentView = Views.Main;
            }

            GUILayout.EndVertical();
        }

        private void DrawManage()
        {

         
            GUILayout.BeginVertical();
            _managerScroll = PluginGUIElements.BeginScrollView(_managerScroll);

            if (_loadoutCache.Count > 0)
            {
                foreach (var loadout in _loadoutCache)
                {
                    DrawManagerEntry(loadout);
                }
            }
            else
            {
                GUILayout.Label("No loadouts!");
            }

            GUILayout.EndScrollView();
            if (GUILayout.Button("Back", PluginGUIStyles.Button))
            {
                _currentView = Views.Main;
            }

            GUILayout.EndVertical();
        }
        private void DrawManagerEntry(Loadout entry)
        {
            GUILayout.BeginVertical(PluginGUIStyles.Card);
            GUILayout.Label(entry.Name, PluginGUIStyles.HeaderLabel);
            GUILayout.BeginHorizontal();
            try
            {
                if (GUILayout.Button("Apply", PluginGUIStyles.Button))
                {
                    _loadoutManager.SetLoadout(entry);
                    Notify($"Loadout \"{entry.Name}\" applied successfully!");
                }
                if (GUILayout.Button("Delete", PluginGUIStyles.Button))
                {
                    _loadoutRepository.Remove(entry.Name);
                    Notify($"Loadout \"{entry.Name}\" removed successfully!");
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex.Message);
                throw (ex);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}

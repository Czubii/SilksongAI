using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    public class LoadoutWindow : MultiViewWindow
    {

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

            AddView("Main", DrawMain);
            AddView("CreateFromCurrent", DrawCreateFromCurrent);
            AddView("Manager", DrawManager);
        }

        public override bool CanEnable() => true;

        private void UpdateLoadoutCache()
        {
            _loadoutCache = _loadoutRepository.All.ToList();
        }
        
        private void DrawMain()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Create New from Current Equipment", WeaverNetStyles.Button))
            {
                _newLoadoutName = "";
                SwitchView("CreateFromCurrent");
            }
            if (GUILayout.Button("Manage Loadouts", WeaverNetStyles.Button))
            {
                SwitchView("Manager");
            }

            GUILayout.EndVertical();
        }

        private void DrawCreateFromCurrent()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Creating new loadout repository entry based on current player data");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name: ");
            _newLoadoutName = GUILayout.TextField(_newLoadoutName, WeaverNetStyles.TextField);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", WeaverNetStyles.Button))
            {
                _newLoadoutName = "";
                SwitchView("Main");
            }

            GUI.enabled = !string.IsNullOrWhiteSpace(_newLoadoutName);

            if (GUILayout.Button("Save", WeaverNetStyles.Button))
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
        private void DrawManager()
        {
            GUILayout.BeginVertical();
            _managerScroll = Elements.PluginGUI.BeginScrollView(_managerScroll);

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
            if (GUILayout.Button("Back", WeaverNetStyles.Button))
            {
                SwitchView("Main");
            }

            GUILayout.EndVertical();
        }
        private void DrawManagerEntry(Loadout entry)
        {
            GUILayout.BeginVertical(WeaverNetStyles.Card);
            GUILayout.Label(entry.Name, WeaverNetStyles.WindowTitleLabel);
            GUILayout.BeginHorizontal();
            try
            {
                if (GUILayout.Button("Apply", WeaverNetStyles.Button))
                {
                    _loadoutManager.SetLoadout(entry);
                    Notify($"Loadout \"{entry.Name}\" applied successfully!");
                }
                if (GUILayout.Button("Delete", WeaverNetStyles.Button))
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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Popups;
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
            : base(name, new Rect(0, 0, 250, 600))
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

            _newLoadoutName = PluginGUI.Labeled("Name",
                () => PluginGUI.TextField(_newLoadoutName),
                "Name for new loadout repository entry based on current player data");

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", WeaverNetStyles.Button))
            {
                _newLoadoutName = "";
                SwitchView("Main");
            }
            GUILayout.Space(10);
            GUI.enabled = !string.IsNullOrWhiteSpace(_newLoadoutName);

            if (GUILayout.Button("Save", WeaverNetStyles.Button))
            {
                try
                {
                    var loadout = _loadoutManager.BuildLoadout(_newLoadoutName);

                    _loadoutRepository.Add(loadout);
                    var notificationPopup = new NotificationPopup(WindowRect, "Success", $"Loadout {_newLoadoutName} saved successfully!");
                    Context.ShowPopup(notificationPopup, this);
                    _newLoadoutName = "";
                }
                catch (Exception ex)
                {
                    var errorPopup = new ErrorPopup(WindowRect, "Error", $"Exception met while saving the loadout: \n {ex.Message}");
                    ShowPopup(errorPopup);
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
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", WeaverNetStyles.Button))
            {
                SwitchView("Main");
            }

            GUILayout.EndVertical();
        }
        private void DrawManagerEntry(Loadout entry)
        {
            GUILayout.BeginVertical(WeaverNetStyles.Card);
            GUILayout.Label(entry.Name, WeaverNetStyles.ElementLabelTitle);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            try
            {
                if (GUILayout.Button("Apply", WeaverNetStyles.Button))
                {
                    _loadoutManager.SetLoadout(entry);

                    var notificationPopup = new NotificationPopup(WindowRect, "Success", $"Loadout \"{entry.Name}\" applied successfully!");
                    ShowPopup(notificationPopup);
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Delete", WeaverNetStyles.Button))
                {
                    var confirmPopup = new ConfirmPopup(WindowRect, 
                        "Confirm Action", 
                        $"Are you sure you want to remove loadout \"{entry.Name}\"? (This action cannot be undone)", 
                        () => _loadoutRepository.Remove(entry.Name));
                    ShowPopup(confirmPopup);
                }
            }
            catch (Exception ex)
            {
                var errorPopup = new ErrorPopup(WindowRect, "Error", $"Exception met while saving the loadout {ex.Message}");
                ShowPopup(errorPopup);
                throw (ex);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}

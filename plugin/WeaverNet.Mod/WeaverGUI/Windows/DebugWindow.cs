using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Mod.Game;
using WeaverNet.Mod.Game.Debug;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;
using WeaverNet.Diagnostics.Extensions;
using WeaverNet.Core.Game;
using WeaverNet.Mod.Game.Player;
using WeaverNet.Mod.DataCollection;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    public class DebugWindow : BaseWindow
    {
        private HitboxVisualizer HitboxVisualizer { get; }
        private RaycastScannerVisualizer RayCastVisualizer { get; }
        private ILoadoutManager LoadoutManager { get; }
        private Loadout _loadout = null;

        private FoldableState _raycastLayersFoldableState = new FoldableState();

        private enum RaycastMaskBuilderType
        {
            Custom,
            Preset
        }

        private static readonly RaycastMaskBuilderType[] MaskBuilderOptions =
            (RaycastMaskBuilderType[])Enum.GetValues(typeof(RaycastMaskBuilderType));

        private static readonly List<string> PresetNames = PresetLayerMasks.AllPresets.Keys.ToList();

        private DropdownState<RaycastMaskBuilderType> _builderTypeDropdownState = new DropdownState<RaycastMaskBuilderType>();
        private DropdownState<string> _presetDropdownState = new DropdownState<string>();

        public DebugWindow(string name, HitboxVisualizer hitbox, RaycastScannerVisualizer rayCastVisualizer)
            : base(name, new Rect(0, 0, 200, 150))
        {
            HitboxVisualizer = hitbox;
            RayCastVisualizer = rayCastVisualizer;
        }

        public override bool CanEnable() => true;

        protected override void DrawContent()
        {
            GUILayout.BeginVertical();

            HitboxVisualizer.enabled = PluginGUI.Labeled("Hitboxes",
                () => PluginGUI.Toggle(HitboxVisualizer.enabled),
                "Draw Hitboxes");

            RayCastVisualizer.enabled = PluginGUI.Labeled("Raycasts",
                () => PluginGUI.Toggle(RayCastVisualizer.enabled),
                "Draw raycasts");


            // Toggle group / Foldout for Raycast Layers
            _raycastLayersFoldableState = PluginGUI.Foldable(
                _raycastLayersFoldableState,
                "Raycast Layers",
                DrawRaycastLayerOptions,
                300.0f);

            if (PluginGUI.Labeled("Print Hitbox Layers", () => PluginGUI.Button("Print")))
            {
                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        PluginLog.Info($"Layer {i}: '{layerName}'");
                    }
                }
            }

            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        private void DrawRaycastLayerOptions()
        {
            _builderTypeDropdownState = PluginGUI.Labeled("Layer Mask Type",
                () => PluginGUI.Dropdown(Context, _builderTypeDropdownState, MaskBuilderOptions, a => a.ToString()));

            switch (_builderTypeDropdownState.SelectedOption)
            {
                case RaycastMaskBuilderType.Custom:
                    DrawCustomRaycastBuildingOptions();
                    break;

                case RaycastMaskBuilderType.Preset:
                    DrawPresetRaycastBuilingOptions();
                    break;
            }
        }

        private void DrawPresetRaycastBuilingOptions()
        {
            GUILayout.BeginVertical();

            _presetDropdownState = PluginGUI.Labeled("Select Preset",
                () => PluginGUI.Dropdown(Context, _presetDropdownState, PresetNames, name => name));

            if (PluginGUI.Labeled("Apply Preset", () => PluginGUI.Button("Apply")))
            {
                ApplySelectedPreset();
            }

            GUILayout.EndVertical();
        }

        private void ApplySelectedPreset()
        {
            string selectedPresetName = _presetDropdownState.SelectedOption;

            // Fallback if null or uninitialized
            if (string.IsNullOrEmpty(selectedPresetName) && PresetNames.Count > 0)
            {
                selectedPresetName = PresetNames[0];
            }

            if (!string.IsNullOrEmpty(selectedPresetName) &&
                PresetLayerMasks.AllPresets.TryGetValue(selectedPresetName, out int selectedMask))
            {
                RayCastVisualizer.SetMask(selectedMask);
            }
        }

        private void DrawCustomRaycastBuildingOptions()
        {
            int currentMask = RayCastVisualizer.Mask;
            int newMask = currentMask;

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName)) continue;

                bool isLayerEnabled = (currentMask & (1 << i)) != 0;

                bool toggled = PluginGUI.Labeled(layerName,
                    () => PluginGUI.Toggle(isLayerEnabled));

                if (toggled != isLayerEnabled)
                {
                    if (toggled)
                        newMask |= (1 << i);
                    else
                        newMask &= ~(1 << i);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (newMask != currentMask)
            {
                RayCastVisualizer.SetMask(newMask);
            }
        }
    }
}
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    internal class DatasetCreatorWindow : BaseWindow
    {
        private class State
        {
            public DropdownState<BossData> BossDropdownState = new DropdownState<BossData>();
            public DropdownState<Loadout> LoadoutDropdownState = new DropdownState<Loadout>();
            public int RecordingCount = 0;
        }
        private readonly IRecordingCatalog _recordingCatalog;
        private readonly IRecordingRepositoryQuery _recordingQuery;
        private readonly State _state = new State();
        public DatasetCreatorWindow(
            IRecordingCatalog recordingCatalog, 
            IRecordingRepositoryQuery recordingQuery) : 
            base("Dataset Creator", new Rect(300, 300, 300, 300))
        {
            _recordingQuery = recordingQuery;
            _recordingCatalog = recordingCatalog;
        }
        public override bool CanEnable() => true;

        protected override void DrawContent()
        {
            var oldBossState = _state.BossDropdownState;
            var oldLoadoutState = _state.LoadoutDropdownState;
            _state.BossDropdownState = PluginGUI.Labeled("Boss",
                () => PluginGUI.Dropdown(
                        Context,
                        _state.BossDropdownState,
                        _recordingCatalog.DistinctBosses,
                        boss => boss.DisplayName),
                "Select the boss for new dataset");

            _state.LoadoutDropdownState = PluginGUI.Labeled("Loadout",
                () => PluginGUI.Dropdown(
                        Context,
                        _state.LoadoutDropdownState,
                        _recordingCatalog.DistinctLoadouts,
                        obj => obj.Name),
                "Select the loadout for new dataset");

            bool bossChanged = !Equals(oldBossState?.SelectedOption, _state.BossDropdownState?.SelectedOption);
            bool loadoutChanged = !Equals(oldLoadoutState?.SelectedOption, _state.LoadoutDropdownState?.SelectedOption);

            if (bossChanged || loadoutChanged)
            {
                PluginLog.Warning("Changing Option");
                _state.RecordingCount = 
                    _recordingQuery.CountRecordings(
                        _state.BossDropdownState.SelectedOption.Id, 
                        _state.LoadoutDropdownState.SelectedOption);
            }
            PluginGUI.Label($"Recordings matching the filters: {_state.RecordingCount}", WeaverGUI.Styles.WeaverNetStyles.ElementLabelTitle);
        }
    }
}

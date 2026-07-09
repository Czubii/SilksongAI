using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game.Bosses
{
    public class DataDrivenBossBehavior: IBossBehavior
    {
        private readonly RespawnFlags _flags;
        public DataDrivenBossBehavior(RespawnFlags flags)
        {
            _flags = flags;
        }
        public void Respawn()
        {
            var pd = PlayerData.instance;
            foreach(var playerFlag in _flags.PlayerDataBools)
            {
                pd.SetBool(playerFlag.Key, playerFlag.Value);
            }

            var sd = SceneData.instance;
            foreach(var sceneFlag in _flags.SceneBools)
            {
                sd.PersistentBools.SetValue(new PersistentItemData<bool>
                {
                    SceneName = sceneFlag.SceneName,
                    ID = sceneFlag.ID,
                    Value = sceneFlag.Value
                });
            }
            foreach (var sceneInt in _flags.SceneInts)
            {
                sd.PersistentInts.SetValue(new PersistentItemData<int>
                {
                    SceneName = sceneInt.SceneName,
                    ID = sceneInt.ID,
                    Value = sceneInt.Value,
                    //Mutator = sceneInt.Mutator
                });
            }
        }
        public void RespawnTemporary(TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            foreach (var playerFlag in _flags.PlayerDataBools)
            {
                var currentValue = pd.GetBool(playerFlag.Key);
                pd.SetBool(playerFlag.Key, playerFlag.Value);
                modifier.AddUndo(() =>
                {
                    pd.SetBool(playerFlag.Key, currentValue);
                });
            }

            var sd = SceneData.instance;
            foreach (var sceneFlag in _flags.SceneBools)
            {
                var currentValue = sd.PersistentBools.GetValueOrDefault(sceneFlag.SceneName, sceneFlag.ID);

                sd.PersistentBools.SetValue(new PersistentItemData<bool>
                {
                    SceneName = sceneFlag.SceneName,
                    ID = sceneFlag.ID,
                    Value = sceneFlag.Value
                });

                modifier.AddUndo(() =>
                {
                    sd.PersistentBools.SetValue(new PersistentItemData<bool>
                    {
                        SceneName = sceneFlag.SceneName,
                        ID = sceneFlag.ID,
                        Value = currentValue
                    });
                });
            }

            foreach (var sceneInt in _flags.SceneInts)
            {
                var currentValue = sd.PersistentInts.GetValueOrDefault(sceneInt.SceneName, sceneInt.ID);

                sd.PersistentInts.SetValue(new PersistentItemData<int>
                {
                    SceneName = sceneInt.SceneName,
                    ID = sceneInt.ID,
                    Value = sceneInt.Value
                    //Mutator = sceneInt.Mutator
                });

                modifier.AddUndo(() =>
                {
                    SceneData.instance.PersistentInts.SetValue(new PersistentItemData<int>
                    {
                        SceneName = sceneInt.SceneName,
                        ID = sceneInt.ID,
                        Value = currentValue
                        //Mutator = sceneInt.Mutator TODO: take a closer look at those 
                    });
                });
            }
        }
    }
}

using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game.Bosses
{
    public class BossSpawner: IBossSpawner
    {
        public void Respawn(BossRespawnFlags flags)
        {
            var pd = PlayerData.instance;
            foreach(var playerFlag in flags.PlayerDataBools)
            {
                pd.SetBool(playerFlag.Key, playerFlag.Value);
            }

            var sd = SceneData.instance;
            foreach(var sceneFlag in flags.SceneBools)
            {
                sd.PersistentBools.SetValue(new PersistentItemData<bool>
                {
                    SceneName = sceneFlag.SceneName,
                    ID = sceneFlag.ID,
                    Value = sceneFlag.Value
                });
            }
            foreach (var sceneInt in flags.SceneInts)
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
        public void RespawnTemporary(BossRespawnFlags flags, TemporaryStateModifier modifier)
        {
            var pd = PlayerData.instance;
            foreach (var playerFlag in flags.PlayerDataBools)
            {
                var currentValue = pd.GetBool(playerFlag.Key);
                pd.SetBool(playerFlag.Key, playerFlag.Value);
                modifier.AddUndo(() =>
                {
                    pd.SetBool(playerFlag.Key, currentValue);
                });
            }

            var sd = SceneData.instance;
            foreach (var sceneFlag in flags.SceneBools)
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

            foreach (var sceneInt in flags.SceneInts)
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

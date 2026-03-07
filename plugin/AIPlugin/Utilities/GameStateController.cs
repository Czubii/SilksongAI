using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{
    /// <summary>
    /// Allows to safely modify some variables and restore them to previous state once we are done
    /// </summary>
    public class TemporaryStateModifier
    {
        private List<Action> _undoActions = new List<Action>();
        public void ModifyVar<T>(Action<T> setter, T originalValue, T newValue)
        {
            _undoActions.Add(() => setter(originalValue));
            setter(newValue);
        }
        public void AddUndo(Action action)
        {
            _undoActions.Add(action);
        }
        public void RestoreState()
        {
            for (int i = _undoActions.Count - 1; i >= 0; i--) // loop backwards to get to the first state
                _undoActions[i].Invoke();
        }
    }
    public class GameStateScope: IDisposable
    {
        public TemporaryStateModifier Modifier;

        public GameStateScope()
        {
            Modifier = new TemporaryStateModifier();
        }

        public void Dispose()
        {
            Modifier.RestoreState();
        }
    }
    public class GameStateController
    {
        public void SetFullHP()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillHealthToMax();
        }

        public void SetFullSilk()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillSilkToMaxSilent();

        }
        public void RemoveCocoon()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("GameStateController.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.CocoonBroken();
            AIPlugin.Log.LogDebug("Cocoon Removed");
        }

        public IEnumerator AwaitCocoonAndRemove() 
        {
            yield return new WaitUntil(() =>
            {
                var pd = PlayerData.instance;

                if (pd == null) return false;

                return pd.HeroCorpseMarkerGuid != null;
            });

            RemoveCocoon();
        }
    }
}

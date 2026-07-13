using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure
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
        public void RestoreState() // TODO: add here safe guard to automatically restore the state either before saving or quiting to menu
        {
            for (int i = _undoActions.Count - 1; i >= 0; i--) // loop backwards to get to the first state
                _undoActions[i].Invoke();

            _undoActions.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.DataCollection.Recording
{
    public class FsmObservation
    {
        public string FsmName { get; }
        public string ActiveStateName { get; }
        public FsmObservation(string fsmName, string activeStateName)
        {
            FsmName = fsmName;
            ActiveStateName = activeStateName;
        }
    }
}

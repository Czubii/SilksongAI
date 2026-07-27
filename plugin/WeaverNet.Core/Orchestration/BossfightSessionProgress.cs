using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionProgress
    {
        public string Message { get; }
        public float? Percentage { get; }
        public BossfightSessionProgress(string message, float? percentage)
        {
            Message = message;
            Percentage = percentage;
        }
    }
}

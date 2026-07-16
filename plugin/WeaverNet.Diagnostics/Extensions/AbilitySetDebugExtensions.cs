using System.Diagnostics;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Diagnostics.Extensions
{
    public static class AbilitySetDebugExtensions
    {
        public static string ToDebugString(this AbilitySet abilities)
        {
            return string.Format(
                   "Abilities:\n" +
                   "  Dash:        {0}\n" +
                   "  DoubleJump:  {1}\n" +
                   "  WallJump:    {2}\n" +
                   "  HarpoonDash: {3}\n" +
                   "  SuperJump:   {4}\n" +
                   "  Brolly:      {5}\n" +
                   "  ChargeSlash: {6}\n" +
                   "  Needolin:    {7}\n" +
                   "  Sylphsong:   {8}",
                   abilities.Dash,
                   abilities.DoubleJump,
                   abilities.WallJump,
                   abilities.HarpoonDash,
                   abilities.SuperJump,
                   abilities.Brolly,
                   abilities.ChargeSlash,
                   abilities.Needolin,
                   abilities.Sylphsong);
        }

        [Conditional("DEBUG")]
        public static void Print(this AbilitySet abilities)
        {
            PluginLog.Info(abilities.ToDebugString());
        }
    }
}
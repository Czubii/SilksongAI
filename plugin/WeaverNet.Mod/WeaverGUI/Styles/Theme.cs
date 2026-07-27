using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Styles
{
    public static partial class WeaverNetStyles
    {
        private static class Theme
        {
            // Neutral surfaces
            public static readonly Color Background = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            public static readonly Color Surface = new Color(0.125f, 0.125f, 0.125f);
            public static readonly Color SurfaceRaised = new Color(0.15f, 0.15f, 0.15f);

            public static readonly Color BackgroundTranslucent = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            // Interactive neutral states
            public static readonly Color Control = new Color(0.18f, 0.18f, 0.18f);
            public static readonly Color ControlHover = new Color(0.23f, 0.23f, 0.23f);
            public static readonly Color ControlPressed = new Color(0.30f, 0.30f, 0.30f);

            // Accent (enabled / selected / primary actions)
            public static readonly Color Primary = new Color(0.333f, 0.243f, 0.639f);
            public static readonly Color PrimaryHover = new Color(0.295f, 0.20f, 0.59f);
            public static readonly Color PrimaryPressed = new Color(0.266f, 0.184f, 0.537f);

            // Destructive actions
            public static readonly Color Danger = new Color(0.91f, 0.20f, 0.15f);
            public static readonly Color DangerHover = new Color(0.96f, 0.28f, 0.22f);
            public static readonly Color DangerPressed = new Color(0.75f, 0.08f, 0.08f);

            // Text
            public static readonly Color Text = new Color(0.86f, 0.86f, 0.86f);
            public static readonly Color TextMuted = new Color(0.62f, 0.62f, 0.62f);
            public static readonly Color TextDisabled = new Color(0.42f, 0.42f, 0.42f);

            // Optional outlines
            public static readonly Color Border = new Color(0.23f, 0.23f, 0.23f);
            public static readonly Color BorderAccent = Primary;
        }
    }
}

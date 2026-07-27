using System.Text;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public readonly struct IntFieldState
    {
        public string Text { get; }
        public int Value { get; }

        public IntFieldState(int value)
        {
            Value = value;
            Text = value.ToString();
        }

        internal IntFieldState(string text, int value)
        {
            Text = text;
            Value = value;
        }
    }

    public static partial class PluginGUI
    {
        private static string FilterInteger(string input, bool allowNegative = true)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (char.IsDigit(c))
                {
                    result.Append(c);
                }
                else if (allowNegative && c == '-' && i == 0)
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public static IntFieldState IntField(
       IntFieldState state,
       params GUILayoutOption[] options)
        {
            string text = TextField(state.Text, options);

            text = FilterInteger(text);

            int value = state.Value;

            if (int.TryParse(text, out int parsed))
                value = parsed;

            return new IntFieldState(text, value);
        }

        public static IntFieldState IntField(
            IntFieldState state,
            int min,
            int max,
            params GUILayoutOption[] options)
        {
            string text = TextField(state.Text, options);

            text = FilterInteger(text);

            int value = state.Value;

            if (int.TryParse(text, out int parsed))
            {
                value = Mathf.Clamp(parsed, min, max);
                text = value.ToString();
            }

            return new IntFieldState(text, value);
        }

        public static string TextField(
            string value,
            params GUILayoutOption[] options)
        {

            value = GUILayout.TextField(
                value,
                WeaverNetStyles.TextField,
                MergeOptions(
                    GUILayout.Width(150),
                    options));

            return value;
        }
    }
}
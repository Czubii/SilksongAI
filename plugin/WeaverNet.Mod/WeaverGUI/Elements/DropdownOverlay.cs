using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public class DropdownOverlay<T> : IGUIOverlay
    {
        private readonly IDropdownState<T> _state;
        private readonly IReadOnlyList<T> _elements;
        private readonly Func<T, string> _labelSelector;
        private readonly Action<(bool SelectionChanged, int SelectedIdx)> _onClose;
        public bool IsAlive => _state.Expanded;

        public DropdownOverlay(
            IDropdownState<T> state,
            IReadOnlyList<T> elements,
            Func<T, string> labelSelector,
            Action<(bool SelectionChanged, int SelectedIdx)> onClose)
        {
            _state = state;
            _elements = elements;
            _labelSelector = labelSelector;
        }


        public void Draw()
        {
            const float optionHeight = 30f;

            Rect rect = new Rect(
                _state.ScreenButtonRect.x,
                _state.ScreenButtonRect.yMax,
                _state.ScreenButtonRect.width,
                optionHeight * _elements.Count);


            GUI.Box(
                rect,
                GUIContent.none,
                WeaverNetStyles.DropdownOverlayBacground);


            for (int i = 0; i < _elements.Count; i++)
            {
                Rect optionRect = new Rect(
                    rect.x,
                    rect.y + i * optionHeight,
                    rect.width,
                    optionHeight);


                if (GUI.Button(
                    optionRect,
                    _labelSelector(_elements[i]),
                    i == _state.SelectedIdx
                        ? WeaverNetStyles.AccentButton
                        : WeaverNetStyles.Button))
                {
                    SwitchSelection(i);
                }
            }


            // Close when clicking outside
            if (Event.current.type == EventType.MouseDown &&
                !rect.Contains(Event.current.mousePosition))
            {
                Close();
                Event.current.Use();
            }
        }
        private void Close()
        {
            _onClose?.Invoke((false, _state.SelectedIdx));
        }
        private void SwitchSelection(int newIndex)
        {
            if (newIndex >= 0 && newIndex < _elements.Count)
            {
                _onClose?.Invoke((true, newIndex));
            }
        }
    }
}

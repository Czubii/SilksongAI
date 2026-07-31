using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Elements
{
    public class DropdownOverlay<T> : IGUIOverlay
    {
        private readonly DropdownState<T> _state;
        private readonly IReadOnlyList<T> _elements;
        private readonly Func<T, string> _labelSelector;

        public bool IsAlive => _state.Expanded;

        public DropdownOverlay(
            DropdownState<T> state,
            IReadOnlyList<T> elements,
            Func<T, string> labelSelector)
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
                    _state.SelectedIdx = i;
                    _state.Expanded = false;
                }
            }


            // Close when clicking outside
            if (Event.current.type == EventType.MouseDown &&
                !rect.Contains(Event.current.mousePosition))
            {
                _state.Expanded = false;
                Event.current.Use();
            }
        }
    }
}

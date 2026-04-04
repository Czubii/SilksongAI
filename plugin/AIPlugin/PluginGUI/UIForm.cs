using AIPlugin.Networking;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AIPlugin.PluginGUI.CustomGUI;

namespace AIPlugin.PluginGUI
{
    public interface IForm
    {
        bool IsValid();
    }
    public class EmptyForm: IForm { public bool IsValid() => true; }
    static class BindingCache<TModel>
    {
        private static Dictionary<MemberInfo, object> _cache = new Dictionary<MemberInfo, object>();

        public static (Func<TModel, TValue> getter, Action<TModel, TValue> setter)
        Get<TValue>(Expression<Func<TModel, TValue>> expr)
        {
            var body = expr.Body is UnaryExpression u ? u.Operand : expr.Body;
            var member = (MemberExpression)body;
            var key = member.Member;

            if (_cache.TryGetValue(key, out var value))
            {
                return ((Func<TModel, TValue>, Action<TModel, TValue>))value;
            }

            var compiled = Compile(expr);
            _cache[key] = compiled;

            return compiled;
        }

        private static (Func<TModel, TValue> getter, Action<TModel, TValue> setter)
        Compile<TValue>(Expression<Func<TModel, TValue>> expr)
        {
            var getter = expr.Compile();

            var paramModel = Expression.Parameter(typeof(TModel));
            var paramValue = Expression.Parameter(typeof(TValue));

            var member = (MemberExpression)expr.Body;

            var assign = Expression.Assign(Expression.MakeMemberAccess(paramModel, member.Member), paramValue);

            var setter = Expression.Lambda<Action<TModel, TValue>>(assign, paramModel, paramValue).Compile();

            return (getter, setter);
        }
    }
    public static class UI
    {
        public static UIForm<T> Form<T>(T model)
            where T : IForm
        {
            return new UIForm<T>(model);
        }
    }
    public class UIForm<T>
        where T : IForm
    {
        private T _model;
        public UIForm(T model)
        {
            _model = model;
            GUILayout.BeginVertical();
        }
        public UIForm<T> Toggle(string label, Expression<Func<T, bool>> expr)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            Labeled(label, 
                () => value = GUILayout.Toggle(value, "", Styles.Toggle));

            set(_model, value);

            return this;
        }
        public UIForm<T> Dropdown<TDropdown>(string label,
            List<(string label, TDropdown value)> elements,
            Expression<Func<T, DropdownState<TDropdown>>> expr,
            Action onChanged = null)
            where TDropdown : class
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            value = CustomGUI.Dropdown(value, elements, label);

            set(_model, value);

            if (value.SelectionChanged) onChanged?.Invoke();

            return this;
        }
        public UIForm<T> IntegerField(string label, Expression<Func<T, int>> expr, params GUILayoutOption[] options)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            Labeled(label,
                () => value = CustomGUI.IntegerField(value, options));

            set(_model, value);

            return this;
        }
        public UIForm<T> TextField(string label, Expression<Func<T, string>> expr, params GUILayoutOption[] options)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            Labeled(label,
                () => value = GUILayout.TextField(value, Styles.TextField, options));

            set(_model, value);

            return this;
        }
        public UIForm<T> HorizontalSlider(string label, float min, float max, float step, 
            Expression<Func<T, float>> expr, 
            params GUILayoutOption[] labelOptions)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelOptions);
            value = GUILayout.HorizontalSlider(value, min, max, Styles.SliderTrack, Styles.SliderThumb);
            GUILayout.EndHorizontal();

            if (step != 0.0f) value = Mathf.Ceil(value / step) * step;

            set(_model, value);

            return this;
        }
        public UIForm<T> ParamListField(Expression<Func<T, List<ServerParam>>> expr, params GUILayoutOption[] options)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            for (int i = 0; i < value.Count; i++)
            {
                value[i] = CustomGUI.ParamField(value[i], options);
            }

            set(_model, value);

            return this;
        }
        public UIForm<T> Label(string label, GUIStyle style = null)
        {
            if (style == null) style = GUI.skin.label;

            GUILayout.Label(label, style);
            return this;
        }
        public UIForm<T> Space(float pixels)
        {
            GUILayout.Space(pixels);
            return this;
        }
        public UIForm<T> BeginCard(string cardLabel, GUIStyle style)
        {
            GUILayout.BeginVertical(style);
            GUILayout.Label(cardLabel, Styles.HeaderLabel);
            return this;    
        }
        public UIForm<T> EndCard()
        {
            GUILayout.EndVertical();
            return this;
        }

        public UIForm<T> BeginScrollView(Expression<Func<T, Vector2>> expr)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var scroll = get(_model);

            scroll = GUILayout.BeginScrollView(scroll, Styles.ScrollView, Styles.VerticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.skin.verticalScrollbarThumb = Styles.VerticalScrollbarThumb;

            set(_model, scroll);

            return this;
        }
        public UIForm<T> EndScrollView()
        {
            GUILayout.EndScrollView();
            return this;
        }
        public UIForm<T> FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
            return this;
        }
        public UIForm<T> Button(string label, Action OnPress, bool enabled = true)
        {
            GUI.enabled = enabled && GUI.enabled;
            if(GUILayout.Button(label, Styles.Button))
                OnPress();
            GUI.enabled = true;
            return this;
        }
        public UIForm<T> GUIEnabled(bool enabled)
        {
            GUI.enabled = enabled;
            return this;
        }
        public UIForm<T> End()
        {
            GUILayout.EndVertical();
            GUI.enabled = true;
            return this;
        }
        public UIForm<T> CustomAction(Action action)
        {
            action?.Invoke();
            return this;
        }
        private void Labeled(string label, Action action)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();

            action();

            GUILayout.EndHorizontal();
        }

    }
}

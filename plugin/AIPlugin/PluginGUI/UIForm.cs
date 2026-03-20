using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.PluginGUI
{
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
        {
            return new UIForm<T>(model);
        }
    }
    public class UIForm<T>
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
        public UIForm<T> Dropdown(string label, List<string> options, Expression<Func<T, CustomGUI.DropdownState>> expr, Action OnChanged = null)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            GUILayout.Label(label);
            value = CustomGUI.Dropdown(value, options);

            if (value.SelectionChanged) OnChanged?.Invoke();

            set(_model, value);

            return this;
        }
        public UIForm<T> IntegerField(string label, Expression<Func<T, int>> expr)
        {
            var (get, set) = BindingCache<T>.Get(expr);

            var value = get(_model);

            Labeled(label,
                () => value = CustomGUI.IntegerField(value, GUILayout.Width(80)));

            set(_model, value);

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
        public UIForm<T> Button(string label, Action OnPress)
        {
            if(GUILayout.Button(label, Styles.Button))
                OnPress();
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

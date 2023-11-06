﻿using System;
using Plasma.Mods.RuntimeUnityEditor.Core.Utils;
using Plasma.Mods.RuntimeUnityEditor.Core.Utils.Abstractions;

namespace Plasma.Mods.RuntimeUnityEditor.Core
{
    public interface IFeature
    {
        bool Enabled { get; set; }
        void OnInitialize(InitSettings initSettings);
        void OnUpdate();
        void OnLateUpdate();
        void OnOnGUI();
        void OnEditorShownChanged(bool visible);
        FeatureDisplayType DisplayType { get; }
        string DisplayName { get; }
    }

    public enum FeatureDisplayType
    {
        Hidden,
        Feature,
        Window
    }

    public abstract class FeatureBase<T> : IFeature where T : FeatureBase<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static bool _initialized;
        public static bool Initialized => _initialized;
        public static T Instance { get; private set; }

        protected FeatureBase()
        {
            DisplayType = FeatureDisplayType.Feature;
            FeatureBase<T>.Instance = (T)this;
        }

        protected string SettingCategory = "Features";
        private protected string _displayName;
        private bool _enabled;
        private Action<bool> _confEnabled;

        /// <summary>
        /// Name shown in taskbar
        /// </summary>
        public virtual string DisplayName
        {
            get => _displayName ?? (_displayName = GetType().Name);
            set => _displayName = value;
        }

        /// <summary>
        /// If this instance is enabled and can be shown (if RUE is shown as a whole).
        /// </summary>
        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    // Need to get this before setting _enabled
                    var prevVisible = Visible;
                    _enabled = value;

                    var nowVisible = Visible;
                    if (prevVisible != nowVisible)
                        OnVisibleChanged(nowVisible);

                    _confEnabled?.Invoke(value);
                }
            }
        }


        /// <summary>
        /// If this instance is actually shown on screen / has its events fired.
        /// </summary>
        public bool Visible => Enabled && RuntimeUnityEditorCore.Instance.Show;

        /// <summary>
        /// How this Feature is shown in taskbar
        /// </summary>
        public FeatureDisplayType DisplayType { get; protected set; }

        void IFeature.OnInitialize(InitSettings initSettings)
        {
            if (Initialized) throw new InvalidOperationException("The Feature is already initialized");

            Initialize(initSettings);
            AfterInitialized(initSettings);
            _initialized = true;
        }

        protected virtual void AfterInitialized(InitSettings initSettings)
        {
            _confEnabled = initSettings.RegisterSetting(SettingCategory, DisplayName + " enabled", Enabled, string.Empty, b => Enabled = b);
        }

        void IFeature.OnUpdate()
        {
            if (_initialized && Enabled)
            {
                try
                {
                    Update();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError( e);
                }
            }
        }
        void IFeature.OnLateUpdate()
        {
            if (_initialized && Enabled)
            {
                try
                {
                    LateUpdate();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError( e);
                }
            }
        }
        void IFeature.OnOnGUI()
        {
            if (_initialized && Enabled)
            {
                try
                {
                    OnGUI();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError( e);
                }
            }
        }
        void IFeature.OnEditorShownChanged(bool visible)
        {
            if (_initialized)
            {
                if (!Enabled) return;

                OnVisibleChanged(visible);
            }
        }

        protected virtual void OnVisibleChanged(bool visible)
        {
            try
            {
                VisibleChanged(visible);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError( e);
            }
        }

        protected abstract void Initialize(InitSettings initSettings);
        protected virtual void Update() { }
        protected virtual void LateUpdate() { }
        protected virtual void OnGUI() { }
        protected virtual void VisibleChanged(bool visible) { }
    }
}

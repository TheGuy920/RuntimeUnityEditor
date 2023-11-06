﻿using Plasma.Mods.RuntimeUnityEditor.Core.ObjectTree;
using Plasma.Mods.RuntimeUnityEditor.Core.ObjectView;
using Plasma.Mods.RuntimeUnityEditor.Core.Profiler;
using Plasma.Mods.RuntimeUnityEditor.Core.REPL;
using Plasma.Mods.RuntimeUnityEditor.Core.UI;
using Plasma.Mods.RuntimeUnityEditor.Core.Utils;
using Plasma.Mods.RuntimeUnityEditor.Core.Utils.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
#pragma warning disable CS0618

namespace Plasma.Mods.RuntimeUnityEditor.Core
{
    public class RuntimeUnityEditorCore
    {
        public const string Version = "4.2";
        public const string GUID = "RuntimeUnityEditor";

        [Obsolete("Use window Instance instead")] public Inspector.Inspector Inspector => Core.Inspector.Inspector.Initialized ? Core.Inspector.Inspector.Instance : null;
        [Obsolete("Use window Instance instead")] public ObjectTreeViewer TreeViewer => ObjectTreeViewer.Initialized ? ObjectTreeViewer.Instance : null;
        [Obsolete("Use window Instance instead")] public ObjectViewWindow PreviewWindow => ObjectViewWindow.Initialized ? ObjectViewWindow.Instance : null;
        [Obsolete("Use window Instance instead")] public ProfilerWindow ProfilerWindow => ProfilerWindow.Initialized ? ProfilerWindow.Instance : null;
        [Obsolete("Use window Instance instead")] public ReplWindow Repl => ReplWindow.Initialized ? ReplWindow.Instance : null;
        [Obsolete("Use window Instance instead")] public WindowManager WindowManager => WindowManager.Instance;

        [Obsolete("No longer works", true)] public event EventHandler SettingsChanged;

        public KeyCode ShowHotkey
        {
            get => _showHotkey;
            set
            {
                if (_showHotkey != value)
                {
                    _showHotkey = value;
                    _onHotkeyChanged?.Invoke(value);
                }
            }
        }

        private readonly Action<KeyCode> _onHotkeyChanged;

        public bool ShowRepl
        {
            get => Repl != null && Repl.Enabled;
            set { if (Repl != null) Repl.Enabled = value; }
        }

        public bool EnableMouseInspect
        {
            get => MouseInspect.Initialized && MouseInspect.Instance.Enabled;
            set => MouseInspect.Instance.Enabled = value;
        }

        public bool ShowInspector
        {
            get => Inspector != null && Inspector.Enabled;
            set => Inspector.Enabled = value;
        }

        public static RuntimeUnityEditorCore Instance { get; private set; }

        internal static MonoBehaviour PluginObject => _initSettings.PluginMonoBehaviour;
        private static InitSettings _initSettings;

        private readonly List<IFeature> _initializedFeatures = new List<IFeature>();
        /// <summary>
        /// All features that have been successfully initialized so far
        /// </summary>
        public IEnumerable<IFeature> InitializedFeatures => _initializedFeatures;
        private KeyCode _showHotkey = KeyCode.F12;

        //private readonly List<IWindow> _initializedWindows = new List<IWindow>();

        public RuntimeUnityEditorCore(InitSettings initSettings)
        {
            if (Instance != null)
                throw new InvalidOperationException("Can only create one instance of the Core object");

            _initSettings = initSettings;

            Instance = this;

            _onHotkeyChanged = initSettings.RegisterSetting("General", "Open/close runtime editor", KeyCode.F12, "", x => ShowHotkey = x);

            var iFeatureType = typeof(IFeature);
            // Create all instances first so they are accessible in Initialize methods in case there's crosslinking spaghetti
            var allFeatures = typeof(RuntimeUnityEditorCore).Assembly.GetTypesSafe().Where(t => !t.IsAbstract && iFeatureType.IsAssignableFrom(t)).Select(Activator.CreateInstance).Cast<IFeature>().ToList();

            foreach (var feature in allFeatures)
            {
                try
                {
                    AddFeatureInt(feature);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning( $"Failed to initialize {feature.GetType().Name} - " + e);
                }
            }

            WindowManager.SetFeatures(_initializedFeatures);
            UnityEngine.Debug.Log( $"Successfully initialized {_initializedFeatures.Count}/{allFeatures.Count} features: {string.Join(", ", _initializedFeatures.Select(x => x.GetType().Name).ToArray())}");

        }

        /// <summary>
        /// Add a new feature to runtime editor.
        /// Will throw if feature fails to initialize.
        /// </summary>
        public void AddFeature(IFeature feature)
        {
            AddFeatureInt(feature);
            WindowManager.SetFeatures(_initializedFeatures);
        }

        private void AddFeatureInt(IFeature feature)
        {
            feature.OnInitialize(_initSettings);

            _initializedFeatures.Add(feature);
            //if (feature is IWindow window)
            //    _initializedWindows.Add(window);
        }

        public bool Show
        {
            get => WindowManager.Enabled;
            set
            {
                if (WindowManager.Enabled == value) return;
                WindowManager.Enabled = value;

                for (var index = 0; index < _initializedFeatures.Count; index++)
                    _initializedFeatures[index].OnEditorShownChanged(value);

                // todo safe invoke
                //ShowChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        //public event EventHandler ShowChanged;

        public void OnGUI()
        {
            if (Show)
            {
                var originalSkin = GUI.skin;
                GUI.skin = InterfaceMaker.CustomSkin;

                for (var index = 0; index < _initializedFeatures.Count; index++)
                    _initializedFeatures[index].OnOnGUI();

                // Restore old skin for maximum compatibility
                GUI.skin = originalSkin;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetAsyncKeyState(int vKey);
        private static bool IsF12KeyPressed()
        {
            const int VK_F12 = 0x7B;
            return (GetAsyncKeyState(VK_F12) & 0x8000) != 0;
        }
        private bool waiting_for_release = false;
        public void Update()
        {
            if (IsF12KeyPressed() && !waiting_for_release)
            {
                waiting_for_release = true;
                Show = !Show;
            }
            else if (!IsF12KeyPressed() && waiting_for_release)
            {
                waiting_for_release = false;
            }
            if (Show)
            {
                for (var index = 0; index < _initializedFeatures.Count; index++)
                    _initializedFeatures[index].OnUpdate();
            }
        }

        public void LateUpdate()
        {
            if (Show)
            {
                for (var index = 0; index < _initializedFeatures.Count; index++)
                    _initializedFeatures[index].OnLateUpdate();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plasma.Mods.RuntimeUnityEditor.Core.Utils.Abstractions
{
    /// <summary>
    /// Abstraction layer over Unity's input systems for use in universal plugins that need to use hotkeys.
    /// It can use either Input or Unity.InputSystem, depending on what's available. Input is preferred.
    /// WARNING: Use only inside of Unity's main thread!
    /// Copied from BepInEx 5.4.20 to keep support for other plugin loaders.
    /// </summary>
    internal class UnityInput
    {
        private static IInputSystem _current;
        /// <summary>
        /// Best currently supported input system.
        /// </summary>
        public static IInputSystem Current
        {
            get
            {
                if (_current == null)
                {
                    try
                    {
                        throw new InvalidOperationException();
                        Input.GetKeyDown(KeyCode.A);
                        _current = new LegacyInputSystem();
                        UnityEngine.Debug.Log( "[UnityInput] Using LegacyInputSystem");
                    }
                    catch (InvalidOperationException)
                    {
                        _current = new NewInputSystem();
                        UnityEngine.Debug.Log( "[UnityInput] Using NewInputSystem");
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.Log( "[UnityInput] Failed to detect available input systems - " + ex);
                    }
                }
                return _current;
            }
        }

        /// <summary>
        /// True if the Input class is not disabled.
        /// </summary>
        public bool LegacyInputSystemAvailable => Current is LegacyInputSystem;
    }

    /// <summary>
    /// Generic input system interface. Just barely good enough for hotkeys.
    /// </summary>
    public interface IInputSystem
    {
        // No easy way to use these with the new input system
        //bool GetButton(string buttonName);
        //bool GetButtonDown(string buttonName);
        //bool GetButtonUp(string buttonName);

        /// <inheritdoc cref="Input.GetKey(string)"/>
        bool GetKey(string name);
        /// <inheritdoc cref="Input.GetKey(KeyCode)"/>
        bool GetKey(KeyCode key);

        /// <inheritdoc cref="Input.GetKeyDown(string)"/>
        bool GetKeyDown(string name);
        /// <inheritdoc cref="Input.GetKeyDown(KeyCode)"/>
		bool GetKeyDown(KeyCode key);

        /// <inheritdoc cref="Input.GetKeyUp(string)"/>
		bool GetKeyUp(string name);
        /// <inheritdoc cref="Input.GetKeyUp(KeyCode)"/>
		bool GetKeyUp(KeyCode key);


        /// <inheritdoc cref="Input.GetMouseButton(int)"/>
        bool GetMouseButton(int button);
        /// <inheritdoc cref="Input.GetMouseButtonDown(int)"/>
		bool GetMouseButtonDown(int button);
        /// <inheritdoc cref="Input.GetMouseButtonUp(int)"/>
		bool GetMouseButtonUp(int button);

        /// <inheritdoc cref="Input.ResetInputAxes()"/>
		void ResetInputAxes();

        /// <inheritdoc cref="Input.mousePosition"/>
		Vector3 mousePosition { get; }
        /// <inheritdoc cref="Input.mouseScrollDelta"/>
		Vector2 mouseScrollDelta { get; }

        /// <inheritdoc cref="Input.mousePresent"/>
		bool mousePresent { get; }

        /// <inheritdoc cref="Input.anyKey"/>
		bool anyKey { get; }
        /// <inheritdoc cref="Input.anyKeyDown"/>
        bool anyKeyDown { get; }

        /// <summary>
        /// All KeyCodes supported by the current input system.
        /// </summary>
		IEnumerable<KeyCode> SupportedKeyCodes { get; }
    }

    internal class NewInputSystem : IInputSystem
    {
        public bool GetKey(string name) => Input.GetKey(name);

        public bool GetKey(KeyCode key) => Input.GetKey(key);

        public bool GetKeyDown(string name) => Input.GetKeyDown(name);

        public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);

        public bool GetKeyUp(string name) => Input.GetKeyUp(name);

        public bool GetKeyUp(KeyCode key) => Input.GetKeyUp(key);

        public bool GetMouseButton(int button) => Input.GetMouseButton((int)KeyCode.Mouse0 + button);

        public bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown((int)KeyCode.Mouse0 + button);

        public bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp((int)KeyCode.Mouse0 + button);

        public void ResetInputAxes() { /*Not supported*/ }
        public Vector3 mousePosition => Input.mousePosition;
        public Vector2 mouseScrollDelta => Input.mouseScrollDelta;
        public bool mousePresent => Input.mousePresent;

        public bool anyKey
        {
            get
            {
                if (Input.anyKey)
                    return true;
                return Input.GetMouseButton(0) ||
                       Input.GetMouseButton(1) ||
                       Input.GetMouseButton(2) ||
                       Input.GetMouseButton(3) ||
                       Input.GetMouseButton(4);
            }
        }

        public bool anyKeyDown
        {
            get
            {
                return Input.anyKeyDown;
            }
        }

        public IEnumerable<KeyCode> SupportedKeyCodes => Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();
    }

    internal class LegacyInputSystem : IInputSystem
    {
        public bool GetKey(string name) => Input.GetKey(name);

        public bool GetKey(KeyCode key) => Input.GetKey(key);

        public bool GetKeyDown(string name) => Input.GetKeyDown(name);

        public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);

        public bool GetKeyUp(string name) => Input.GetKeyUp(name);

        public bool GetKeyUp(KeyCode key) => Input.GetKeyUp(key);

        public bool GetMouseButton(int button) => Input.GetMouseButton(button);

        public bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);

        public bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp(button);

        public void ResetInputAxes() => Input.ResetInputAxes();

        public Vector3 mousePosition => Input.mousePosition;
        public Vector2 mouseScrollDelta => Input.mouseScrollDelta;
        public bool mousePresent => Input.mousePresent;
        public bool anyKey => Input.anyKey;
        public bool anyKeyDown => Input.anyKeyDown;

        public IEnumerable<KeyCode> SupportedKeyCodes { get; } = (KeyCode[])Enum.GetValues(typeof(KeyCode));
    }
}
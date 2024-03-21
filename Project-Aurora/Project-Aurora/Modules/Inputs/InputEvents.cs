using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AuroraRgb.Utils;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using User32 = AuroraRgb.Utils.User32;

namespace AuroraRgb.Modules.Inputs;

/// <summary>
/// Class for subscribing to various HID input events
/// </summary>
public sealed class InputEvents : IInputEvents
{
    /// <summary>
    /// Event for a Key pressed Down on a keyboard
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs>? KeyDown;

    /// <summary>
    /// Event for a Key released on a keyboard
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs>? KeyUp;

    /// <summary>
    /// Event that fires when a mouse button is pressed down.
    /// </summary>
    public event EventHandler<MouseKeyEventArgs>? MouseButtonDown;

    /// <summary>
    /// Event that fires when a mouse button is released.
    /// </summary>
    public event EventHandler<MouseKeyEventArgs>? MouseButtonUp;

    /// <summary>
    /// Event that fires when the mouse scroll wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseScrollEventArgs>? Scroll;

    private readonly List<Keys> _pressedKeySequence = [];

    private readonly List<MouseButtons> _pressedMouseButtons = [];

    private bool _disposed;

    public IReadOnlyCollection<Keys> PressedKeys { get; private set; } = [];

    public IReadOnlyCollection<MouseButtons> PressedMouseButtons => _pressedMouseButtons;

    public bool Shift { get; private set; }
    public bool Alt { get; private set; }
    public bool Control { get; private set; }
    public bool Windows { get; private set; }

    private delegate nint WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable to keep reference for garbage collector
    private readonly WndProc? _fnWndProcHook;
    private readonly nint _originalWndProc;
    private readonly IntPtr _hWnd;

    public InputEvents()
    {
        _hWnd = User32.CreateWindowEx(0, "STATIC", "", 0x80000000, 0, 0,
            0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        _originalWndProc = User32.GetWindowLongPtr(_hWnd, -4);

        // register the keyboard device and you can register device which you need like mouse
        RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, _hWnd);
        RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, _hWnd);

        _fnWndProcHook = Hook;
        var newLong = Marshal.GetFunctionPointerForDelegate(_fnWndProcHook);
        User32.SetWindowLongPtr(_hWnd, -4, newLong);
    }
    
    private nint Hook(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam)
    {
        const int wmInput = 0x00FF;

        // You can read inputs by processing the WM_INPUT message.
        if (msg != wmInput) return User32.CallWindowProc(_originalWndProc, _hWnd, msg, wparam, lparam);
        // Create an RawInputData from the handle stored in lParam.
        var data = RawInputData.FromHandle(lparam);

        // The data will be an instance of either RawInputMouseData, RawInputKeyboardData, or RawInputHidData.
        // They contain the raw input data in their properties.
        var intercepted = data switch
        {
            RawInputMouseData mouse => DeviceOnMouseInput(mouse.Mouse),
            RawInputKeyboardData keyboard => DeviceOnKeyboardInput(keyboard.Keyboard),
            _ => false,
        };

        return intercepted ? IntPtr.Zero : User32.CallWindowProc(_originalWndProc, _hWnd, msg, wparam, lparam);
    }

    /// <param name="keyboardData"></param>
    /// <returns>if input should be interrupted or not</returns>
    private bool DeviceOnKeyboardInput(RawKeyboard keyboardData)
    {
        if (HasNoListener())
        {
            return false;
        }

        try
        {
            var flags = keyboardData.Flags;
            // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
            // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
            var isE0 = flags.HasFlag(RawKeyboardFlags.KeyE0);
            var isE1 = flags.HasFlag(RawKeyboardFlags.KeyE1);
            var key = KeyUtils.CorrectRawInputData(keyboardData.VirutalKey, keyboardData.ScanCode, isE0, isE1);
            if ((int)key == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return false;
            }

            var down = (flags & RawKeyboardFlags.Up) == 0;
            SetModifierKeys(key, down);
            if (!ProcessKeyList(down, key))
            {
                return false;
            }

            PressedKeys = _pressedKeySequence.ToArray();
            
            var keyboardKeyEvent = new KeyboardKeyEventArgs(key, flags.HasFlag(RawKeyboardFlags.KeyE0), PressedKeys);
            if (down)
            {
                KeyDown?.Invoke(this, keyboardKeyEvent);
            }
            else
            {
                KeyUp?.Invoke(this, keyboardKeyEvent);
            }

            return keyboardKeyEvent.Intercepted;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception while handling keyboard input");
            return false;
        }
    }

    private bool HasNoListener()
    {
        var noKeyDownListeners = KeyDown == null || KeyDown.GetInvocationList().Length == 0;
        var noKeyUpListeners = KeyUp == null || KeyUp?.GetInvocationList().Length == 0;
        return noKeyDownListeners && noKeyUpListeners;
    }

    private bool ProcessKeyList(bool down, Keys key)
    {
        if (down)
        {
            if (_pressedKeySequence.Contains(key))
            {
                // this key is already processed
                return false;
            }
            _pressedKeySequence.Add(key);
        }
        else
        {
            var removed = _pressedKeySequence.Remove(key);
            return removed;   // return if key is removed
        }

        return true;
    }

    private void SetModifierKeys(Keys key, bool down)
    {
        switch (key)
        {
            case Keys.ShiftKey:
            case Keys.RShiftKey:
            case Keys.LShiftKey:
                Shift = down;
                break;
            case Keys.Menu:
            case Keys.RMenu:
            case Keys.LMenu:
                Alt = down;
                break;
            case Keys.ControlKey:
            case Keys.RControlKey:
            case Keys.LControlKey:
                Control = down;
                break;
            case Keys.RWin:
            case Keys.LWin:
                Windows = down;
                break;
        }
    }

    /// <summary>
    /// Handles a SharpDX MouseInput event and fires the relevant InputEvents event (Scroll, MouseButtonDown or MouseButtonUp).
    /// <returns>if input should be interrupted or not</returns>
    /// </summary>
    private bool DeviceOnMouseInput(RawMouse mouseData)
    {
        // Scrolling
        if (mouseData.ButtonData != 0)
        {
            if (mouseData.Buttons != RawMouseButtonFlags.MouseWheel) return false;

            var mouseScrollEvent = new MouseScrollEventArgs(mouseData.ButtonData);
            Scroll?.Invoke(this, mouseScrollEvent);
            return mouseScrollEvent.Intercepted;
        }

        var (button, isDown) = mouseData.Buttons switch
        {
            RawMouseButtonFlags.LeftButtonDown => (MouseButtons.Left, true),
            RawMouseButtonFlags.LeftButtonUp => (MouseButtons.Left, false),
            RawMouseButtonFlags.MiddleButtonDown => (MouseButtons.Middle, true),
            RawMouseButtonFlags.MiddleButtonUp => (MouseButtons.Middle, false),
            RawMouseButtonFlags.RightButtonDown => (MouseButtons.Right, true),
            RawMouseButtonFlags.RightButtonUp => (MouseButtons.Right, false),
            _ => (MouseButtons.Left, false)
        };

        var mouseKeyEvent = new MouseKeyEventArgs(button);
        if (isDown)
        {
            _pressedMouseButtons.Add(button);
            MouseButtonDown?.Invoke(this, mouseKeyEvent);
        }
        else
        {
            _pressedMouseButtons.Remove(button);
            MouseButtonUp?.Invoke(this, mouseKeyEvent);
        }

        return mouseKeyEvent.Intercepted;
    }

    public TimeSpan GetTimeSinceLastInput() {
        var inf = new User32.TagLastInputInfo { cbSize = (uint)Marshal.SizeOf<User32.TagLastInputInfo>() };
        return !User32.GetLastInputInfo(ref inf) ?
            new TimeSpan(0) :
            new TimeSpan(0, 0, 0, 0, Environment.TickCount - inf.dwTime);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
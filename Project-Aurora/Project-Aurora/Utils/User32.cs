﻿using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AuroraRgb.Utils;

internal static partial class User32
{
    private static readonly TagLastInputInfo LastInput = new()
    {
        cbSize = (uint)Marshal.SizeOf(typeof(TagLastInputInfo)),
        dwTime = 0,
    };
    
    [LibraryImport("user32.dll", EntryPoint = "CallWindowProcA")]
    public static partial nint CallWindowProc(nint lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [Pure]
    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [LibraryImport("user32.dll", EntryPoint = "DestroyWindowA")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(IntPtr hWnd);

    [Pure]
    [LibraryImport("user32.dll")]
    internal static partial IntPtr GetForegroundWindow();

    [Pure]
    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    internal static partial IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [Pure]
    [LibraryImport("user32.dll")]
    internal static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    internal static partial void SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
        int idChild, uint dwEventThread, uint dwmsEventTime);

    [LibraryImport("user32.dll")]
    internal static partial IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial void UnhookWinEvent(IntPtr eventHook);

    [Pure]
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetLastInputInfo(ref TagLastInputInfo plii);

    [Pure]
    internal static bool GetLastInputInfoOut(out TagLastInputInfo plii)
    {
        plii = LastInput;
        return GetLastInputInfo(ref plii);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TagLastInputInfo
    {
        public uint cbSize;
        public Int32 dwTime;
    }
}
using System;
using System.Runtime.InteropServices;

namespace AuroraRgb.Utils;

public static partial class Kernel32
{
    private const string KERNEL32_DLL = "kernel32.dll";
    public static IntPtr CurrentModuleHandle { get; }

    static Kernel32()
    {
        CurrentModuleHandle = GetModuleHandle(null);
        if (CurrentModuleHandle == IntPtr.Zero)
            throw new Exception("Could not get module handle.");
    }
    
    [LibraryImport(KERNEL32_DLL, EntryPoint = "WTSGetActiveConsoleSessionId")]
    internal static partial uint WtsGetActiveConsoleSessionId();

    [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.Winapi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    private static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPTStr)] string? lpModuleName);
}
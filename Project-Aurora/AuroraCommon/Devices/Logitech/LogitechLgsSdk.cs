using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Common.Utils;

namespace Common.Devices.Logitech;

public sealed class LogitechLgsSdk : ILogitechGsdk
{

    public bool LogiLedInit()
    {
        return LgsImports.LogiLedInit();
    }

    public bool LogiLedSetTargetDevice(LogiLedType deviceType)
    {
        return LgsImports.LogiLedSetTargetDevice(deviceType);
    }

    public bool LogiLedSetLighting(SimpleColor color)
    {
        var (r, g, b) = GetColorValues(color);
        return LgsImports.LogiLedSetLighting(r, g, b);
    }

    public bool LogiLedSetLightingFromBitmap(byte[] bitmap)
    {
        return LgsImports.LogiLedSetLightingFromBitmap(bitmap);
    }

    public bool LogiLedSetLightingForKeyWithKeyName(KeyboardNames keyCode, SimpleColor color)
    {
        var (r, g, b) = GetColorValues(color);
        return LgsImports.LogiLedSetLightingForKeyWithKeyName(keyCode, r, g, b);
    }

    public bool LogiLedSetLightingForTargetZone(DeviceType deviceType, int zone, SimpleColor color)
    {
        var (r, g, b) = GetColorValues(color);
        return LgsImports.LogiLedSetLightingForTargetZone((byte)deviceType, zone, r, g, b);
    }

    public void LogiLedShutdown()
    {
        LgsImports.LogiLedShutdown();
    }

    public bool LogiLedSaveCurrentLighting()
    {
        return LgsImports.LogiLedSaveCurrentLighting();
    }

    public bool LogiLedRestoreLighting()
    {
        return LgsImports.LogiLedRestoreLighting();
    }

    private static (int R, int G, int B) GetColorValues(SimpleColor clr)
    {
        clr = CommonColorUtils.CorrectWithAlpha(clr);
        return ((int)(clr.R * 100.0 / 255.0),
            (int)(clr.G * 100.0 / 255.0),
            (int)(clr.B * 100.0 / 255.0));
    }
}

internal static partial class LgsImports
{
    private const string Dllpath = @"Logi\LGS\LogitechLed.dll";

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedInit();

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedSetTargetDevice(LogiLedType targetDevice);

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedSaveCurrentLighting();

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage);

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedRestoreLighting();

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedSetLightingFromBitmap(in byte[] bitmap);

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedSetLightingForKeyWithKeyName(KeyboardNames keyCode, int redPercentage, int greenPercentage, int bluePercentage);

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedSetLightingForTargetZone(byte deviceType, int zone, int redPercentage, int greenPercentage, int bluePercentage);

    [LibraryImport(Dllpath)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void LogiLedShutdown();
}

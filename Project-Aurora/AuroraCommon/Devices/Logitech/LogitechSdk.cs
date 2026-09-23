using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Common.Utils;

namespace Common.Devices.Logitech;

public sealed class LogitechSdk
{
    private const int LogiLedBitmapWidth = 21;
    private const int LogiLedBitmapHeight = 6;
    private const int LogiLedBitmapBytesPerKey = 4;

    public const int LOGI_LED_BITMAP_SIZE = LogiLedBitmapWidth * LogiLedBitmapHeight * LogiLedBitmapBytesPerKey;

    public bool LogiLedInitWithName(string name)
    {
        return GhubImports.LogiLedInitWithName(name);
    }

    public bool LogiLedSetTargetDevice(LogiLedType deviceType)
    {
        return GhubImports.LogiLedSetTargetDevice(deviceType);
    }

    public bool LogiLedSetLighting(SimpleColor color)
    {
        var (r, g, b) = GetColorValues(color);
        return GhubImports.LogiLedSetLighting(r, g, b);
    }

    public bool LogiLedExcludeKeysFromBitmap(in KeyboardNames[] names)
    {
        return GhubImports.LogiLedExcludeKeysFromBitmap(names, names.Length);
    }

    public bool LogiLedSetLightingFromBitmap(in byte[] bitmap)
    {
        return GhubImports.LogiLedSetLightingFromBitmap(bitmap);
    }

    public bool LogiLedSetLightingForKeyWithKeyName(KeyboardNames keyCode, SimpleColor color)
    {
        var (r, g, b) = GetColorValues(color);
        return GhubImports.LogiLedSetLightingForKeyWithKeyName(keyCode, r, g, b);
    }

    public bool LogiLedSetLightingForTargetZone(DeviceType deviceType, int zone, SimpleColor color)
    {
        var (r, g, b) = GetColorValues(color);
        return GhubImports.LogiLedSetLightingForTargetZone((byte)deviceType, zone, r, g, b);
    }

    public void LogiLedShutdown()
    {
        GhubImports.LogiLedShutdown();
    }

    public bool LogiLedSaveCurrentLighting()
    {
        return GhubImports.LogiLedSaveCurrentLighting();
    }

    public bool LogiLedRestoreLighting()
    {
        return GhubImports.LogiLedRestoreLighting();
    }

    private (int R, int G, int B) GetColorValues(SimpleColor clr)
    {
        clr = CommonColorUtils.CorrectWithAlpha(clr);
        return ((int)(clr.R * 100.0 / 255.0),
            (int)(clr.G * 100.0 / 255.0),
            (int)(clr.B * 100.0 / 255.0));
    }
}

internal static partial class GhubImports
{
    private const string Dllpath = @"x64\LogitechLedEnginesWrapper.dll";

    [LibraryImport(Dllpath, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LogiLedInitWithName(string name);

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
    public static partial bool LogiLedExcludeKeysFromBitmap(in KeyboardNames[] names, int length); 

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
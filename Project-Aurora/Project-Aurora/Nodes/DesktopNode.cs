using System;
using AuroraRgb.Utils;
using Common.Utils;

namespace AuroraRgb.Nodes;

public class DesktopNode : Node
{
    /// <summary>
    /// Returns whether or not the device dession is in a locked state.
    /// </summary>
    public static bool IsLocked => DesktopUtils.IsDesktopLocked;

    public int AccentColorA { get; private set; }
    public int AccentColorR { get; private set; }
    public int AccentColorG { get; private set; }
    public int AccentColorB { get; private set; }

    public bool NightLightEnabled { get; private set; }

    public double NightLightColorB { get; private set; }
    public double NightLightColorG { get; private set; }

    private static CursorPositionNode? _cursorPosition;
    public static CursorPositionNode CursorPosition => _cursorPosition ??= new CursorPositionNode();

    private readonly RegistryWatcher _accentColorWatcher = new(RegistryHiveOpt.CurrentUser, @"SOFTWARE\\Microsoft\\Windows\\DWM", "AccentColor");

    private readonly RegistryWatcher _nightLightStateWatcher = new(RegistryHiveOpt.CurrentUser,
        @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate",
        "Data");

    private readonly RegistryWatcher _nightLightSettingsWatcher = new(RegistryHiveOpt.CurrentUser,
        @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.settings\\windows.data.bluelightreduction.settings",
        "Data");

    public DesktopNode()
    {
        _accentColorWatcher.RegistryChanged += UpdateAccentColor;
        _accentColorWatcher.StartWatching();

        _nightLightStateWatcher.RegistryChanged += UpdateNightLight;
        _nightLightStateWatcher.StartWatching();

        _nightLightSettingsWatcher.RegistryChanged += UpdateNightLightColor;
        _nightLightSettingsWatcher.StartWatching();
    }

    private void UpdateAccentColor(object? sender, RegistryChangedEventArgs registryChangedEventArgs)
    {
        var data = registryChangedEventArgs.Data;
        switch (data)
        {
            case null:
                return;
            case int color:
                var a = (byte)((color >> 24) & 0xFF);
                var b = (byte)((color >> 16) & 0xFF);
                var g = (byte)((color >> 8) & 0xFF);
                var r = (byte)((color >> 0) & 0xFF);

                AccentColorA = a;
                AccentColorR = r;
                AccentColorG = g;
                AccentColorB = b;
                break;
        }
    }

    private void UpdateNightLightColor(object? sender, RegistryChangedEventArgs e)
    {
        var data = e.Data;
        if (data is null)
        {
            NightLightColorB = 255;
            NightLightColorG = 255;
            return;
        }

        var byteData = (byte[])data;

        // 0.0 - 0.5 blue reduction, 0.5 - 1.0 green reduction
        var nightLightsStrength = ParseNightLightStrength(byteData);

        NightLightColorB = Math.Clamp(1 - nightLightsStrength * 2, 0.0, 1.0);
        NightLightColorG = Math.Clamp(2 - nightLightsStrength * 2, 0.0, 1.0);
    }

    private static double ParseNightLightStrength(byte[] data)
    {
        const int min = 4832;
        const int max = 26056;
        if (data.Length < 37)
        {
            return 0;
        }

        var value = BitConverter.ToInt16(data, 35);
        return 1 - (double)(value - min) / (max - min);
    }

    private void UpdateNightLight(object? sender, RegistryChangedEventArgs registryChangedEventArgs)
    {
        var data = registryChangedEventArgs.Data;
        if (data is not byte[] byteData)
        {
            NightLightEnabled = false;
            return;
        }

        NightLightEnabled = byteData.Length > 24 && byteData[23] == 0x10 && byteData[24] == 0x00;
    }
}
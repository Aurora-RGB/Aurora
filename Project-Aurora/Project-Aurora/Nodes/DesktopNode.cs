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

    private static CursorPositionNode? _cursorPosition;
    public static CursorPositionNode CursorPosition => _cursorPosition ??= new CursorPositionNode();
    
    private readonly RegistryWatcher _accentColorWatcher = new(RegistryHiveOpt.CurrentUser, @"SOFTWARE\\Microsoft\\Windows\\DWM", "AccentColor");

    public DesktopNode()
    {
        _accentColorWatcher.RegistryChanged += UpdateAccentColor;
        _accentColorWatcher.StartWatching();
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
}
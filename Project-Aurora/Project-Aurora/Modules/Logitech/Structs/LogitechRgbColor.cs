using System.Drawing;
using System.Runtime.InteropServices;
using Common;
using Common.Utils;

namespace AuroraRgb.Modules.Logitech.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct LogitechRgbColor
{
    public static readonly LogitechRgbColor Empty = new();

    public int R;
    public int G;
    public int B;

    public static implicit operator Color(LogitechRgbColor c)
    {
        return CommonColorUtils.FastColor((byte)(c.R * 255d / 100d), (byte)(c.G * 255d / 100d), (byte)(c.B * 255d / 100d));
    }

    public static implicit operator SimpleColor(LogitechRgbColor c)
    {
        return new SimpleColor((byte)(c.R * 255d / 100d), (byte)(c.G * 255d / 100d), (byte)(c.B * 255d / 100d));
    }
}
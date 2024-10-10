using System.Drawing;
using System.Runtime.InteropServices;
using Common;
using Common.Utils;

namespace AuroraRgb.Modules.Logitech.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LogitechArgbColor
{
    //bgra
    public readonly byte B;
    public readonly byte G;
    public readonly byte R;
    public readonly byte A;

    public static implicit operator Color(LogitechArgbColor c)
    {
        return CommonColorUtils.FastColor((byte)(c.R * 255d / 100d), (byte)(c.G * 255d / 100d), (byte)(c.B * 255d / 100d), (byte)(c.A * 255d / 100d));
    }

    public static implicit operator SimpleColor(LogitechArgbColor c)
    {
        return new SimpleColor((byte)(c.R * 255d / 100d), (byte)(c.G * 255d / 100d), (byte)(c.B * 255d / 100d), (byte)(c.A * 255d / 100d));
    }
}
﻿using System.Runtime.InteropServices;
using Common;

namespace AuroraDeviceManager.Devices.Omen;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct LightingColor
{
    public byte r;
    public byte g;
    public byte b;

    public static LightingColor FromColor(SimpleColor c)
    {
        var lc = new LightingColor
        {
            r = c.R,
            g = c.G,
            b = c.B
        };
        return lc;
    }
}
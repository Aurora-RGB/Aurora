using System;

namespace AuroraRgb.Modules.Logitech.Enums;

[Flags]
public enum LogiSetTargetDevice
{
    None = 0,
    Monochrome = 1 << 0,
    Rgb = 1 << 1,
    PerKeyRgb = 1 << 2,
    All = Monochrome | Rgb | PerKeyRgb
}
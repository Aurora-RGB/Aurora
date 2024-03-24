using System;
using System.Runtime.InteropServices;
using AuroraRgb.Modules.Logitech.Structs;

namespace AuroraRgb.Modules.Logitech;

public class LogitechPipeConverter
{
    public static LogitechRgbColor ReadPercentageColor(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length != 12 ? LogitechRgbColor.Empty : MemoryMarshal.Read<LogitechRgbColor>(bytes);
    }
}
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace AuroraRgb.Bitmaps;

public interface IBitmapReader : IDisposable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ref readonly Color GetRegionColor(Rectangle rectangle);
}
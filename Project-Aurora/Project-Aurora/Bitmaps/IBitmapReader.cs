using System;
using System.Drawing;

namespace AuroraRgb.Bitmaps;

public interface IBitmapReader : IDisposable
{
    Color GetRegionColor(Rectangle rectangle);
}
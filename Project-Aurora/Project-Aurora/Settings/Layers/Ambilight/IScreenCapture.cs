using System;
using System.Collections.Generic;
using System.Drawing;

namespace AuroraRgb.Settings.Layers.Ambilight;

internal interface IScreenCapture : IDisposable
{
    /// <summary>
    /// Captures a screenshot of the full screen, returning a full resolution bitmap
    /// </summary>
    /// <returns></returns>
    void Capture(Rectangle desktopRegion, Bitmap bitmap);

    event EventHandler<Bitmap> ScreenshotTaken;

    IEnumerable<string> GetDisplays();
}
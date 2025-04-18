﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AuroraRgb.Modules.ProcessMonitor;

namespace AuroraRgb.Settings.Layers.Ambilight;

internal sealed class GdiScreenCapture : IScreenCapture
{
    private static readonly Bitmap DefaultBitmap = new(8, 8);

    public event EventHandler<Bitmap>? ScreenshotTaken;

    private Bitmap? _lastBitmap;
    private Graphics _graphics = Graphics.FromImage(DefaultBitmap);
    private readonly WindowListener.WindowListenerReference _windowListenerReference = new();

    public WindowListener WindowListener => _windowListenerReference.WindowListener;

    public void Capture(Rectangle desktopRegion, Bitmap bitmap)
    {
        if (_lastBitmap != bitmap)
        {
            _graphics.Dispose();
            _graphics = Graphics.FromImage(bitmap);
            _graphics.CompositingMode = CompositingMode.SourceCopy;
            _graphics.CompositingQuality = CompositingQuality.Invalid;
            _graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            _graphics.SmoothingMode = SmoothingMode.HighSpeed;
            _graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            _lastBitmap = bitmap;
        }
        _graphics.CopyFromScreen(desktopRegion.Location, Point.Empty, desktopRegion.Size);

        ScreenshotTaken?.Invoke(this, bitmap);
    }

    public IEnumerable<string> GetDisplays() =>
        Screen.AllScreens.Select((s, index) =>
            $"Display {index + 1}: X:{s.Bounds.X}, Y:{s.Bounds.Y}, W:{s.Bounds.Width}, H:{s.Bounds.Height}");

    public void Dispose()
    {
        _graphics.Dispose();
        _lastBitmap = null;
        _windowListenerReference.Dispose();
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace AuroraRgb.Settings.Layers.Ambilight;

internal sealed class DxScreenCapture : IScreenCapture
{
    public event EventHandler<Bitmap>? ScreenshotTaken;
    
    private static readonly Dictionary<Output5, DesktopDuplicator> Duplicators = new();
    private static readonly object Lock = new();
    
    private Rectangle _currentBounds = Rectangle.Empty;
    private DesktopDuplicator? _desktopDuplicator;

    public void Capture(Rectangle desktopRegion, Bitmap bitmap)
    {
        SetTarget(desktopRegion);

        lock (Lock)
        {
            CaptureLocked(bitmap);
        }
    }

    private void CaptureLocked(Bitmap bitmap)
    {
        try
        {
            if (_currentBounds.IsEmpty)
                return;
            
            var capture = _desktopDuplicator?.Capture(_currentBounds, bitmap, 5000);
            if (capture == null)
            {
                if (_desktopDuplicator?.IsDisposed ?? false)
                {
                    _desktopDuplicator = null;
                }

                return;
            }
            ScreenshotTaken?.Invoke(this, capture);
        }
        catch(Exception e)
        {
            Global.logger.Error(e, "DX Screenshot Error");
            if (!_desktopDuplicator?.IsDisposed ?? false)
            {
                _desktopDuplicator.Dispose(); 
                _desktopDuplicator = null;
            }
            Thread.Sleep(2000);
        }
    }

    private void SetTarget(Rectangle captureRegion)
    {
        var outputs = GetOutputs();
        var currentOutput = outputs.Find(output => RectangleContains(output.Description.DesktopBounds, captureRegion));

        if (currentOutput == null)
        {
            return;
        }

        var desktopBounds = currentOutput.Description.DesktopBounds;
        var x = Math.Max(desktopBounds.Left, captureRegion.Left - desktopBounds.Left);
        var y = Math.Max(desktopBounds.Top, captureRegion.Top - desktopBounds.Top);
        var screenWindowRectangle = new Rectangle(
            x,
            y,
            Math.Min(captureRegion.Width, captureRegion.Width - captureRegion.Right + desktopBounds.Right),
            Math.Min(captureRegion.Height, captureRegion.Height - captureRegion.Bottom + desktopBounds.Bottom)
        );

        if (screenWindowRectangle == _currentBounds && _desktopDuplicator != null)
        {
            return;
        }

        _currentBounds = screenWindowRectangle;

        
        lock (Lock)
        {
            if (Duplicators.TryGetValue(currentOutput, out _desktopDuplicator)) return;
            _desktopDuplicator = new DesktopDuplicator(currentOutput);
            Duplicators.Add(currentOutput, _desktopDuplicator);
        }
    }

    public IEnumerable<string> GetDisplays() => GetOutputs().Select((s, index) =>
    {
        var b = s.Description.DesktopBounds;

        return $"Display {index + 1}: X:{b.Left}, Y:{b.Top}, W:{b.Right - b.Left}, H:{b.Bottom - b.Top}";
    });

    private static Factory1? _factory1;
    private static List<Output5>? _outputs;
    private static List<Output5> GetOutputs()
    {
        if (_factory1 != null && _factory1.IsCurrent && _outputs != null) return _outputs;
        _factory1?.Dispose();
        _factory1 = new Factory1();
        WeakEventManager<Factory1, EventArgs>.AddHandler(_factory1, nameof(_factory1.Disposing), FactoryDisposed);
        _outputs = _factory1.Adapters1
            .SelectMany(m => m.Outputs.Select(n =>
            {
                var o = n.QueryInterface<Output5>();
                WeakEventManager<Output5, EventArgs>.AddHandler(o, nameof(o.Disposing), OutputDisposed);
                return o;
            }))
            .ToList();

        return _outputs;
    }

    private static void FactoryDisposed(object? sender, EventArgs e)
    {
        lock (Lock)
        {
            _outputs = null;
            Duplicators.Clear();
        }
    }

    private static void OutputDisposed(object? sender, EventArgs e)
    {
        if (sender == null)
        {
            return;
        }
        var output5 = (Output5)sender;
        Duplicators.Remove(output5);
        _outputs = null;
    }

    private static bool RectangleContains(RawRectangle containingRectangle, Rectangle rec)
    {
        var center = rec.Location + rec.Size / 2;
        return containingRectangle.Left <= center.X && containingRectangle.Right > center.X &&
               containingRectangle.Top <= center.Y && containingRectangle.Bottom > center.Y;
    }

    public void Dispose()
    {
        lock (Lock)
        {
            _desktopDuplicator = null;
        }
    }
}
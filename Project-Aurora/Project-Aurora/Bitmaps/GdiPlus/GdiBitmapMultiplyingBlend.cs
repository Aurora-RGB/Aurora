using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;

namespace AuroraRgb.Bitmaps.GdiPlus;

// TODO this needs multiplying blend render for preview to work correctly
public class GdiBitmapMultiplyingBlend : IGdiBitmap
{
    public Bitmap Bitmap { get; }

    public double Opacity
    {
        get;
        set
        {
            field = value;
            _colorMatrix.Matrix33 = (float)value;
            _imageAttributes.SetColorMatrix(_colorMatrix);
        }
    } = 1;

    private TextureBrush? _textureBrush;

    private readonly Graphics _graphics;

    private KeySequence _excludeSequence = new();

    private readonly ColorMatrix _colorMatrix = new()
    {
        Matrix33 = 1.0f,
    };
    private readonly ImageAttributes _imageAttributes;

    private bool _disposed;

    public Rectangle Dimension { get; }

    public TextureBrush TextureBrush
    {
        get
        {
            if (_textureBrush != null) return _textureBrush;

            PerformExclude(_excludeSequence);

            if (_disposed)
            {
                return GdiBitmap.EmptyBitmap.TextureBrush;
            }

            _textureBrush = new TextureBrush(Bitmap, Dimension, _imageAttributes);
            return _textureBrush;
        }
    }

    public GdiBitmapMultiplyingBlend(int canvasWidth, int canvasHeight)
    {
        Bitmap = new Bitmap(canvasWidth, canvasHeight);
        _graphics = Graphics.FromImage(Bitmap);
        Dimension = new Rectangle(0, 0, canvasWidth, canvasHeight);

        _imageAttributes = new ImageAttributes();
        _imageAttributes.SetColorMatrix(_colorMatrix);
        _imageAttributes.SetWrapMode(WrapMode.Clamp, Color.Empty);

        SetGraphics();
    }

    private void SetGraphics()
    {
        _graphics.CompositingMode = CompositingMode.SourceOver;
        _graphics.CompositingQuality = CompositingQuality.HighSpeed;
        _graphics.SmoothingMode = SmoothingMode.None;
        _graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        _graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
    }

    public void Reset()
    {
        _graphics.ResetClip();
        _graphics.ResetTransform();
        _textureBrush?.Dispose();
        _textureBrush = null;
    }

    public void DrawRectangle(Brush brush, Rectangle dimension, bool overwriteColor = false)
    {
    }

    public void DrawRectangle(Brush brush, RectangleF dimension, bool overwriteColor = false)
    {
    }

    public void DrawRectangle(Pen pen, RectangleF dimension)
    {
    }

    public void DrawRectangle(EffectLayer brush)
    {
    }

    public void DrawRectangle(IAuroraBrush brush, RectangleF dimension)
    {
    }

    public void DrawRectangle(IAuroraBrush brush, Rectangle dimension)
    {
    }

    public void SetTransform(Matrix value) => _graphics.Transform = value;

    public void DrawEllipse(Pen pen, RectangleF dimension)
    {
    }

    public void DrawOver(IGdiBitmap gdiBitmap)
    {
    }

    public void PerformExclude(KeySequence excludeSequence)
    {
    }
}
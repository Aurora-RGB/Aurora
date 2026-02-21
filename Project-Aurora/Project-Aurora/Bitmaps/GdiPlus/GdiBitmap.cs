using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;

namespace AuroraRgb.Bitmaps.GdiPlus;

public sealed class GdiBitmap
{
    public static readonly GdiBitmap EmptyBitmap = new(1, 1);

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

    private readonly Rectangle _dimension;
    private KeySequence _excludeSequence = new();

    private readonly ColorMatrix _colorMatrix = new()
    {
        Matrix33 = 1.0f,
    };
    private readonly ImageAttributes _imageAttributes;

    private bool _disposed;

    private TextureBrush TextureBrush
    {
        get
        {
            if (_textureBrush != null) return _textureBrush;

            PerformExclude(_excludeSequence);

            if (_disposed)
            {
                return EmptyBitmap.TextureBrush;
            }

            _textureBrush = new TextureBrush(Bitmap, _dimension, _imageAttributes);
            return _textureBrush;
        }
    }

    public GdiBitmap(int canvasWidth, int canvasHeight)
    {
        Bitmap = new Bitmap(canvasWidth, canvasHeight);
        _graphics = Graphics.FromImage(Bitmap);
        _dimension = new Rectangle(0, 0, canvasWidth, canvasHeight);

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

    public IBitmapReader CreateReader()
    {
        return new GdiPartialCopyBitmapReader(Bitmap, this);
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
        if (!overwriteColor)
        {
            _graphics.FillRectangle(brush, dimension.X, dimension.Y, dimension.Width, dimension.Height);
            return;
        }

        var graphicsState = _graphics.Save();
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush, dimension.X, dimension.Y, dimension.Width, dimension.Height);
        _graphics.Restore(graphicsState);
    }

    public void DrawRectangle(Brush brush, RectangleF dimension, bool overwriteColor = false)
    {
        if (!overwriteColor)
        {
            _graphics.FillRectangle(brush, dimension.X, dimension.Y, dimension.Width, dimension.Height);
            return;
        }

        var graphicsState = _graphics.Save();
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush, dimension.X, dimension.Y, dimension.Width, dimension.Height);
        _graphics.Restore(graphicsState);
    }

    public void DrawRectangle(Pen pen, RectangleF dimension)
    {
        _graphics.DrawRectangle(pen, dimension);
    }

    public void DrawRectangle(EffectLayer brush)
    {
        var bitmapEffectLayer = (BitmapEffectLayer)brush;
        var gdiBitmap = bitmapEffectLayer.GetBitmap();
        DrawOver(gdiBitmap);
    }

    public void DrawRectangle(IAuroraBrush brush, RectangleF dimension)
    {
        DrawRectangle(brush.GetBrush(), dimension);
    }

    public void DrawRectangle(IAuroraBrush brush, Rectangle dimension)
    {
        DrawRectangle(brush.GetBrush(), dimension);
    }

    public void ReplaceRectangle(Brush brush, Rectangle dimension)
    {
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush, dimension);
        _graphics.CompositingMode = CompositingMode.SourceOver;
    }

    public void ReplaceRectangle(Brush brush, RectangleF dimension)
    {
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush, dimension);
        _graphics.CompositingMode = CompositingMode.SourceOver;
    }

    public void ReplaceRectangle(IAuroraBrush brush, Rectangle dimension)
    {
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush.GetBrush(), dimension);
        _graphics.CompositingMode = CompositingMode.SourceOver;
    }

    public void ReplaceRectangle(IAuroraBrush brush, RectangleF dimension)
    {
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush.GetBrush(), dimension);
        _graphics.CompositingMode = CompositingMode.SourceOver;
    }

    public void PerformExclude(KeySequence excludeSequence)
    {
        _excludeSequence = excludeSequence;
        using var gp = new GraphicsPath();
        switch (excludeSequence.Type)
        {
            case KeySequenceType.Sequence:
            {
                if (excludeSequence.Keys.Count == 0)
                {
                    return;
                }

                foreach (var k in excludeSequence.Keys)
                {
                    var keyBounds = Effects.Canvas.GetRectangle(k);
                    gp.AddRectangle(keyBounds.Rectangle); //Overlapping additions remove that shape
                }

                break;
            }
            case KeySequenceType.FreeForm:
            {
                var freeform = excludeSequence.Freeform;
                if (freeform.Height == 0 || freeform.Width == 0)
                {
                    return;
                }

                var xPos = (float)Math.Round((freeform.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth);
                var yPos = (float)Math.Round((freeform.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight);
                var width = (float)Math.Round(freeform.Width * Effects.Canvas.EditorToCanvasWidth);
                var height = (float)Math.Round(freeform.Height * Effects.Canvas.EditorToCanvasHeight);

                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);
                using var myMatrix = new Matrix();
                myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                gp.AddRectangle(new RectangleF(xPos, yPos, width, height));
                gp.Transform(myMatrix);
                break;
            }
        }

        _graphics.SetClip(gp);
        _graphics.Clear(Color.Transparent);
    }

    /// <summary>
    /// Inlcudes provided sequence from the layer (Applies a mask)
    /// </summary>
    public void OnlyInclude(KeySequence sequence)
    {
        using var exclusionPath = GetExclusionPath(sequence);
        _graphics.SetClip(exclusionPath);
        _graphics.Clear(Color.Transparent);
    }

    private static GraphicsPath GetExclusionPath(KeySequence sequence)
    {
        if (sequence.Type == KeySequenceType.Sequence)
        {
            var gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(0, 0, Effects.Canvas.Width, Effects.Canvas.Height));
            foreach (var k in sequence.Keys)
            {
                var keyBounds = Effects.Canvas.GetRectangle(k);
                gp.AddRectangle(keyBounds.Rectangle); //Overlapping additons remove that shape
            }

            return gp;
        }
        else
        {
            var gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(0, 0, Effects.Canvas.Width, Effects.Canvas.Height));
            gp.AddRectangle(sequence.GetAffectedRegion());
            return gp;
        }
    }

    public void SetClip(RectangleF boundsRaw) => _graphics.SetClip(boundsRaw);

    public void SetTransform(Matrix value) => _graphics.Transform = value;

    public void DrawEllipse(Pen pen, RectangleF dimension)
    {
        _graphics.DrawEllipse(pen, dimension);
    }

    public void FillEllipse(Brush brush, RectangleF dimension)
    {
        _graphics.FillEllipse(brush, dimension);
    }

    public void FillEllipse(IAuroraBrush brush, Rectangle dimension)
    {
        _graphics.FillEllipse(brush.GetBrush(), dimension);
    }

    public void FillEllipse(IAuroraBrush brush, RectangleF dimension)
    {
        _graphics.FillEllipse(brush.GetBrush(), dimension);
    }

    public void DrawImage(Image image, float x = 0, float y = 0)
    {
        _graphics.DrawImage(image, x, y);
    }

    public void DrawLine(Pen pen, PointF startPoint, PointF endPoint)
    {
        _graphics.DrawLine(pen, startPoint, endPoint);
    }

    public void Fill(Brush brush)
    {
        var graphicsState = _graphics.Save();
        Reset();
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.FillRectangle(brush, _graphics.VisibleClipBounds);
        _graphics.Restore(graphicsState);
    }

    public void DrawOver(GdiBitmap gdiBitmap)
    {
        _graphics.FillRectangle(gdiBitmap.TextureBrush, gdiBitmap._dimension);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _textureBrush?.Dispose();
        _textureBrush = null;
        _graphics.Dispose();
        Bitmap.Dispose();
    }
}
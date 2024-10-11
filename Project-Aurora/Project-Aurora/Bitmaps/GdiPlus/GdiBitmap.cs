using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using Common.Utils;

namespace AuroraRgb.Bitmaps.GdiPlus;

public sealed class GdiBitmap : IAuroraBitmap
{
    public static readonly GdiBitmap EmptyBitmap = new(1, 1);

    private TextureBrush? _textureBrush;

    private readonly Bitmap _bitmap;
    private readonly RectangleF _dimension;

    public Bitmap Bitmap => _bitmap;

    private readonly Graphics _graphics;

    private bool _disposed;

    internal Rectangle Dimension;
    private KeySequence _excludeSequence = new();

    public float Opacity { get; set; } = 1;

    //B, G, R, A
    private static readonly long[] ColorData = [0L, 0L, 0L, 0L];
    private static readonly Dictionary<Size, BitmapData> Bitmaps = new();
    // ReSharper disable once CollectionNeverQueried.Local //to keep reference
    private static readonly Dictionary<Size, int[]> BitmapBuffers = new();

    internal TextureBrush TextureBrush
    {
        get
        {
            if (_textureBrush != null) return _textureBrush;

            var colorMatrix = new ColorMatrix
            {
                Matrix33 = Opacity
            };
            using var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);
            imageAttributes.SetWrapMode(WrapMode.Clamp, Color.Empty);

            PerformExclude(_excludeSequence);

            if (_disposed)
            {
                return EmptyBitmap.TextureBrush;
            }

            _textureBrush = new TextureBrush(_bitmap, Dimension, imageAttributes);

            return _textureBrush;
        }
    }

    public GdiBitmap(int canvasWidth, int canvasHeight)
    {
        var graphicsUnit = GraphicsUnit.Pixel;

        _bitmap = new Bitmap(canvasWidth, canvasHeight);
        _dimension = _bitmap.GetBounds(ref graphicsUnit);
        _graphics = Graphics.FromImage(_bitmap);
        Dimension = new Rectangle(0, 0, canvasWidth, canvasHeight);

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
    
    /**
     * Gets average color of region, ignoring transparency
     * NOT thread-safe
     */
    public Color GetRegionColor(Rectangle rectangle)
    {
        if (rectangle.Width == 0 || rectangle.Height == 0 || !_dimension.Contains(rectangle))
            return Color.Black;

        ColorData[0] = 0L;
        ColorData[1] = 0L;
        ColorData[2] = 0L;
        ColorData[3] = 0L;

        if (!Bitmaps.TryGetValue(rectangle.Size, out var buff))
        {
            var bitmapBuffer = new int[rectangle.Width * rectangle.Height];
            BitmapBuffers[rectangle.Size] = bitmapBuffer;

            var buffer = Marshal.AllocHGlobal(bitmapBuffer.Length * sizeof(int));
            Marshal.Copy(bitmapBuffer, 0, buffer, bitmapBuffer.Length);
            // Create new bitmap data.
            buff = new BitmapData
            {
                Width = rectangle.Width,
                Height = rectangle.Height,
                PixelFormat = PixelFormat.Format32bppArgb,
                Stride = rectangle.Width * sizeof(int),
                Scan0 = buffer
            };
            
            Bitmaps[rectangle.Size] = buff;
        }

        var srcData = _bitmap.LockBits(
            rectangle,
            (ImageLockMode)5,   //ImageLockMode.UserInputBuffer | ImageLockMode.ReadOnly
            PixelFormat.Format32bppRgb, buff);
        var scan0 = srcData.Scan0;

        var rectangleHeight = rectangle.Height;
        var rectangleWidth = rectangle.Width;
        unsafe
        {
            var p = (byte*)(void*)scan0;

            var j = 0;
            for (var y = 0; y < rectangleHeight; y++)
            {
                for (var x = 0; x < rectangleWidth; x++)
                {
                    ColorData[0] += p[j++];
                    ColorData[1] += p[j++];
                    ColorData[2] += p[j++];
                    ColorData[3] += p[j++];
                }
            }
        }
        _bitmap.UnlockBits(srcData);

        var area = ColorData[3] / 255;
        if (area == 0)
        {
            return Color.Transparent;
        }
        return CommonColorUtils.FastColor(
            (byte) (ColorData[2] / area), (byte) (ColorData[1] / area), (byte) (ColorData[0] / area)
        );
    }

    public void Reset()
    {
        _graphics.ResetClip();
        _graphics.ResetTransform();
        _textureBrush?.Dispose();
        _textureBrush = null;
    }

    public void DrawRectangle(Brush brush, Rectangle dimension)
    {
        _graphics.FillRectangle(brush, dimension.X, dimension.Y, dimension.Width, dimension.Height);
    }

    public void DrawRectangle(Brush brush, RectangleF dimension)
    {
        _graphics.FillRectangle(brush, dimension.X, dimension.Y, dimension.Width, dimension.Height);
    }

    public void DrawRectangle(Pen pen, RectangleF dimension)
    {
        _graphics.DrawRectangle(pen, dimension);
    }

    public void DrawRectangle(EffectLayer brush)
    {
        var gdiBitmap = GetGdiBitmap(brush.GetBitmap());
        DrawOver(gdiBitmap);
    }

    public void DrawRectangle(EffectLayer brush, Rectangle dimension)
    {
        var gdiBitmap = GetGdiBitmap(brush.GetBitmap());
        DrawRectangle(gdiBitmap.TextureBrush, dimension);
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

    public static GdiBitmap GetGdiBitmap(IAuroraBitmap bitmap)
    {
        return bitmap switch
        {
            GdiBitmap gdiBitmap => gdiBitmap,
            RuntimeChangingBitmap runtimeChangingBitmap => runtimeChangingBitmap.GetGdiBitmap(),
            _ => throw new NotSupportedException("Only GdiBitmaps are supported.")
        };
    }

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

    public void DrawImage(Image image, float x = 0, float y = 0, float width = 0, float height = 0)
    {
        _graphics.DrawImage(image, x, y, width, height);
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
        _graphics.FillRectangle(gdiBitmap.TextureBrush, gdiBitmap.Dimension);
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
        _bitmap.Dispose();
    }
}
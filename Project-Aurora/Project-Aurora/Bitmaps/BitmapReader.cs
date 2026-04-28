using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AuroraRgb.Bitmaps.GdiPlus;
using Common.Utils;

namespace AuroraRgb.Bitmaps;

public sealed class BitmapReader
{
    private const int SmallestBufferLength = 32;

    private static readonly Dictionary<Size, BitmapData> Bitmaps = new(20);

    // ReSharper disable once CollectionNeverQueried.Local //to keep reference
    private static readonly Dictionary<Size, int[]> BitmapBuffers = new(20);
    private static readonly Color TransparentColor = Color.Transparent;

    private readonly Bitmap _bitmap;
    private readonly IGdiBitmap _gdiBitmap;
    private readonly RectangleF _dimension;
    private readonly GraphicsUnit _graphicsUnit = GraphicsUnit.Pixel;

    public BitmapReader(Bitmap bitmap, IGdiBitmap gdiBitmap)
    {
        _bitmap = bitmap;
        _gdiBitmap = gdiBitmap;
        _dimension = bitmap.GetBounds(ref _graphicsUnit);
    }

    /**
     * Optimized with SIMD instructions where available
     */
    public Color GetRegionColor(in Rectangle rectangle)
    {
        if (!_dimension.Contains(rectangle))
            return TransparentColor;

        var opacity = _gdiBitmap.Opacity;
        var area = rectangle.Width * rectangle.Height;
        var divider = area / opacity;
        var size = rectangle.Size;
        if (!Bitmaps.TryGetValue(size, out var buff))
        {
            buff = CreateBuffer(rectangle);
            Bitmaps[size] = buff;
        }

        var srcData = _bitmap.LockBits(
            rectangle,
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb,
            buff);

        var totals = GdiBitmapVectorUtils.ProcessPixels(srcData.Scan0, rectangle.Width, rectangle.Height, srcData.Stride);

        _bitmap.UnlockBits(srcData);

        return CommonColorUtils.FastColor(
            (byte)(totals.R / divider),
            (byte)(totals.G / divider),
            (byte)(totals.B / divider),
            (byte)(totals.A / divider)
        );
    }

    private static BitmapData CreateBuffer(in Rectangle rectangle)
    {
        var area = rectangle.Width * rectangle.Height;
        var bufferArea = Math.Max(area, SmallestBufferLength);
        var bitmapBuffer = new int[bufferArea];
        BitmapBuffers[rectangle.Size] = bitmapBuffer;

        var buffer = Marshal.AllocHGlobal(bitmapBuffer.Length * sizeof(int));
        Marshal.Copy(bitmapBuffer, 0, buffer, bitmapBuffer.Length);

        return new BitmapData
        {
            Width = rectangle.Width,
            Height = rectangle.Height,
            PixelFormat = PixelFormat.Format32bppArgb,
            Stride = rectangle.Width * sizeof(int),
            Scan0 = buffer
        };
    }
}
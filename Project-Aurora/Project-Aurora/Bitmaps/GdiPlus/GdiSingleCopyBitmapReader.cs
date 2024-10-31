using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Common.Utils;

namespace AuroraRgb.Bitmaps.GdiPlus;

public sealed class GdiSingleCopyBitmapReader : IBitmapReader
{
    //B, G, R, A
    private static readonly long[] ColorData = [0L, 0L, 0L, 0L];
    private static readonly Dictionary<Size, BitmapData> Bitmaps = new();
    // ReSharper disable once CollectionNeverQueried.Local //to keep reference
    private static readonly Dictionary<Size, int[]> BitmapBuffers = new();

    private readonly Bitmap _bitmap;
    private readonly Size _bitmapSize;
    private readonly RectangleF _dimension;

    private readonly BitmapData _srcData;
    private readonly IntPtr _scan0;
    
    private Color _transparentColor = Color.Transparent;
    private Color _currentColor = Color.Black;

    public GdiSingleCopyBitmapReader(Bitmap bitmap)
    {
        _bitmap = bitmap;
        var graphicsUnit = GraphicsUnit.Pixel;
        _dimension = bitmap.GetBounds(ref graphicsUnit);

        _bitmapSize = bitmap.Size;

        if (!Bitmaps.TryGetValue(_bitmapSize, out var buff))
        {
            buff = CreateBitmapData(_bitmapSize);

            Bitmaps[_bitmapSize] = buff;
        }

        var rectangle = new Rectangle(Point.Empty, _bitmapSize);
        _srcData = _bitmap.LockBits(
            rectangle,
            (ImageLockMode)5,   //ImageLockMode.UserInputBuffer | ImageLockMode.ReadOnly
            PixelFormat.Format32bppArgb, buff);
        _scan0 = _srcData.Scan0;
    }

    /**
     * Gets average color of region, ignoring transparency
     * NOT thread-safe
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly Color GetRegionColor(Rectangle rectangle)
    {
        if (rectangle.Width == 0 || rectangle.Height == 0 || !_dimension.Contains(rectangle))
            return ref _transparentColor;

        ColorData[0] = 0L;
        ColorData[1] = 0L;
        ColorData[2] = 0L;
        ColorData[3] = 0L;

        if (!Bitmaps.TryGetValue(rectangle.Size, out var buff))
        {
            buff = CreateBitmapData(rectangle.Size);

            Bitmaps[rectangle.Size] = buff;
        }

        var rectangleWidth = rectangle.Width;
        var rectangleBottom = rectangle.Bottom;
        var rectangleLeft = rectangle.Left;
        unsafe
        {
            var p = (byte*)(void*)_scan0;

            for (var y = rectangle.Top; y < rectangleBottom; y++)
            {
                var j = _bitmapSize.Width * y + rectangleLeft;
                var byteStart = j * 4;
                var byteEnd = (j + rectangleWidth) * 4;
                for (var x = byteStart; x < byteEnd; x += 4)
                {
                    ColorData[0] += p[x];
                    ColorData[1] += p[x + 1];
                    ColorData[2] += p[x + 2];
                    ColorData[3] += p[x + 3];
                }
            }
        }

        var area = ColorData[3] / 255;
        if (area == 0)
        {
            return ref _transparentColor;
        }
        _currentColor = CommonColorUtils.FastColor(
            (byte) (ColorData[2] / area), (byte) (ColorData[1] / area), (byte) (ColorData[0] / area)
        );
        return ref _currentColor;
    }

    private static BitmapData CreateBitmapData(Size size)
    {
        var bitmapBuffer = new int[size.Width * size.Height];
        BitmapBuffers[size] = bitmapBuffer;

        var buffer = Marshal.AllocHGlobal(bitmapBuffer.Length * sizeof(int));
        Marshal.Copy(bitmapBuffer, 0, buffer, bitmapBuffer.Length);
        // Create new bitmap data.
        var buff = new BitmapData
        {
            Width = size.Width,
            Height = size.Height,
            PixelFormat = PixelFormat.Format32bppArgb,
            Stride = size.Width * sizeof(int),
            Scan0 = buffer
        };
        return buff;
    }

    public void Dispose()
    {
        _bitmap.UnlockBits(_srcData);
    }
}
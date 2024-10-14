using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Common.Utils;

namespace AuroraRgb.Bitmaps.GdiPlus;

public sealed class GdiPartialCopyBitmapReader : IBitmapReader
{
    //B, G, R, A
    private static readonly long[] ColorData = [0L, 0L, 0L, 0L];
    private static readonly Dictionary<Size, BitmapData> Bitmaps = new(20);
    // ReSharper disable once CollectionNeverQueried.Local //to keep reference
    private static readonly Dictionary<Size, int[]> BitmapBuffers = new(20);

    private readonly Bitmap _bitmap;
    private readonly Size _bitmapSize = Size.Empty;
    private readonly RectangleF _dimension;

    public GdiPartialCopyBitmapReader(Bitmap bitmap)
    {
        _bitmap = bitmap;
        var graphicsUnit = GraphicsUnit.Pixel;
        _dimension = bitmap.GetBounds(ref graphicsUnit);

        _bitmapSize = bitmap.Size;
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

    public void Dispose()
    {
    }
}
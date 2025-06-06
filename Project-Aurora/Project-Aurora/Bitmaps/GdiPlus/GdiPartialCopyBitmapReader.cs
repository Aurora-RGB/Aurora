using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Common.Utils;

namespace AuroraRgb.Bitmaps.GdiPlus;

public sealed class GdiPartialCopyBitmapReader : IBitmapReader
{
    private const int SmallestBufferLength = 32;
    private static readonly Dictionary<Size, BitmapData> Bitmaps = new(20);

    // ReSharper disable once CollectionNeverQueried.Local //to keep reference
    private static readonly Dictionary<Size, int[]> BitmapBuffers = new(20);

    private readonly Bitmap _bitmap;
    private readonly RectangleF _dimension;
    private readonly double _opacity;
    private readonly Vector256<int> _zeroVector = Vector256<int>.Zero;

    private readonly Color _transparentColor = Color.Transparent;
    private Color _currentColor = Color.Black;

    private readonly int[] _emptySmallestBuffer = new int[SmallestBufferLength];

    public GdiPartialCopyBitmapReader(Bitmap bitmap, double opacity)
    {
        _bitmap = bitmap;
        _opacity = opacity;
        var graphicsUnit = GraphicsUnit.Pixel;
        _dimension = bitmap.GetBounds(ref graphicsUnit);
    }

    /**
     * Optimized with SIMD instructions where available
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly Color GetRegionColor(Rectangle rectangle)
    {
        if (rectangle.Width == 0 || rectangle.Height == 0 || !_dimension.Contains(rectangle))
            return ref _transparentColor;

        var area = rectangle.Width * rectangle.Height;
        var size = rectangle.Size;
        if (!Bitmaps.TryGetValue(size, out var buff))
        {
            buff = CreateBuffer(rectangle);
            Bitmaps[size] = buff;
        }

        if (area < SmallestBufferLength)
        {
            // clear the padded array
            Array.Copy(_emptySmallestBuffer, 0, BitmapBuffers[size], 0, _emptySmallestBuffer.Length);
        }

        var srcData = _bitmap.LockBits(
            rectangle,
            (ImageLockMode)5, //ImageLockMode.UserInputBuffer | ImageLockMode.ReadOnly
            PixelFormat.Format32bppArgb,
            buff);

        var bufferSize = Math.Max(area, SmallestBufferLength);
        var totals = ProcessPixels(srcData.Scan0, bufferSize);

        _bitmap.UnlockBits(srcData);

        var divider = area / _opacity;
        _currentColor = CommonColorUtils.FastColor(
            (byte)(totals.R / divider),
            (byte)(totals.G / divider),
            (byte)(totals.B / divider),
            (byte)(totals.A / divider)
        );
        return ref _currentColor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe (long R, long G, long B, long A) ProcessPixels(in IntPtr scan0, int area)
    {
        var p = (byte*)scan0;
        long sumB = 0, sumG = 0, sumR = 0, sumA = 0;

        if (Avx2.IsSupported && area >= 32)
        {
            var vectorSumB = _zeroVector;
            var vectorSumG = _zeroVector;
            var vectorSumR = _zeroVector;
            var vectorSumA = _zeroVector;

            // Process 32 pixels at a time (32 * 4 bytes = 128 bytes)
            var fullVector = Vector256.Create(0xFF);
            var vectorCount = area / 32;
            for (var i = 0; i < vectorCount; i++)
            {
                var offset = i * 128;
                var vector1 = Avx.LoadVector256((int*)(p + offset));
                var vector2 = Avx.LoadVector256((int*)(p + offset + 32));
                var vector3 = Avx.LoadVector256((int*)(p + offset + 64));
                var vector4 = Avx.LoadVector256((int*)(p + offset + 96));

                vectorSumB = Avx2.Add(vectorSumB, Avx2.And(vector1, fullVector));
                vectorSumG = Avx2.Add(vectorSumG, Avx2.And(Avx2.ShiftRightLogical(vector1, 8), fullVector));
                vectorSumR = Avx2.Add(vectorSumR, Avx2.And(Avx2.ShiftRightLogical(vector1, 16), fullVector));
                vectorSumA = Avx2.Add(vectorSumA, Avx2.And(Avx2.ShiftRightLogical(vector1, 24), fullVector));

                // Repeat for other vectors...
                vectorSumB = Avx2.Add(vectorSumB, Avx2.And(vector2, fullVector));
                vectorSumG = Avx2.Add(vectorSumG, Avx2.And(Avx2.ShiftRightLogical(vector2, 8), fullVector));
                vectorSumR = Avx2.Add(vectorSumR, Avx2.And(Avx2.ShiftRightLogical(vector2, 16), fullVector));
                vectorSumA = Avx2.Add(vectorSumA, Avx2.And(Avx2.ShiftRightLogical(vector2, 24), fullVector));

                vectorSumB = Avx2.Add(vectorSumB, Avx2.And(vector3, fullVector));
                vectorSumG = Avx2.Add(vectorSumG, Avx2.And(Avx2.ShiftRightLogical(vector3, 8), fullVector));
                vectorSumR = Avx2.Add(vectorSumR, Avx2.And(Avx2.ShiftRightLogical(vector3, 16), fullVector));
                vectorSumA = Avx2.Add(vectorSumA, Avx2.And(Avx2.ShiftRightLogical(vector3, 24), fullVector));

                vectorSumB = Avx2.Add(vectorSumB, Avx2.And(vector4, fullVector));
                vectorSumG = Avx2.Add(vectorSumG, Avx2.And(Avx2.ShiftRightLogical(vector4, 8), fullVector));
                vectorSumR = Avx2.Add(vectorSumR, Avx2.And(Avx2.ShiftRightLogical(vector4, 16), fullVector));
                vectorSumA = Avx2.Add(vectorSumA, Avx2.And(Avx2.ShiftRightLogical(vector4, 24), fullVector));
            }

            // Sum up the vector lanes
            sumB = SumVector256(vectorSumB);
            sumG = SumVector256(vectorSumG);
            sumR = SumVector256(vectorSumR);
            sumA = SumVector256(vectorSumA);

            // Process remaining pixels
            var processed = vectorCount * 32;
            var remaining = area - processed;
            p += processed * 4;
            area = remaining;
        }

        // Process remaining pixels or all pixels if AVX2 is not supported
        var end = area * 4;
        for (var j = 0; j < end;)
        {
            sumB += p[j++];
            sumG += p[j++];
            sumR += p[j++];
            sumA += p[j++];
        }

        return (sumR, sumG, sumB, sumA);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long SumVector256(in Vector256<int> vector)
    {
        var sum = 0L;
        for (var i = 0; i < 8; i++)
        {
            sum += vector.GetElement(i);
        }

        return sum;
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

    public void Dispose()
    {
    }
}
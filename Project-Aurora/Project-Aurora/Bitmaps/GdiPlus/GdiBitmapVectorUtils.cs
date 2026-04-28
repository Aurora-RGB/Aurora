using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace AuroraRgb.Bitmaps.GdiPlus;

public static class GdiBitmapVectorUtils
{

    public static unsafe (long R, long G, long B, long A) ProcessPixels(
        in IntPtr scan0,
        int width,
        int height,
        int stride)
    {
        var basePtr = (byte*)scan0;

        var sumB = Vector256<int>.Zero;
        var sumG = Vector256<int>.Zero;
        var sumR = Vector256<int>.Zero;
        var sumA = Vector256<int>.Zero;
        long tailR = 0, tailG = 0, tailB = 0, tailA = 0;
        var simdEnd = width - width % 32;

        for (var y = 0; y < height; y++)
        {
            var row = basePtr + y * stride;

            var x = 0;

            for (; x < simdEnd; x += 32)
            {
                var src0 = row + x * 4;
                var src1 = src0 + 16 * 4;

                var v0 = Avx.LoadVector256(src0).AsByte();
                var v1 = Avx.LoadVector256(src1).AsByte();

                SumAllChannels(v0, ref sumB, ref sumG, ref sumR, ref sumA);
                SumAllChannels(v1, ref sumB, ref sumG, ref sumR, ref sumA);
            }

            // scalar tail per row
            for (; x < width; x++)
            {
                var px = row + x * 4;
                tailB += px[0];
                tailG += px[1];
                tailR += px[2];
                tailA += px[3];
            }
        }

        return (
            HorizontalSum(sumR) + tailR,
            HorizontalSum(sumG) + tailG,
            HorizontalSum(sumB) + tailB,
            HorizontalSum(sumA) + tailA
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SumAllChannels(
        Vector256<byte> v,
        ref Vector256<int> sumB, ref Vector256<int> sumG,
        ref Vector256<int> sumR, ref Vector256<int> sumA)
    {
        var asInt = v.AsInt32();

        sumB = Avx2.Add(sumB, Avx2.And(asInt, Vector256.Create(0xFF)));
        sumG = Avx2.Add(sumG, Avx2.And(Avx2.ShiftRightLogical(asInt, 8), Vector256.Create(0xFF)));
        sumR = Avx2.Add(sumR, Avx2.And(Avx2.ShiftRightLogical(asInt, 16), Vector256.Create(0xFF)));
        sumA = Avx2.Add(sumA, Avx2.ShiftRightLogical(asInt, 24));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long HorizontalSum(Vector256<int> v)
    {
        return (long)v.GetElement(0)
               + v.GetElement(1)
               + v.GetElement(2)
               + v.GetElement(3)
               + v.GetElement(4)
               + v.GetElement(5)
               + v.GetElement(6)
               + v.GetElement(7);
    }
}
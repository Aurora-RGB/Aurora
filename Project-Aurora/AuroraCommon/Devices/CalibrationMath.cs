using System.Collections.Generic;

namespace Common.Devices;

/// <summary>
/// Fits a <see cref="ColorMatrix"/> from a set of (reference color -> matched target output) samples
/// using least squares, so a device's mixed colors visually match a reference device.
/// </summary>
public static class CalibrationMath
{
    /// <summary>
    /// Solves for the 3x3 matrix M (normalized RGB) minimizing the squared error of M * input ≈ output
    /// over all samples. A small ridge term keeps the system well-conditioned.
    /// </summary>
    public static ColorMatrix FitColorMatrix(IReadOnlyList<SimpleColor> inputs, IReadOnlyList<SimpleColor> outputs)
    {
        var n = System.Math.Min(inputs.Count, outputs.Count);
        if (n < 3)
        {
            return ColorMatrix.Identity;
        }

        // Normal equations: A = sum(c c^T); for each output channel k, rhs_k = sum(c * o_k).
        var a = new double[9];
        var rhs = new double[9]; // rhs[k*3 + j]

        for (var i = 0; i < n; i++)
        {
            double[] c = [inputs[i].R / 255.0, inputs[i].G / 255.0, inputs[i].B / 255.0];
            double[] o = [outputs[i].R / 255.0, outputs[i].G / 255.0, outputs[i].B / 255.0];

            for (var r = 0; r < 3; r++)
            {
                for (var col = 0; col < 3; col++)
                {
                    a[r * 3 + col] += c[r] * c[col];
                }
            }

            for (var k = 0; k < 3; k++)
            {
                for (var j = 0; j < 3; j++)
                {
                    rhs[k * 3 + j] += c[j] * o[k];
                }
            }
        }

        const double ridge = 1e-3;
        a[0] += ridge;
        a[4] += ridge;
        a[8] += ridge;

        if (!TryInvert3X3(a, out var inv))
        {
            return ColorMatrix.Identity;
        }

        var m = new double[9];
        for (var k = 0; k < 3; k++)
        {
            for (var j = 0; j < 3; j++)
            {
                var sum = 0.0;
                for (var l = 0; l < 3; l++)
                {
                    sum += inv[j * 3 + l] * rhs[k * 3 + l];
                }

                m[k * 3 + j] = sum;
            }
        }

        return new ColorMatrix(m);
    }

    private static bool TryInvert3X3(double[] a, out double[] inv)
    {
        var det =
            a[0] * (a[4] * a[8] - a[5] * a[7]) -
            a[1] * (a[3] * a[8] - a[5] * a[6]) +
            a[2] * (a[3] * a[7] - a[4] * a[6]);

        if (System.Math.Abs(det) < 1e-9)
        {
            inv = [];
            return false;
        }

        var d = 1.0 / det;
        inv =
        [
            (a[4] * a[8] - a[5] * a[7]) * d,
            (a[2] * a[7] - a[1] * a[8]) * d,
            (a[1] * a[5] - a[2] * a[4]) * d,
            (a[5] * a[6] - a[3] * a[8]) * d,
            (a[0] * a[8] - a[2] * a[6]) * d,
            (a[2] * a[3] - a[0] * a[5]) * d,
            (a[3] * a[7] - a[4] * a[6]) * d,
            (a[1] * a[6] - a[0] * a[7]) * d,
            (a[0] * a[4] - a[1] * a[3]) * d
        ];
        return true;
    }
}

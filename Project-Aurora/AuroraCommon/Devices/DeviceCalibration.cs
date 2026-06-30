using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Common.Devices;

/// <summary>
/// Per-device color calibration used to make devices from different models look the same.
/// <para>The runtime pipeline per channel is: per-channel brightness <see cref="CalibrationCurve"/>
/// (matches the brightness response), then a 3x3 <see cref="ColorMatrix"/> (matches the tonality of
/// mixed colors via cross-channel terms), then the manual multiplicative fine-tune
/// (Brightness / per-channel gain / Gamma).</para>
/// <see cref="DeviceCalibration.Identity"/> leaves colors untouched.
/// </summary>
[method: JsonConstructor]
public sealed record DeviceCalibration(
    double Brightness = 1.0,
    double RedGain = 1.0,
    double GreenGain = 1.0,
    double BlueGain = 1.0,
    double Gamma = 1.0,
    CalibrationCurve? RedCurve = null,
    CalibrationCurve? GreenCurve = null,
    CalibrationCurve? BlueCurve = null,
    ColorMatrix? Matrix = null,
    ColorSample[]? Samples = null)
{
    public static readonly DeviceCalibration Identity = new();

    // Cached lookup tables are kept outside the record so they don't participate in value equality.
    private static readonly ConcurrentDictionary<DeviceCalibration, CalibrationLookup> LookupCache = new();

    [JsonIgnore]
    public bool IsIdentity =>
        Brightness is 1.0 && RedGain is 1.0 && GreenGain is 1.0 && BlueGain is 1.0 && Gamma is 1.0 &&
        (RedCurve?.IsIdentity ?? true) && (GreenCurve?.IsIdentity ?? true) && (BlueCurve?.IsIdentity ?? true) &&
        (Matrix?.IsIdentity ?? true) && (Samples is null || Samples.Length == 0);

    /// <summary>
    /// Returns the cached lookup for this calibration. Resolve once per device update (not per led)
    /// and call <see cref="CalibrationLookup.Apply"/> to keep the update hot path allocation-free.
    /// </summary>
    public CalibrationLookup GetLookup()
    {
        // Dragging calibration sliders produces many transient values; keep the cache bounded.
        if (LookupCache.Count > 256)
        {
            LookupCache.Clear();
        }

        return LookupCache.GetOrAdd(this, static c => new CalibrationLookup(c));
    }

    /// <summary>
    /// Builds a calibration from the legacy multiplicative <see cref="SimpleColor"/> calibration,
    /// where 255 meant "no change" and lower values darkened the channel.
    /// </summary>
    public static DeviceCalibration FromLegacy(SimpleColor color) => new(
        RedGain: color.R / 255.0,
        GreenGain: color.G / 255.0,
        BlueGain: color.B / 255.0);
}

/// <summary>
/// A per-channel response curve sampled at a set of input levels, used to match a device's brightness
/// response to a reference device. Control points are (<see cref="Inputs"/>[i] -> <see cref="Outputs"/>[i]);
/// intermediate values are linearly interpolated, with an implicit 0 -> 0 anchor and clamping past the
/// last point.
/// </summary>
[method: JsonConstructor]
public sealed record CalibrationCurve(byte[] Inputs, byte[] Outputs)
{
    [JsonIgnore]
    public bool IsIdentity
    {
        get
        {
            if (Inputs.Length == 0 || Inputs.Length != Outputs.Length)
            {
                return true;
            }

            for (var i = 0; i < Inputs.Length; i++)
            {
                if (Inputs[i] != Outputs[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>Linearly interpolates the output for a given input level (0-255).</summary>
    public double Map(int input)
    {
        if (Inputs.Length == 0 || Inputs.Length != Outputs.Length)
        {
            return input;
        }

        if (input <= Inputs[0])
        {
            return Inputs[0] == 0 ? Outputs[0] : Outputs[0] * (input / (double)Inputs[0]);
        }

        for (var i = 1; i < Inputs.Length; i++)
        {
            if (input > Inputs[i])
            {
                continue;
            }

            var span = Inputs[i] - Inputs[i - 1];
            if (span == 0)
            {
                return Outputs[i];
            }

            var t = (input - Inputs[i - 1]) / (double)span;
            return Outputs[i - 1] + t * (Outputs[i] - Outputs[i - 1]);
        }

        return Outputs[^1];
    }
}

/// <summary>
/// A 3x3 color-correction matrix operating on normalized RGB (each channel in [0,1]), row-major:
/// out_r = m[0]*r + m[1]*g + m[2]*b, etc. Captures cross-channel tonality differences between devices
/// (e.g. a target whose purple is off even when its primaries match).
/// </summary>
[method: JsonConstructor]
public sealed record ColorMatrix(double[] Values)
{
    public static readonly ColorMatrix Identity = new([1, 0, 0, 0, 1, 0, 0, 0, 1]);

    [JsonIgnore]
    public bool IsIdentity
    {
        get
        {
            if (Values.Length != 9)
            {
                return true;
            }

            for (var i = 0; i < 9; i++)
            {
                var expected = i % 4 == 0 ? 1.0 : 0.0; // diagonal entries are indices 0,4,8
                if (Math.Abs(Values[i] - expected) > 1e-6)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

/// <summary>
/// A single calibration sample: the <see cref="Source"/> color sent and the <see cref="Output"/> the
/// user matched on the target device. Used to bake a non-linear 3D correction (see
/// <see cref="CalibrationLookup"/>) for mixed colors a single matrix can't fix.
/// </summary>
[method: JsonConstructor]
public sealed record ColorSample(SimpleColor Source, SimpleColor Output);

/// <summary>
/// Precomputed apply pipeline for a <see cref="DeviceCalibration"/>: per-channel brightness curve LUTs,
/// an optional 3x3 matrix and per-channel manual fine-tune LUTs. When <see cref="DeviceCalibration.Samples"/>
/// are present, a non-linear 3D lookup table is baked from the curve+matrix base plus local sample
/// corrections, and applied with trilinear interpolation.
/// </summary>
public sealed class CalibrationLookup
{
    private const int LutSize = 17; // grid nodes per axis of the baked 3D LUT

    private readonly byte[] _curveR = new byte[256];
    private readonly byte[] _curveG = new byte[256];
    private readonly byte[] _curveB = new byte[256];

    private readonly byte[] _manualR = new byte[256];
    private readonly byte[] _manualG = new byte[256];
    private readonly byte[] _manualB = new byte[256];

    private readonly double[]? _matrix;

    // baked 3D LUT (LutSize^3 nodes, interleaved RGB); null when no samples
    private readonly byte[]? _lut;

    internal CalibrationLookup(DeviceCalibration calibration)
    {
        BuildCurve(_curveR, calibration.RedCurve);
        BuildCurve(_curveG, calibration.GreenCurve);
        BuildCurve(_curveB, calibration.BlueCurve);

        var invGamma = calibration.Gamma <= 0 ? 1.0 : 1.0 / calibration.Gamma;
        BuildManual(_manualR, calibration.RedGain, calibration.Brightness, invGamma);
        BuildManual(_manualG, calibration.GreenGain, calibration.Brightness, invGamma);
        BuildManual(_manualB, calibration.BlueGain, calibration.Brightness, invGamma);

        _matrix = calibration.Matrix is { IsIdentity: false } m && m.Values.Length == 9 ? m.Values : null;

        if (calibration.Samples is { Length: > 0 } samples)
        {
            _lut = BakeLut(samples);
        }
    }

    public SimpleColor Apply(in SimpleColor color)
    {
        int r, g, b;
        if (_lut is { } lut)
        {
            (r, g, b) = TrilinearLookup(lut, color);
        }
        else
        {
            (r, g, b) = CurveMatrix(color);
        }

        return new SimpleColor(_manualR[r], _manualG[g], _manualB[b], color.A);
    }

    /// <summary>Applies the per-channel brightness curve then the 3x3 matrix (no manual stage).</summary>
    private (int r, int g, int b) CurveMatrix(in SimpleColor color)
    {
        int r = _curveR[color.R];
        int g = _curveG[color.G];
        int b = _curveB[color.B];

        if (_matrix is { } m)
        {
            var rn = r / 255.0;
            var gn = g / 255.0;
            var bn = b / 255.0;
            r = ClampByte((m[0] * rn + m[1] * gn + m[2] * bn) * 255.0);
            g = ClampByte((m[3] * rn + m[4] * gn + m[5] * bn) * 255.0);
            b = ClampByte((m[6] * rn + m[7] * gn + m[8] * bn) * 255.0);
        }

        return (r, g, b);
    }

    private byte[] BakeLut(ColorSample[] samples)
    {
        // local-correction kernel over normalized RGB space
        const double sigma = 0.22;
        const double anchor = 0.05; // keeps regions far from any sample on the curve+matrix base
        var twoSigmaSq = 2.0 * sigma * sigma;

        var n = samples.Length;
        var sr = new double[n];
        var sg = new double[n];
        var sb = new double[n];
        var dr = new double[n];
        var dg = new double[n];
        var db = new double[n];
        for (var i = 0; i < n; i++)
        {
            var src = samples[i].Source;
            var dst = samples[i].Output;
            var (br, bg, bb) = CurveMatrix(src);
            sr[i] = src.R / 255.0;
            sg[i] = src.G / 255.0;
            sb[i] = src.B / 255.0;
            // correction = desired output - base(source)
            dr[i] = dst.R - br;
            dg[i] = dst.G - bg;
            db[i] = dst.B - bb;
        }

        var lut = new byte[LutSize * LutSize * LutSize * 3];
        var idx = 0;
        for (var ri = 0; ri < LutSize; ri++)
        for (var gi = 0; gi < LutSize; gi++)
        for (var bi = 0; bi < LutSize; bi++)
        {
            var node = new SimpleColor(GridLevel(ri), GridLevel(gi), GridLevel(bi));
            var (baseR, baseG, baseB) = CurveMatrix(node);

            var rn = ri / (double)(LutSize - 1);
            var gn = gi / (double)(LutSize - 1);
            var bn = bi / (double)(LutSize - 1);

            double wSum = anchor, cr = 0, cg = 0, cb = 0;
            for (var i = 0; i < n; i++)
            {
                var d2 = (rn - sr[i]) * (rn - sr[i]) + (gn - sg[i]) * (gn - sg[i]) + (bn - sb[i]) * (bn - sb[i]);
                var w = Math.Exp(-d2 / twoSigmaSq);
                wSum += w;
                cr += w * dr[i];
                cg += w * dg[i];
                cb += w * db[i];
            }

            lut[idx++] = (byte)ClampDouble(baseR + cr / wSum);
            lut[idx++] = (byte)ClampDouble(baseG + cg / wSum);
            lut[idx++] = (byte)ClampDouble(baseB + cb / wSum);
        }

        return lut;
    }

    private static byte GridLevel(int i) => (byte)ClampDouble(i * 255.0 / (LutSize - 1));

    private static (int r, int g, int b) TrilinearLookup(byte[] lut, in SimpleColor color)
    {
        var scale = LutSize - 1;
        var fr = color.R / 255.0 * scale;
        var fg = color.G / 255.0 * scale;
        var fb = color.B / 255.0 * scale;

        int r0 = (int)fr, g0 = (int)fg, b0 = (int)fb;
        int r1 = Math.Min(r0 + 1, scale), g1 = Math.Min(g0 + 1, scale), b1 = Math.Min(b0 + 1, scale);
        double tr = fr - r0, tg = fg - g0, tb = fb - b0;

        double outR = 0, outG = 0, outB = 0;
        for (var c = 0; c < 8; c++)
        {
            var ri = (c & 1) == 0 ? r0 : r1;
            var gi = (c & 2) == 0 ? g0 : g1;
            var bi = (c & 4) == 0 ? b0 : b1;
            var wr = (c & 1) == 0 ? 1 - tr : tr;
            var wg = (c & 2) == 0 ? 1 - tg : tg;
            var wb = (c & 4) == 0 ? 1 - tb : tb;
            var w = wr * wg * wb;

            var o = ((ri * LutSize + gi) * LutSize + bi) * 3;
            outR += w * lut[o];
            outG += w * lut[o + 1];
            outB += w * lut[o + 2];
        }

        return (ClampByte(outR), ClampByte(outG), ClampByte(outB));
    }

    private static void BuildCurve(byte[] table, CalibrationCurve? curve)
    {
        var hasCurve = curve is { IsIdentity: false };
        for (var i = 0; i < 256; i++)
        {
            table[i] = hasCurve ? (byte)ClampByte(curve!.Map(i)) : (byte)i;
        }
    }

    private static void BuildManual(byte[] table, double gain, double brightness, double invGamma)
    {
        for (var i = 0; i < 256; i++)
        {
            var normalized = Math.Pow(i / 255.0, invGamma) * brightness * gain;
            table[i] = (byte)ClampDouble(normalized * 255.0);
        }
    }

    private static int ClampByte(double value) => (int)ClampDouble(value);

    private static double ClampDouble(double value) => value switch
    {
        >= 255.0 => 255.0,
        <= 0.0 => 0.0,
        _ => Math.Round(value)
    };
}

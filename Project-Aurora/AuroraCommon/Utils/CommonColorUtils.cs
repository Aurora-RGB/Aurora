using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Common.Utils;

/// <summary>
/// Various color utilities
/// </summary>
public static class CommonColorUtils
{
    private static readonly Random Randomizer = new();

    /// <summary>
    /// Multiplies a byte by a specified double balue
    /// </summary>
    /// <param name="color">Part of the color, as a byte</param>
    /// <param name="value">The value to multiply the byte by</param>
    /// <returns>The color byte</returns>
    public static byte ColorByteMultiplication(byte color, double value)
    {
        var val = (int)(color * value);

        if (val > 255)
            return 255;
        if (val < 0)
            return 0;

        return (byte) val;
    }

    /// <summary>
    /// Blends two colors together by a specified amount
    /// </summary>
    /// <param name="background">The background color (When percent is at 0.0D, only this color is shown)</param>
    /// <param name="foreground">The foreground color (When percent is at 1.0D, only this color is shown)</param>
    /// <param name="percent">The blending percent value</param>
    /// <returns>The blended color</returns>
    public static Color BlendColors(in Color background, in Color foreground, double percent)
    {
        if (percent < 0.0)
            percent = 0.0;
        else if (percent > 1.0)
            percent = 1.0;

        var red = (byte)Math.Min(foreground.R * percent + background.R * (1.0 - percent), 255);
        var green = (byte)Math.Min(foreground.G * percent + background.G * (1.0 - percent), 255);
        var blue = (byte)Math.Min(foreground.B * percent + background.B * (1.0 - percent), 255);
        var alpha = (byte)Math.Min(foreground.A * percent + background.A * (1.0 - percent), 255);

        return FastColor(red, green, blue, alpha);
    }

    /// <summary>
    /// Blends two colors together by a specified amount
    /// </summary>
    /// <param name="background">The background color (When percent is at 0.0D, only this color is shown)</param>
    /// <param name="foreground">The foreground color (When percent is at 1.0D, only this color is shown)</param>
    /// <param name="percent">The blending percent value</param>
    /// <returns>The blended color</returns>
    public static SimpleColor BlendColors(in SimpleColor background, in SimpleColor foreground, double percent)
    {
        if (percent < 0.0)
            percent = 0.0;
        else if (percent > 1.0)
            percent = 1.0;

        var red = (byte)Math.Min(foreground.R * percent + background.R * (1.0 - percent), 255);
        var green = (byte)Math.Min(foreground.G * percent + background.G * (1.0 - percent), 255);
        var blue = (byte)Math.Min(foreground.B * percent + background.B * (1.0 - percent), 255);
        var alpha = (byte)Math.Min(foreground.A * percent + background.A * (1.0 - percent), 255);

        return new SimpleColor(red, green, blue, alpha);
    }

    /// <summary>
    /// Adds two colors together by using the "SRC over DST" blending
    /// </summary>
    /// <param name="background">The background color</param>
    /// <param name="foreground">The foreground color</param>
    /// <returns>The sum of two colors including combined alpha</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color AddColors(in Color background, in Color foreground)
    {
        var foreA = foreground.A;
        var backA = background.A;
        var aDraw = foreA / 255f;
        var aBase = backA / 255f * (1 - aDraw);

        // Convert Color structs to Vector4 (R, G, B, A) representation
        var drawVec = new Vector4(foreground.R, foreground.G, foreground.B, foreA);
        var baseVec = new Vector4(background.R, background.G, background.B, backA);

        // Multiply colors with their respective alpha values
        var resultVec = drawVec * aDraw + baseVec * aBase;

        var r = (byte)resultVec.X;
        var g = (byte)resultVec.Y;
        var b = (byte)resultVec.Z;
        var a = (byte)((aDraw + aBase) * 255);

        return FastColor(r, g, b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Color AddColors(in Color background, in Color foreground, ref Color resultCache)
    {
        var foreA = foreground.A;
        var backA = background.A;
        var aDraw = foreA / 255f;
        var aBase = backA / 255f * (1 - aDraw);

        // Convert Color structs to Vector4 (R, G, B, A) representation
        var drawVec = new Vector4(foreground.R, foreground.G, foreground.B, foreA);
        var baseVec = new Vector4(background.R, background.G, background.B, backA);

        // Multiply colors with their respective alpha values
        var resultVec = drawVec * aDraw + baseVec * aBase;

        var r = (byte)resultVec.X;
        var g = (byte)resultVec.Y;
        var b = (byte)resultVec.Z;
        var a = (byte)((aDraw + aBase) * 255);

        resultCache = FastColor(r, g, b, a);
        return ref resultCache;
    }

    public static SimpleColor CorrectWithAlpha(in SimpleColor color)
    {
        var scalar = color.A / 255.0f;

        var red = ColorByteMultiplication(color.R, scalar);
        var green = ColorByteMultiplication(color.G, scalar);
        var blue = ColorByteMultiplication(color.B, scalar);

        return new SimpleColor(red, green, blue);
    }

    /// <summary>
    /// Multiplies a Drawing Color instance by a scalar value
    /// </summary>
    /// <param name="color">The color to be multiplied</param>
    /// <param name="scalar">The scalar amount for multiplication</param>
    /// <returns>The multiplied Color</returns>
    public static Color MultiplyColorByScalar(in Color color, double scalar)
    {
        var red = color.R;
        var green = color.G;
        var blue = color.B;
        var alpha = ColorByteMultiplication(color.A, scalar);

        return FastColor(red, green, blue, alpha);
    }
    
    public static SimpleColor MultiplyColorByScalar(in SimpleColor color, double scalar)
    {
        var red = color.R;
        var green = color.G;
        var blue = color.B;
        var alpha = ColorByteMultiplication(color.A, scalar);

        return new SimpleColor(red, green, blue, alpha);
    }

    /// <summary>
    /// Generates a random color
    /// </summary>
    /// <returns>A random color</returns>
    public static Color GenerateRandomColor()
    {
        return FastColor((byte)Randomizer.Next(255), 
            (byte)Randomizer.Next(255), 
            (byte)Randomizer.Next(255)
        );
    }

    public static Color GetColorFromInt(int integer)
    {
        integer = integer switch
        {
            < 0 => 0,
            > 16777215 => 16777215,
            _ => integer
        };

        var r = integer >> 16;
        var g = (integer >> 8) & 255;
        var b = integer & 255;

        return FastColor((byte)r, (byte)g, (byte)b);
    }

    public static int GetIntFromColor(in Color color)
    {
        return (color.R << 16) | (color.G << 8) | color.B;
    }

    public static void ToHsv(in Color color, out double hue, out double saturation, out double value)
    {
        ToHsv((color.R, color.G, color.B), out hue, out saturation, out value);
    }

    public static void ToHsv((byte r, byte g, byte b) color, out double hue, out double saturation, out double value)
    {
        var max = Math.Max(color.r, Math.Max(color.g, color.b));
        var min = Math.Min(color.r, Math.Min(color.g, color.b));

        var delta = max - min;

        hue = 0d;
        if (delta != 0)
        {
            if (color.r == max) hue = (color.g - color.b) / (double)delta;
            else if (color.g == max) hue = 2d + (color.b - color.r) / (double)delta;
            else if (color.b == max) hue = 4d + (color.r - color.g) / (double)delta;
        }

        hue *= 60;
        if (hue < 0.0) hue += 360;

        saturation = max == 0 ? 0 : 1d - 1d * min / max;
        value = max / 255d;
    }

    public static Color FromHsv(double hue, double saturation, double value)
    {
        saturation = Math.Max(Math.Min(saturation, 1), 0);
        value = Math.Max(Math.Min(value, 1), 0);

        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = (byte)(value);
        var p = (byte)(value * (1 - saturation));
        var q = (byte)(value * (1 - f * saturation));
        var t = (byte)(value * (1 - (1 - f) * saturation));

        switch (hi)
        {
            case 0: return FastColor(v, t, p);
            case 1: return FastColor(q, v, p);
            case 2: return FastColor(p, v, t);
            case 3: return FastColor(p, q, v);
            case 4: return FastColor(t, p, v);
            default: return FastColor(v, p, q);
        }
    }

    /// <summary>
    /// Changes the hue of <paramref name="color"/>
    /// </summary>
    /// <param name="color">Color to be modified</param>
    /// <param name="offset">Hue offset in degrees</param>
    /// <returns>Color with modified hue</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ChangeHue(in Color color, double offset)
    {
        if (offset == 0)
            return color;

        ToHsv(color, out var hue, out var saturation, out var value);

        hue += offset;

        while (hue > 360) hue -= 360;
        while (hue < 0) hue += 360;

        return FromHsv(hue, saturation, value);
    }

    /// <summary>
    /// Changes the brightness of <paramref name="color"/>
    /// </summary>
    /// <param name="color">Color to be modified</param>
    /// <param name="strength">
    /// The strength of brightness change.
    /// <para>Values between (0, 1] increase the brightness by (0%, inf%]</para>
    /// <para>Values between [-1, 0) decrease the brightness by [inf%, 0%)</para>
    /// </param>
    /// <returns>Color with modified brightness</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ChangeBrightness(in Color color, double strength)
    {
        if (strength == 0)
            return color;

        ToHsv(color, out var hue, out var saturation, out var value);
        ChangeHsvComponent(ref value, strength);
        return FromHsv(hue, saturation, value);
    }

    /// <summary>
    /// Changes the saturation of <paramref name="color"/>
    /// </summary>
    /// <param name="color">Color to be modified</param>
    /// <param name="strength">
    /// The strength of saturation change.
    /// <para>Values between (0, 1] increase the saturation by (0%, inf%]</para>
    /// <para>Values between [-1, 0) decrease the saturation by [inf%, 0%)</para>
    /// </param>
    /// <returns>Color with modified saturation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ChangeSaturation(in Color color, double strength)
    {
        if (strength == 0)
            return color;

        ToHsv(color, out var hue, out var saturation, out var value);
        ChangeHsvComponent(ref saturation, strength);
        return FromHsv(hue, saturation, value);
    }

    private static void ChangeHsvComponent(ref double component, double strength)
    {
        if (component == 0)
            return;

        strength = strength >= 0 ? MathUtils.Clamp(strength, 0, 1) : MathUtils.Clamp(strength, -1, 0);
        if (strength <= -1)
        {
            component = 0;
            return;
        }

        if (strength >= 1)
        {
            component = 1;
            return;
        }

        var result = strength >= 0 ? component / (1 - Math.Sin(Math.PI * strength / 2))
            : component * (1 - Math.Sin(-Math.PI * strength / 2));
        component = MathUtils.Clamp(result, 0, 1);
    }

    public static Color CloneColor(in Color clr)
    {
        return Color.FromArgb(clr.ToArgb());
    }

    public static Color FastColorTransparent(byte r, byte g, byte b)
    {
        var brightness = Math.Max(b, Math.Max(g, r));
        var normalizer = 255d / brightness;
        return FastColor((byte)(r * normalizer), (byte)(g * normalizer), (byte)(b * normalizer), brightness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color FastColor(byte r, byte g, byte b, byte a = 255)
    {
        return Color.FromArgb(
            b | (g << 8) | (r << 16) | (a << 24)
        );
    }

    public static Color FastColor(int colors)
    {
        return Color.FromArgb(colors);
    }
}
using System.Drawing;
using System.Text.Json.Serialization;
using Common.Utils;

namespace Common;

[method: JsonConstructor]
public readonly record struct SimpleColor(byte R, byte G, byte B, byte A = 255)
{
    public static readonly SimpleColor Transparent = new(0, 0, 0, 0);
    public static readonly SimpleColor White = new(255, 255, 255);
    public static readonly SimpleColor Black = new(0, 0, 0);

    private const int ArgbAlphaShift = 24;
    private const int ArgbRedShift = 16;
    private const int ArgbGreenShift = 8;
    private const int ArgbBlueShift = 0;
    
    public int ToArgb() => (A << ArgbAlphaShift) | (R << ArgbRedShift) | (G << ArgbGreenShift) | B << ArgbBlueShift;

    public static explicit operator Color(SimpleColor color)
    {
        return CommonColorUtils.FastColor(color.R, color.G, color.B, color.A);
    }

    public static explicit operator SimpleColor(Color color)
    {
        return new SimpleColor(color.R, color.G, color.B, color.A);
    }

    public static SimpleColor FromArgb(int argb)
    {
        var r = unchecked((byte)(argb >> ArgbRedShift));
        var g = unchecked((byte)(argb >> ArgbGreenShift));
        var b = unchecked((byte)(argb >> ArgbBlueShift));
        var a = unchecked((byte)(argb >> ArgbAlphaShift));

        return new SimpleColor(r, g, b, a);
    }

    public static SimpleColor FromRgba(byte red, byte green, byte blue, byte alpha = 255)
    {
        return new SimpleColor(red, green, blue, alpha);
    }

    public static SimpleColor operator *(SimpleColor color, double scalar)
    {
        return color with { A = ColorByteMultiplication(color.A, scalar) };
    }

    public static SimpleColor operator /(SimpleColor color, double scalar)
    {
        var r = ColorByteMultiplication(color.R, 1.0 / scalar);
        var g = ColorByteMultiplication(color.G, 1.0 / scalar);
        var b = ColorByteMultiplication(color.B, 1.0 / scalar);
        var a = ColorByteMultiplication(color.A, 1.0 / scalar);

        return new SimpleColor(r, g, b, a);
    }

    private static byte ColorByteMultiplication(byte color, double value)
    {
        return (color * value) switch
        {
            >= 255.0 => 255,
            <= 0.0 => 0,
            _ => (byte)(color * value)
        };
    }
}
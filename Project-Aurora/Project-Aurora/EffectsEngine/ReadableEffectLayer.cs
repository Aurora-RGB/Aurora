using System.Drawing;
using AuroraRgb.Bitmaps;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public class ReadableEffectLayer(string name, Color color) : EffectLayer(name, color, LayerReadability.Readable)
{
    private IBitmapReader? _bitmapReader;

    /// <summary>
    /// Retrieves a color of the specified DeviceKeys key from the bitmap
    /// </summary>
    public Color Get(DeviceKeys key)
    {
        var keyRectangle = Effects.Canvas.GetRectangle(key);

        var keyColor = keyRectangle.IsEmpty switch
        {
            true => Color.Black,
            false => GetColor(keyRectangle.Rectangle)
        };
        return keyColor;
    }

    private Color GetColor(Rectangle rectangle)
    {
        _bitmapReader ??= _colormap.CreateReader();
        return _bitmapReader.GetRegionColor(rectangle);
    }

    public void Close()
    {
        _bitmapReader?.Dispose();
        _bitmapReader = null;
    }
}
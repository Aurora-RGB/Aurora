using System.Collections.Generic;
using System.Drawing;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public class ReadableEffectLayer(string name, Color color) : EffectLayer(name, color, true)
{
    private readonly Dictionary<DeviceKeys, Color> _keyColors = new(Effects.MaxDeviceId);

    /// <summary>
    /// Retrieves a color of the specified DeviceKeys key from the bitmap
    /// </summary>
    public Color Get(DeviceKeys key)
    {
        if (_keyColors.TryGetValue(key, out var color))
        {
            return color;
        }

        var keyRectangle = Effects.Canvas.GetRectangle(key);

        var keyColor = keyRectangle.IsEmpty switch
        {
            true => Color.Black,
            false => _colormap.GetRegionColor(keyRectangle.Rectangle)
        };
        _keyColors[key] = keyColor;
        return keyColor;
    }

    public override void Invalidate()
    {
        base.Invalidate();

        _keyColors.Clear();
    }

    protected override void SetOneKey(DeviceKeys key, Brush brush)
    {
        if (brush is SolidBrush solidBrush)
        {
            if (_keyColors.TryGetValue(key, out var currentColor) && currentColor.ToArgb() == solidBrush.Color.ToArgb())
            {
                return;
            }

            _keyColors[key] = solidBrush.Color;
        }

        base.SetOneKey(key, brush);
    }

    public override void Dispose()
    {
        base.Dispose();
        
        _keyColors.Clear();
    }
}
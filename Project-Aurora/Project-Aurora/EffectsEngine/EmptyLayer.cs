using System.Collections.Generic;
using System.Drawing;
using AuroraRgb.Settings;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public sealed class EmptyLayer : EffectLayer
{
    public static readonly EmptyLayer Instance = new();
    private static readonly Color TransparentColor = Color.Transparent;

    private EmptyLayer()
    {
    }

    public void Dispose()
    {
    }

    public DeviceKeys[] ActiveKeys { get; } = [];

    public void Fill(ref readonly Color color)
    {
    }

    public void FillOver(ref readonly Color color)
    {
    }

    public void Clear()
    {
    }

    public void Set(DeviceKeys key, ref readonly Color color)
    {
    }

    public void Set(DeviceKeys[] keys, ref readonly Color color)
    {
    }

    public void Set(KeySequence sequence, ref readonly Color color)
    {
    }

    public void PercentEffect(Color foregroundColor, Color backgroundColor, IReadOnlyList<DeviceKeys> keys, double value, double total,
        PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0, bool flashReversed = false, bool blinkBackground = false)
    {
    }

    public EffectLayer Add(EffectLayer other)
    {
        return other;
    }

    public void Exclude(KeySequence sequence)
    {
    }

    public void OnlyInclude(KeySequence sequence)
    {
    }

    public void SetOpacity(double layerOpacity)
    {
    }

    public Color Get(DeviceKeys key)
    {
        return TransparentColor;
    }

    public void Close()
    {
    }
}
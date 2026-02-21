using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AuroraRgb.Settings;
using Common;
using Common.Devices;
using Common.Utils;

namespace AuroraRgb.EffectsEngine;

public sealed class NoRenderLayer : EffectLayer
{
    private static readonly Color Transparent = Color.Transparent;
    private static DeviceKeys[] AllKeys => Effects.Canvas.Keys;
    private readonly Color[] _emptyKeyColors = new Color[Constants.MaxKeyId + 1];

    private readonly Color[] _keyColors = new Color[Constants.MaxKeyId + 1];
    private double _opacity = 1;
    private bool _isOpaque = true;

    private Color _colorCache;
    
    // TODO optimize a lot by reducing the result of this
    public DeviceKeys[] ActiveKeys => Effects.Canvas.Keys;
    
    private readonly ZoneKeysCache _zoneKeysCache = new();
    private readonly ZoneKeysCache _excludedZoneKeysCache = new();
    private ZoneKeysCache? _onlyIncludedZoneKeysCache;

    public NoRenderLayer()
    {
        _zoneKeysCache.KeysChanged += (_, _) => Clear();
    }

    public void Fill(ref readonly Color color)
    {
        // set all _keyColors to Transparent
        foreach (var key in AllKeys)
            Set(key, in color);
    }

    public void FillOver(ref readonly Color color)
    {
        foreach (var deviceKey in AllKeys)
        {
            SetOver(deviceKey, in color);
        }
    }

    public void Clear()
    {
        Array.Copy(_emptyKeyColors, _keyColors, _emptyKeyColors.Length);
        _zoneKeysCache.Invalidate();
    }

    public void Set(DeviceKeys key, ref readonly Color color)
    {
        if (key == DeviceKeys.NONE)
        {
            return;
        }

        _keyColors[(int)key] = color;
    }

    public void Set(ICollection<DeviceKeys> keys, ref readonly Color color)
    {
        foreach (var deviceKeys in keys)
        {
            Set(deviceKeys, in color);
        }
    }

    public void Set(KeySequence sequence, ref readonly Color color)
    {
        _zoneKeysCache.SetSequence(sequence);
        var keys = _zoneKeysCache.GetKeys();
        Set(keys, in color);
    }

    public EffectLayer Add(EffectLayer other)
    {
        if (other == EmptyLayer.Instance)
        {
            return this;
        }

        // magic iteration from https://blog.ndepend.com/c-array-and-list-fastest-loop/
        Span<DeviceKeys> span = other.ActiveKeys;
        ref var start = ref MemoryMarshal.GetReference(span);
        ref var end = ref Unsafe.Add(ref start, span.Length);
 
        while (Unsafe.IsAddressLessThan(ref start, ref end)) {
            var foregroundColor = other.Get(start);
            SetOver(start, in foregroundColor);
            start = ref Unsafe.Add(ref start, 1);
        }

        return this;
    }

    private void SetOver(DeviceKeys key, ref readonly Color foregroundColor)
    {
        switch (foregroundColor.A)
        {
            // If the drawn color is fully opaque, draw it directly
            case 255:
            {
                Set(key, in foregroundColor);
                return;
            }
            // If the drawn color is fully transparent, do nothing
            case 0:
            {
                return;
            }
        }

        var backgroundColor = Get(key);
        ref var newColor = ref CommonColorUtils.AddColors(in backgroundColor, in foregroundColor, ref _colorCache);
        Set(key, in newColor);
    }

    public void Exclude(KeySequence sequence)
    {
        _excludedZoneKeysCache.SetSequence(sequence);
    }

    public void OnlyInclude(KeySequence sequence)
    {
        _onlyIncludedZoneKeysCache ??= new ZoneKeysCache();
        _onlyIncludedZoneKeysCache.SetSequence(sequence);
    }

    public void SetOpacity(double layerOpacity)
    {
        _opacity = layerOpacity;
        _isOpaque = _opacity > 0.999;
    }

    public Color Get(DeviceKeys key)
    {
        if (KeyExcluded(key))
        {
            return Transparent;
        }
        
        if (_isOpaque)
        {
            return GetCurrentColor(key);
        }

        var color = GetCurrentColor(key);
        var a = (byte)(color.A * _opacity);
        return CommonColorUtils.FastColor(color.R, color.G, color.B, a);
    }

    private bool KeyExcluded(DeviceKeys key)
    {
        if (_excludedZoneKeysCache.GetKeys().Contains(key)) return true;
        if (_onlyIncludedZoneKeysCache == null) return false;
        return !_onlyIncludedZoneKeysCache.GetKeys().Contains(key);
    }

    public void Dispose()
    {
        _zoneKeysCache.Dispose();
        _excludedZoneKeysCache.Dispose();
        _onlyIncludedZoneKeysCache?.Dispose();
    }

    private Color GetCurrentColor(DeviceKeys deviceKey)
    {
        if (deviceKey == DeviceKeys.NONE)
        {
            return Transparent;
        }

        return _keyColors[(int)deviceKey];
    }
}
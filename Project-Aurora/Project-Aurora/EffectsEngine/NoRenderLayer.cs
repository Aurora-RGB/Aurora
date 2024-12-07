﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;

namespace AuroraRgb.EffectsEngine;

public sealed class NoRenderLayer : EffectLayer
{
    private static readonly Color Transparent = Color.Transparent;
    private static DeviceKeys[] AllKeys => Effects.Canvas.Keys;
    private readonly Color[] _emptyKeyColors = new Color[Effects.MaxDeviceId + 1];

    private readonly Color[] _keyColors = new Color[Effects.MaxDeviceId + 1];
    private double _opacity = 1;
    private bool _isOpaque = true;

    private Color _lastColor = Color.Transparent;
    private double _percentProgress = -1;
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

    public void Set(DeviceKeys[] keys, ref readonly Color color)
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

    /// <summary>
    /// Draws a percent effect on the layer bitmap using an array of DeviceKeys keys and solid colors.
    /// </summary>
    /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    public void PercentEffect(Color foregroundColor, Color backgroundColor, IReadOnlyList<DeviceKeys> keys, double value,
        double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
        bool flashReversed = false, bool blinkBackground = false)
    {
        var progressTotal = value / total;
        if (progressTotal < 0.0)
            progressTotal = 0.0;
        else if (progressTotal > 1.0)
            progressTotal = 1.0;

        var progress = progressTotal * keys.Count;

        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            if (blinkBackground)
                backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
            else
                foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
        }

        if (percentEffectType is PercentEffectType.Highest_Key or PercentEffectType.Highest_Key_Blend && keys.Count > 0)
        {
            var activeKey = (int)Math.Ceiling(Math.Clamp(value, 0, 1) / (total / keys.Count)) - 1;
            var col = percentEffectType == PercentEffectType.Highest_Key ?
                foregroundColor : ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal);
            for (var i = 0; i < keys.Count; i++)
            {
                if (i != activeKey)
                {
                    Set(keys[i], Color.Transparent);
                }
            }
            Set(keys[activeKey], in col);

        }
        else
        {
            for (var i = 0; i < keys.Count; i++)
            {
                var currentKey = keys[i];

                switch (percentEffectType)
                {
                    case PercentEffectType.AllAtOnce:
                        Set(currentKey, ColorUtils.BlendColors(in backgroundColor, in foregroundColor, progressTotal));
                        break;
                    case PercentEffectType.Progressive_Gradual:
                        if (i == (int)progress)
                        {
                            var percent = progress - i;
                            Set(currentKey, ColorUtils.BlendColors(in backgroundColor, in foregroundColor, percent));
                        }
                        else if (i < (int)progress)
                            Set(currentKey, foregroundColor);
                        else
                            Set(currentKey, backgroundColor);
                        break;
                    default:
                        Set(currentKey, i < (int) progress ? foregroundColor : backgroundColor);
                        break;
                }
            }
        }
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

    public void Close()
    {
    }

    public void Dispose()
    {
        _zoneKeysCache.Dispose();
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
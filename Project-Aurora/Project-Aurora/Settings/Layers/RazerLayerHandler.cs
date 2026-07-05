using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;
using RazerSdkReader.Structures;

namespace AuroraRgb.Settings.Layers;

public partial class RazerLayerHandlerProperties : LayerHandlerProperties
{

    [JsonIgnore]
    private bool? _transparencyEnabled;
    [JsonProperty("_TransparencyEnabled")]
    [LogicOverridable("Enable Transparency")]
    public bool TransparencyEnabled
    {
        get => Logic?._transparencyEnabled ?? false;
        set => _transparencyEnabled = value;
    }

    private bool? _colorPostProcessEnabled;
    [JsonProperty("_ColorPostProcessEnabled")]
    public bool ColorPostProcessEnabled
    {
        get => Logic?._colorPostProcessEnabled ?? _colorPostProcessEnabled ?? false;
        set => _colorPostProcessEnabled = value;
    }

    private double? _brightnessChange;
    [JsonProperty("_BrightnessChange")]
    public double BrightnessChange
    {
        get => Logic?._brightnessChange ?? _brightnessChange ?? 0;
        set => _brightnessChange = value;
    }

    private double? _saturationChange;
    [JsonProperty("_SaturationChange")]
    public double SaturationChange
    {
        get => Logic?._saturationChange ?? _saturationChange ?? 0;
        set => _saturationChange = value;
    }

    private double? _hueShift;
    [JsonProperty("_HueShift")]
    public double HueShift
    {
        get => Logic?._hueShift ?? _hueShift ?? 0;
        set => _hueShift = value;
    }

    private bool? _smoothingEnabled;
    [JsonProperty("_SmoothingEnabled")]
    [LogicOverridable("Enable Smoothing")]
    public bool SmoothingEnabled
    {
        get => Logic?._smoothingEnabled ?? _smoothingEnabled ?? true;
        set => _smoothingEnabled = value;
    }

    private Dictionary<DeviceKeys, DeviceKeys> _keyCloneMap = new();
    [JsonProperty("_KeyCloneMap")]
    public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap
    {
        get => Logic?._keyCloneMap ?? _keyCloneMap;
        set => _keyCloneMap = value;
    }

    public override void Default()
    {
        base.Default();

        _colorPostProcessEnabled = false;
        _brightnessChange = 0;
        _saturationChange = 0;
        _hueShift = 0;
        _smoothingEnabled = true;
        _keyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(Name = "Razer Chroma", IsDefault = true)]
public class RazerLayerHandler() : LayerHandler<RazerLayerHandlerProperties>("Chroma Layer")
{
    protected override UserControl CreateControl()
    {
        return new Control_RazerLayer(this);
    }

    private static readonly DeviceKeys[] DeviceKeysArray = Enum.GetValues<DeviceKeys>();

    // The Chroma service writes the emulator buffers at ~10 Hz with occasional multi-frame
    // gaps, so raw values step visibly. Each key eases toward the newest service colour per
    // render tick, turning those steps into fades.
    private const double SmoothingTau = 0.10;

    private readonly Dictionary<DeviceKeys, Vector4> _smoothed = new();
    private long _lastRenderTimestamp;

    public override EffectLayer Render(IGameState gameState)
    {
        if (!RzHelper.IsCurrentAppValid())
        {
            _smoothed.Clear();
            _lastRenderTimestamp = 0;
            return EmptyLayer.Instance;
        }

        var now = Stopwatch.GetTimestamp();
        var dt = _lastRenderTimestamp == 0 ? SmoothingTau : (now - _lastRenderTimestamp) / (double)Stopwatch.Frequency;
        _lastRenderTimestamp = now;
        var alpha = Properties.SmoothingEnabled ? (float)(1 - Math.Exp(-dt / SmoothingTau)) : 1f;

        foreach (var key in DeviceKeysArray)
        {
            if (!TryGetColor(key, out var color))
                continue;

            color = Smooth(key, color, alpha);
            EffectLayer.Set(key, in color);
        }

        foreach (var target in Properties.KeyCloneMap)
        {
            if (!_smoothed.TryGetValue(target.Value, out var source))
                continue;
            var color = ToColor(source);
            EffectLayer.Set(target.Key, in color);
        }

        return EffectLayer;
    }

    private Color Smooth(DeviceKeys key, Color target, float alpha)
    {
        var targetVector = new Vector4(target.A, target.R, target.G, target.B);
        if (alpha >= 1f || !_smoothed.TryGetValue(key, out var current))
        {
            _smoothed[key] = targetVector;
            return target;
        }

        current += (targetVector - current) * alpha;
        _smoothed[key] = current;
        return ToColor(current);
    }

    private static Color ToColor(Vector4 argb) => Color.FromArgb(
        (int)Math.Round(argb.X), (int)Math.Round(argb.Y), (int)Math.Round(argb.Z), (int)Math.Round(argb.W));

    private bool TryGetColor(DeviceKeys key, out Color color)
    {
        ChromaColor rColor;
        if (RazerLayoutMap.GenericKeyboard.TryGetValue(key, out var position))
            rColor = RzHelper.KeyboardColors[position[1] + position[0] * 22];
        else if (RazerLayoutMap.Mousepad.TryGetValue(key, out position))
            rColor = RzHelper.MousepadColors[position[0]];
        else if (RazerLayoutMap.Mouse.TryGetValue(key, out position))
            rColor = RzHelper.MouseColors[position[1] + position[0] * 7];
        else if (RazerLayoutMap.Headset.TryGetValue(key, out position))
            rColor = RzHelper.HeadsetColors[position[1]];
        else if (RazerLayoutMap.ChromaLink.TryGetValue(key, out position))
            rColor = RzHelper.ChromaLinkColors[position[0]];
        else
        {
            color = Color.Transparent;
            return false;
        }

        color = Properties.ColorPostProcessEnabled ? PostProcessColor(rColor) : FastTransform(rColor);

        return true;
    }

    private Color PostProcessColor(ChromaColor rzColor)
    {
        if (rzColor is { R: 0, G: 0, B: 0 })
            return Color.Black;

        var color = FastTransform(rzColor);
        
        if (Properties.BrightnessChange >= 0.001)
            color = CommonColorUtils.ChangeBrightness(color, Properties.BrightnessChange);
        if (Properties.SaturationChange >= 0.001)
            color = CommonColorUtils.ChangeSaturation(color, Properties.SaturationChange);
        if (Properties.HueShift >= 0.001)
            color = CommonColorUtils.ChangeHue(color, Properties.HueShift);

        return color;
    }

    private Color FastTransform(ChromaColor color)
    {
        return Properties.TransparencyEnabled ?
            CommonColorUtils.FastColorTransparent(color.R, color.G, color.B) :
            CommonColorUtils.FastColor(color.R, color.G, color.B);
    }
}
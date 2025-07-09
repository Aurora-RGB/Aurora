using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;
using DrawingColor = System.Drawing.Color;

namespace AuroraRgb.Settings.Layers;

public partial class WrapperLightsLayerHandlerProperties : LayerHandlerProperties
{
    // Color Enhancing
    [JsonIgnore]
    private bool? _colorEnhanceEnabled;
    [JsonProperty("_ColorEnhanceEnabled")]
    public bool ColorEnhanceEnabled
    {
        get => Logic?._colorEnhanceEnabled ?? _colorEnhanceEnabled ?? false;
        set => _colorEnhanceEnabled = value;
    }

    [JsonIgnore]
    private int? _colorEnhanceMode;
    [JsonProperty("_ColorEnhanceMode")]
    public int ColorEnhanceMode
    {
        get => Logic?._colorEnhanceMode ?? _colorEnhanceMode ?? 0;
        set => _colorEnhanceMode = value;
    }

    [JsonIgnore]
    private int? _colorEnhanceColorFactor;
    [JsonProperty("_ColorEnhanceColorFactor")]
    public int ColorEnhanceColorFactor
    {
        get => Logic?._colorEnhanceColorFactor ?? _colorEnhanceColorFactor ?? 0;
        set => _colorEnhanceColorFactor = value;
    }

    [JsonIgnore]
    private float? _colorEnhanceColorHsvSine;
    [JsonProperty("_ColorEnhanceColorHSVSine")]
    public float ColorEnhanceColorHsvSine
    {
        get => Logic?._colorEnhanceColorHsvSine ?? _colorEnhanceColorHsvSine ?? 0.0f;
        set => _colorEnhanceColorHsvSine = value;
    }

    [JsonIgnore]
    private float? _colorEnhanceColorHsvGamma;
    [JsonProperty("_ColorEnhanceColorHSVGamma")]
    public float ColorEnhanceColorHsvGamma
    {
        get => Logic?._colorEnhanceColorHsvGamma ?? _colorEnhanceColorHsvGamma ?? 0.0f;
        set => _colorEnhanceColorHsvGamma = value;
    }

    // Key cloning
    [JsonIgnore]
    private Dictionary<DeviceKeys, KeySequence>? _cloningMap;
    [JsonProperty("_CloningMap")]
    public Dictionary<DeviceKeys, KeySequence> CloningMap => Logic?._cloningMap ?? _cloningMap ?? new Dictionary<DeviceKeys, KeySequence>();

    public override void Default()
    {
        base.Default();
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();

        _colorEnhanceEnabled = true;
        _colorEnhanceMode = 0;
        _colorEnhanceColorFactor = 90;
        _colorEnhanceColorHsvSine = 0.1f;
        _colorEnhanceColorHsvGamma = 2.5f;
        _cloningMap = new Dictionary<DeviceKeys, KeySequence>();
    }
}
    
[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(IsDefault = true)]
public class WrapperLightsLayerHandler() : LayerHandler<WrapperLightsLayerHandlerProperties>("Aurora Wrapper")
{

    protected override UserControl CreateControl()
    {
        return new Control_WrapperLightsLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        if (!LfxState.IsWrapperConnected)
        {
            return EmptyLayer.Instance;
        }

        EffectLayer.Fill(GetBoostedColor(LfxState.LastFillColor));

        var allKeys = Enum.GetValues<DeviceKeys>();
        foreach (var key in allKeys)
        {
            // This checks if a key is already being cloned over and thus should be prevented from being re-set by the
            // normal wrapper. Fixes issues with some clones not working. Thanks to @Gurjot95 for finding it :)
            if (Properties.CloningMap.Values.Any(sequence => sequence.Keys.Contains(key)))
                continue;

            if (!LfxState.ExtraKeys.TryGetValue(key, out var extraKey)) continue;
            EffectLayer.Set(key, GetBoostedColor(extraKey));

            // Do the key cloning
            if (Properties.CloningMap.TryGetValue(key, out var targetKey))
                EffectLayer.Set(targetKey, GetBoostedColor(LfxState.ExtraKeys[key]));
        }

        var currentTime = Time.GetMillisecondsSinceEpoch();
        LfxState.CurrentEffect?.SetEffect(EffectLayer, currentTime - LfxState.CurrentEffect.TimeStarted);

        return EffectLayer;
    }

    private DrawingColor GetBoostedColor(in DrawingColor color)
    {
        if (!Properties.ColorEnhanceEnabled)
            return color;

        switch (Properties.ColorEnhanceMode)
        {
            case 0:
                var boostAmount = 0.0f;
                boostAmount += 1.0f - color.R / Properties.ColorEnhanceColorFactor;
                boostAmount += 1.0f - color.G / Properties.ColorEnhanceColorFactor;
                boostAmount += 1.0f - color.B / Properties.ColorEnhanceColorFactor;

                boostAmount = boostAmount <= 1.0f ? 1.0f : boostAmount;

                return ColorUtils.MultiplyColorByScalar(color, boostAmount);

            case 1:
                CommonColorUtils.ToHsv(color, out var hue, out var saturation, out var value);
                var x = Properties.ColorEnhanceColorHsvSine;
                var y = 1.0f / Properties.ColorEnhanceColorHsvGamma;
                value = (float)Math.Min(1, Math.Pow(x * Math.Sin(2 * Math.PI * value) + value, y));
                return CommonColorUtils.FromHsv(hue, saturation, value);

            default:
                return color;
        }
    }
}

internal class LFX_Color : EntireEffect
{
    public LFX_Color(DrawingColor color) : base(color, 0, 0)
    {
    }
}

internal class LFX_Pulse : EntireEffect
{
    private readonly DrawingColor _secondary;
    private DrawingColor _currentColor;

    public LFX_Pulse(DrawingColor primary, DrawingColor secondary, int duration) : base(primary, duration, 0)
    {
        _secondary = secondary;
    }

    public override  ref readonly DrawingColor GetCurrentColor(long time)
    {
        _currentColor = ColorUtils.MultiplyColorByScalar(Color, Math.Pow(Math.Sin(time / 1000.0D * Math.PI), 2.0));
        return ref _currentColor;
    }
}

internal class LFX_Morph : EntireEffect
{
    private readonly DrawingColor _secondary;
    private DrawingColor _currentColor;

    public LFX_Morph(DrawingColor primary, DrawingColor secondary, int duration) : base(primary, duration, 0)
    {
        _secondary = secondary;
    }

    public override ref readonly DrawingColor GetCurrentColor(long time)
    {
        _currentColor = time - TimeStarted >= Duration ? _secondary : ColorUtils.BlendColors(Color, _secondary, (time - TimeStarted) % Duration);
        return ref _currentColor;
    }
}
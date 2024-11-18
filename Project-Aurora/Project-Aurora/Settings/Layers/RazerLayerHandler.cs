using System;
using System.Collections.Generic;
using System.Drawing;
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

    public override EffectLayer Render(IGameState gameState)
    {
        if (!RzHelper.IsCurrentAppValid())
        {
            return EmptyLayer.Instance;
        }
        if (RzHelper.IsStale(this))
            return EffectLayer;

        foreach (var key in DeviceKeysArray)
        {
            if (!TryGetColor(key, out var color))
                continue;
                
            EffectLayer.Set(key, in color);
        }

        foreach (var target in Properties.KeyCloneMap)
            if(TryGetColor(target.Value, out var clr))
                EffectLayer.Set(target.Key, in clr);

        return EffectLayer;
    }

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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Logitech;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class LogitechLayerHandlerProperties : LayerHandlerProperties
{
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

    private Dictionary<DeviceKeys, DeviceKeys>? _keyCloneMap;
    [JsonProperty("_KeyCloneMap")]
    public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap
    {
        get => Logic?._keyCloneMap ?? (_keyCloneMap ??= new Dictionary<DeviceKeys, DeviceKeys>());
        set => _keyCloneMap = value;
    }

    public override void Default()
    {
        base.Default();

        _keyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(Name = "Logitech Lightsync", IsDefault = true)]
public sealed class LogitechLayerHandler : LayerHandler<LogitechLayerHandlerProperties>
{
    public LogitechLayerHandler() : base("Logitech Layer")
    {
        LogitechSdkModule.LogitechSdkListener.ColorsUpdated += LogitechSdkListenerOnColorsUpdated;
    }

    protected override UserControl CreateControl()
    {
        return new Control_LightsyncLayer(this);
    }

    private void LogitechSdkListenerOnColorsUpdated(object? sender, EventArgs e)
    {
        Invalidated = true;
    }

    public override EffectLayer Render(IGameState gameState)
    {
        var logitechSdk = LogitechSdkModule.LogitechSdkListener;
        if (logitechSdk.State != LightsyncSdkState.Connected)
        {
            return EmptyLayer.Instance;
        }

        if (!Invalidated)
        {
            return EffectLayer;
        }

        EffectLayer.Fill((Color)logitechSdk.BackgroundColor);
        foreach (var (key, sdkColor) in logitechSdk.Colors)
        {
            var color = Properties.ColorPostProcessEnabled ? PostProcessColor(sdkColor) : sdkColor;
            EffectLayer.Set(key, in color);
        }

        foreach (var (target, source) in Properties.KeyCloneMap)
            if(TryGetColor(source, out var clr))
            {
                var color = Properties.ColorPostProcessEnabled ? PostProcessColor(clr) : clr;
                EffectLayer.Set(target, in color);
            }

        Invalidated = false;
        return EffectLayer;
    }

    private Color PostProcessColor(Color color)
    {
        if (Properties.BrightnessChange != 0)
            color = CommonColorUtils.ChangeBrightness(color, Properties.BrightnessChange);
        if (Properties.SaturationChange != 0)
            color = CommonColorUtils.ChangeSaturation(color, Properties.SaturationChange);
        if (Properties.HueShift != 0)
            color = CommonColorUtils.ChangeHue(color, Properties.HueShift);

        return color;
    }

    private static bool TryGetColor(DeviceKeys key, out Color color)
    {
        return LogitechSdkModule.LogitechSdkListener.Colors.TryGetValue(key, out color);
    }

    public override void Dispose()
    {
        base.Dispose();

        LogitechSdkModule.LogitechSdkListener.ColorsUpdated -= LogitechSdkListenerOnColorsUpdated;
    }
}
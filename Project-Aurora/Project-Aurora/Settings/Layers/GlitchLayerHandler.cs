using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class GlitchLayerHandlerProperties : LayerHandlerProperties2Color
{
    private double? _updateInterval;

    [JsonProperty("_UpdateInterval")]
    public double UpdateInterval
    {
        get => Logic?._updateInterval ?? _updateInterval ?? 1.0;
        set
        {
            _updateInterval = value;
            OnPropertiesChanged(this);
        }
    }

    private bool? _allowTransparency;

    [JsonProperty("_AllowTransparency")]
    public bool AllowTransparency
    {
        get => Logic?._allowTransparency ?? _allowTransparency ?? false;
        set
        {
            _allowTransparency = value;
            OnPropertiesChanged(this);
        }
    }

    public override void Default()
    {
        base.Default();
        _updateInterval = 1.0;
        _allowTransparency = false;
        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
    }
}

public class GlitchLayerHandler<TProperty>() : LayerHandler<TProperty>("Glitch Layer")
    where TProperty : GlitchLayerHandlerProperties
{
    private readonly Random _randomizer = new();

    private readonly Dictionary<DeviceKeys, Color> _glitchColors = new();

    private long _previousTime;

    public override EffectLayer Render(IGameState state)
    {
        var currentTime = Time.GetMillisecondsSinceEpoch();
        if (_previousTime + Properties.UpdateInterval * 1000L > currentTime) return EffectLayer;
        _previousTime = currentTime;

        var keys = Properties.Sequence.Type == KeySequenceType.FreeForm ? Enum.GetValues(typeof(DeviceKeys)) : Properties.Sequence.Keys.ToArray();
        foreach (DeviceKeys key in keys)
        {
            Color clr;
            if (Properties.AllowTransparency)
                clr = _randomizer.Next() % 2 == 0 ? Color.Transparent : CommonColorUtils.GenerateRandomColor();
            else
                clr = CommonColorUtils.GenerateRandomColor();

            _glitchColors[key] = clr;
        }

        foreach (var kvp in _glitchColors)
        {
            EffectLayer.Set(kvp.Key, kvp.Value);
        }
        EffectLayer.OnlyInclude(Properties.Sequence);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _glitchColors.Clear();
        EffectLayer.Invalidate();
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("SecondaryColor")]
public class GlitchLayerHandler : GlitchLayerHandler<GlitchLayerHandlerProperties>
{
    protected override UserControl CreateControl()
    {
        return new Control_GlitchLayer(this);
    }
}
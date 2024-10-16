﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using Common;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public sealed partial class ToggleKeyLayerHandlerProperties : LayerHandlerProperties2Color {

    private Keybind[]? _triggerKeys;
    [JsonProperty("_TriggerKeys")]
    public Keybind[] TriggerKeys
    {
        get => Logic?._triggerKeys ?? _triggerKeys ?? [];
        set => _triggerKeys = value;
    }

    public override void Default() {
        base.Default();
        _triggerKeys = [];
    }
}

public sealed class ToggleKeyLayerHandler : LayerHandler<ToggleKeyLayerHandlerProperties>
{
    private bool _state = true;
    private readonly SingleColorBrush _primaryBrush;
    private readonly SingleColorBrush _secondaryBrush;

    public ToggleKeyLayerHandler(): base("ToggleKeyLayer")
    {
        _primaryBrush = new SingleColorBrush((SimpleColor)Properties.PrimaryColor);
        _secondaryBrush = new SingleColorBrush((SimpleColor)Properties.SecondaryColor);
    }

    protected override async Task Initialize()
    {
        await base.Initialize();
        
        (await InputsModule.InputEvents).KeyDown += InputEvents_KeyDown;
    }

    public override void Dispose()
    {
        InputsModule.InputEvents.Result.KeyDown -= InputEvents_KeyDown;
        _primaryBrush.Dispose();
        _secondaryBrush.Dispose();
        base.Dispose();
    }

    protected override UserControl CreateControl()
    {
        return new Control_ToggleKeyLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (Invalidated)
        {
            EffectLayer.Clear();
            Invalidated = false;
        }
        EffectLayer.Set(Properties.Sequence, _state ? _primaryBrush : _secondaryBrush);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
 
        _primaryBrush.Color = (SimpleColor)Properties.PrimaryColor;
        _secondaryBrush.Color = (SimpleColor)Properties.SecondaryColor;
    }

    private void InputEvents_KeyDown(object? sender, EventArgs e)
    {
        foreach (var kb in Properties.TriggerKeys)
            if (kb.IsPressed())
                _state = !_state;
    }
}
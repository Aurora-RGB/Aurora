﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using Aurora.Modules;
using Aurora.Profiles;
using Aurora.Settings.Layers.Controls;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers;

public class ToggleKeyLayerHandlerProperties : LayerHandlerProperties2Color<ToggleKeyLayerHandlerProperties> {

    public ToggleKeyLayerHandlerProperties()
    { }
    public ToggleKeyLayerHandlerProperties(bool assignDefault) : base(assignDefault) { }

    private Keybind[]? _triggerKeys;
    [JsonProperty("_TriggerKeys")]
    public Keybind[] TriggerKeys
    {
        get { return Logic._triggerKeys ?? _triggerKeys ?? new Keybind[] { }; }
        set => _triggerKeys = value;
    }

    public override void Default() {
        base.Default();
        _triggerKeys = new Keybind[] { };
    }
}

public class ToggleKeyLayerHandler : LayerHandler<ToggleKeyLayerHandlerProperties>
{
    private bool _state = true;
    private readonly SolidBrush _primaryBrush;
    private readonly SolidBrush _secondaryBrush;
    private bool _invalidated;

    public ToggleKeyLayerHandler(): base("ToggleKeyLayer")
    {
        _primaryBrush = new SolidBrush(Properties.PrimaryColor);
        _secondaryBrush = new SolidBrush(Properties.SecondaryColor);
    }

    protected override async Task Initialize()
    {
        await base.Initialize();
        
        (await InputsModule.InputEvents).KeyDown += InputEvents_KeyDown;
    }

    public override void Dispose()
    {
        base.Dispose();
        InputsModule.InputEvents.Result.KeyDown -= InputEvents_KeyDown;
    }

    protected override System.Windows.Controls.UserControl CreateControl()
    {
        return new Control_ToggleKeyLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        if (_invalidated)
        {
            EffectLayer.Clear();
            _invalidated = false;
        }
        EffectLayer.Set(Properties.Sequence, _state ? _primaryBrush : _secondaryBrush);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _primaryBrush.Color = Properties.PrimaryColor;
        _secondaryBrush.Color = Properties.SecondaryColor;
        _invalidated = true;
    }

    private void InputEvents_KeyDown(object? sender, EventArgs e)
    {
        foreach (var kb in Properties.TriggerKeys)
            if (kb.IsPressed())
                _state = !_state;
    }
}
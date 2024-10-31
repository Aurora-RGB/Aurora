using System;
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

public sealed class ToggleKeyLayerHandler() : LayerHandler<ToggleKeyLayerHandlerProperties>("ToggleKeyLayer")
{
    private bool _state = true;

    protected override async Task Initialize()
    {
        await base.Initialize();
        
        (await InputsModule.InputEvents).KeyDown += InputEvents_KeyDown;
    }

    public override void Dispose()
    {
        InputsModule.InputEvents.Result.KeyDown -= InputEvents_KeyDown;
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
        EffectLayer.Set(Properties.Sequence, _state ? Properties.PrimaryColor : Properties.SecondaryColor);
        return EffectLayer;
    }

    private void InputEvents_KeyDown(object? sender, EventArgs e)
    {
        foreach (var kb in Properties.TriggerKeys)
            if (kb.IsPressed())
                _state = !_state;
    }
}
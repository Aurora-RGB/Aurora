using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public sealed partial class ToolbarLayerHandlerProperties : LayerHandlerProperties2Color {

    private bool? _enableScroll;
    [JsonProperty("_EnableScroll")]
    public bool EnableScroll
    {
        get => Logic?._enableScroll ?? _enableScroll ?? false;
        set => _enableScroll = value;
    }

    private bool? _scrollLoop;
    [JsonProperty("_ScrollLoop")]
    public bool ScrollLoop
    {
        get => Logic?._scrollLoop ?? _scrollLoop ?? true;
        set => _scrollLoop = value;
    }

    public override void Default() {
        base.Default();
        _enableScroll = false;
        _scrollLoop = true;
    }
}

/// <summary>
/// ToolbarLayer as suggested by amahlaka97 on Discord and scroll improvement as suggested by DrMeteor.
/// When one of the keys it uses is pressed, that key becomes "active". When another key is pressed, that key becomes active instead.
/// Sort of like a radio button but for keys on the keyboard :)
/// The mouse scroll wheel can also be used to scroll up and down the active key (if EnableScroll is true).
/// </summary>
public sealed class ToolbarLayerHandler() : LayerHandler<ToolbarLayerHandlerProperties>("Toolbar Layer")
{
    private DeviceKeys _activeKey = DeviceKeys.NONE;

    protected override async Task Initialize()
    {
        await base.Initialize();
            
        // Listen for relevant events
        (await InputsModule.InputEvents).KeyDown += InputEvents_KeyDown;
        (await InputsModule.InputEvents).Scroll += InputEvents_Scroll;
    }

    public override void Dispose() {
        // Remove listeners on dispose
        InputsModule.InputEvents.Result.KeyDown -= InputEvents_KeyDown;
        InputsModule.InputEvents.Result.Scroll -= InputEvents_Scroll;
        base.Dispose();
    }

    protected override UserControl CreateControl() {
            
        return new Control_ToolbarLayer(this);
    }
        
    public override EffectLayer Render(IGameState _) {
        foreach (var key in Properties.Sequence.Keys)
            EffectLayer.Set(key, key == _activeKey ? Properties.SecondaryColor : Properties.PrimaryColor);
        return EffectLayer;
    }

    /// <summary>
    /// Handler for when any keyboard button is pressed.
    /// </summary>
    private void InputEvents_KeyDown(object? sender, KeyboardKeyEventArgs e) {
        if (Properties.Sequence.Keys.Contains(e.GetDeviceKey()))
            _activeKey = e.GetDeviceKey();
    }

    /// <summary>
    /// Handler for the ScrollWheel.
    /// </summary>
    private void InputEvents_Scroll(object? sender, MouseScrollEventArgs e)
    {
        if (!Properties.EnableScroll || Properties.Sequence.Keys.Count <= 1) return;
        // If there's no active key or the ks doesn't contain it (e.g. the sequence was just changed), make the first one active.
        if (_activeKey == DeviceKeys.NONE || !Properties.Sequence.Keys.Contains(_activeKey))
            _activeKey = Properties.Sequence.Keys[0];

        // If there's an active key make scroll move up/down
        else {
            // Target index is the current index +/- 1 depending on the scroll value
            var idx = Properties.Sequence.Keys.IndexOf(_activeKey) + (e.WheelDelta > 0 ? -1 : 1);

            // If scroll loop is enabled, allow the index to wrap around from start to end or end to start.
            if (Properties.ScrollLoop) {
                if (idx < 0) // If index is now negative (if first item selected and scrolling down), add the length to loop back
                    idx += Properties.Sequence.Keys.Count;
                idx %= Properties.Sequence.Keys.Count;

                // If scroll loop isn't enabled, cap the index so that it stops at either end
            } else {
                idx = Math.Max(Math.Min(idx, Properties.Sequence.Keys.Count - 1), 0);
            }

            _activeKey = Properties.Sequence.Keys[idx];
        }
    }
}
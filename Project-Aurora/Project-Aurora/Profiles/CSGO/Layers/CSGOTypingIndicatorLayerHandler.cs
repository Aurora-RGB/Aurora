using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public partial class CSGOTypingIndicatorLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _typingKeysColor;

    [JsonProperty("_TypingKeysColor")]
    public Color TypingKeysColor
    {
        get => Logic?._TypingKeysColor ?? _typingKeysColor ?? Color.Empty;
        set => _typingKeysColor = value;
    }

    public override void Default()
    {
        base.Default();

        _Sequence = new KeySequence(new[] { DeviceKeys.TILDE, DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS, DeviceKeys.BACKSPACE,
            DeviceKeys.TAB, DeviceKeys.Q, DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I, DeviceKeys.O, DeviceKeys.P, DeviceKeys.CLOSE_BRACKET, DeviceKeys.OPEN_BRACKET, DeviceKeys.BACKSLASH,
            DeviceKeys.CAPS_LOCK, DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.F, DeviceKeys.G, DeviceKeys.H, DeviceKeys.J, DeviceKeys.K, DeviceKeys.L, DeviceKeys.SEMICOLON, DeviceKeys.APOSTROPHE, DeviceKeys.HASHTAG, DeviceKeys.ENTER,
            DeviceKeys.LEFT_SHIFT, DeviceKeys.BACKSLASH_UK, DeviceKeys.Z, DeviceKeys.X, DeviceKeys.C, DeviceKeys.V, DeviceKeys.B, DeviceKeys.N, DeviceKeys.M, DeviceKeys.COMMA, DeviceKeys.PERIOD, DeviceKeys.FORWARD_SLASH, DeviceKeys.RIGHT_SHIFT,
            DeviceKeys.LEFT_CONTROL, DeviceKeys.LEFT_WINDOWS, DeviceKeys.LEFT_ALT, DeviceKeys.SPACE, DeviceKeys.RIGHT_ALT, DeviceKeys.RIGHT_WINDOWS, DeviceKeys.APPLICATION_SELECT, DeviceKeys.RIGHT_CONTROL,
            DeviceKeys.ARROW_UP, DeviceKeys.ARROW_LEFT, DeviceKeys.ARROW_DOWN, DeviceKeys.ARROW_RIGHT, DeviceKeys.ESC
        });
        _typingKeysColor = Color.FromArgb(0, 255, 0);
    }
}

[Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
public class CSGOTypingIndicatorLayerHandler() : LayerHandler<CSGOTypingIndicatorLayerHandlerProperties>("CSGO - Typing Keys")
{
    protected override UserControl CreateControl()
    {
        return new Control_CSGOTypingIndicatorLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateCsgo csgostate) return EmptyLayer.Instance;
        if (csgostate.Player.Activity != PlayerActivity.TextInput) return EmptyLayer.Instance;

        //Update Typing Keys
        EffectLayer.Set(Properties.Sequence, Properties.TypingKeysColor);
        return EffectLayer;

    }
}
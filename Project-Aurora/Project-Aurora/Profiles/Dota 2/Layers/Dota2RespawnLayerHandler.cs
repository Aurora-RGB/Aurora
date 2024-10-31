using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Dota_2.GSI;
using AuroraRgb.Profiles.Dota_2.GSI.Nodes;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Dota_2.Layers;

public partial class Dota2RespawnLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _respawnColor;

    [JsonProperty("_RespawnColor")]
    public Color RespawnColor
    {
        get => Logic?._RespawnColor ?? _respawnColor ?? Color.Empty;
        set => _respawnColor = value;
    }

    private Color? _respawningColor;

    [JsonProperty("_RespawningColor")]
    public Color RespawningColor
    {
        get => Logic?._RespawningColor ?? _respawningColor ?? Color.Empty;
        set => _respawningColor = value;
    }

    private Color? _backgroundColor;

    [JsonProperty("_BackgroundColor")]
    public Color BackgroundColor
    {
        get => Logic?._BackgroundColor ?? _backgroundColor ?? Color.Empty;
        set => _backgroundColor = value;
    }

    public override void Default()
    {
        base.Default();

        _respawnColor = Color.FromArgb(255, 0, 0);
        _respawningColor = Color.FromArgb(255, 170, 0);
        _backgroundColor = Color.FromArgb(255, 255, 255);
        _Sequence = new KeySequence(
            new[] {
                DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12,
                DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
            }
        );
    }

}

public class Dota2RespawnLayerHandler() : LayerHandler<Dota2RespawnLayerHandlerProperties, BitmapEffectLayer>("Dota 2 - Respawn")
{
    private readonly SolidBrush _solidBrush = new(Color.Empty);

    protected override UserControl CreateControl()
    {
        return new Control_Dota2RespawnLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateDota2 dota2State) return EmptyLayer.Instance;

        if (dota2State.Player.Team is DotaPlayerTeam.Undefined or DotaPlayerTeam.None ||
            dota2State.Hero.IsAlive) return EmptyLayer.Instance;
        var percent = dota2State.Hero.SecondsToRespawn > 5 ? 0.0 : 1.0 - dota2State.Hero.SecondsToRespawn / 5.0;
        if (percent <= 0) return EmptyLayer.Instance;

        _solidBrush.Color = ColorUtils.BlendColors(Color.Transparent, Properties.BackgroundColor, percent);
        EffectLayer.Fill(_solidBrush);

        EffectLayer.PercentEffect(
            Properties.RespawningColor,
            Properties.RespawnColor,
            Properties.Sequence,
            percent,
            1.0,
            PercentEffectType.AllAtOnce);
                    
        return EffectLayer;

    }
}
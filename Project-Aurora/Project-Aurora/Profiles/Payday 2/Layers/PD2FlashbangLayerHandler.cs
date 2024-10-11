using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Payday_2.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Payday_2.Layers;

public partial class PD2FlashbangLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _flashbangColor;

    [JsonProperty("_FlashbangColor")]
    public Color FlashbangColor
    {
        get => Logic?._FlashbangColor ?? _flashbangColor ?? Color.Empty;
        set => _flashbangColor = value;
    }

    public override void Default()
    {
        base.Default();

        _flashbangColor = Color.FromArgb(255, 255, 255);
    }

}

public class PD2FlashbangLayerHandler : LayerHandler<PD2FlashbangLayerHandlerProperties>
{
    protected override UserControl CreateControl()
    {
        return new Control_PD2FlashbangLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameState_PD2 pd2State) return EffectLayer.EmptyLayer;
        //Update Flashed
        if (pd2State.Game.State != GameStates.Ingame || pd2State.Players.LocalPlayer.FlashAmount <= 0) return EffectLayer.EmptyLayer;
        var flashColor = ColorUtils.MultiplyColorByScalar(Properties.FlashbangColor, pd2State.Players.LocalPlayer.FlashAmount);

        EffectLayer.FillOver(flashColor);

        return EffectLayer.EmptyLayer;
    }
}
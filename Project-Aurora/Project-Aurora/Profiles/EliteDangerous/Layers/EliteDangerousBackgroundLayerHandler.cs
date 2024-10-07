using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.EliteDangerous.GSI;
using AuroraRgb.Profiles.EliteDangerous.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.EliteDangerous.Layers;

public partial class EliteDangerousBackgroundHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _combatModeColor;

    [JsonProperty("_CombatModeColor")]
    public Color CombatModeColor
    {
        get => Logic?._CombatModeColor ?? _combatModeColor ?? Color.Empty;
        set => _combatModeColor = value;
    }

    private Color? _discoveryModeColor;

    [JsonProperty("_DiscoveryModeColor")]
    public Color DiscoveryModeColor
    {
        get => Logic?._DiscoveryModeColor ?? _discoveryModeColor ?? Color.Empty;
        set => _discoveryModeColor = value;
    }

    public override void Default()
    {
        base.Default();
        _combatModeColor = Color.FromArgb(61, 19, 0);
        _discoveryModeColor = Color.FromArgb(0, 38, 61);
    }
}
public class EliteDangerousBackgroundLayerHandler() : LayerHandler<EliteDangerousBackgroundHandlerProperties>("Elite: Dangerous - Background")
{
    private readonly SolidBrush _bg = new(Color.Transparent);

    protected override UserControl CreateControl()
    {
        return new Control_EliteDangerousBackgroundLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        var gameState = state as GameState_EliteDangerous;

        _bg.Color = gameState.Status.IsFlagSet(Flag.HUD_DISCOVERY_MODE) ? Properties.DiscoveryModeColor : Properties.CombatModeColor;
        EffectLayer.FillOver(_bg);

        return EffectLayer;
    }
}
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public class CSGOWinningTeamLayerHandlerProperties : LayerHandlerProperties2Color<CSGOWinningTeamLayerHandlerProperties>
{
    public Color? _CTColor { get; set; }

    [JsonIgnore]
    public Color CTColor => Logic?._CTColor ?? _CTColor ?? Color.Empty;

    public Color? _TColor { get; set; }

    [JsonIgnore]
    public Color TColor => Logic?._TColor ?? _TColor ?? Color.Empty;

    public CSGOWinningTeamLayerHandlerProperties() : base() { }

    public CSGOWinningTeamLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

    public override void Default()
    {
        base.Default();

        _CTColor = Color.FromArgb(33, 155, 221);
        _TColor = Color.FromArgb(221, 99, 33);
    }

}

public class CSGOWinningTeamLayerHandler : LayerHandler<CSGOWinningTeamLayerHandlerProperties>
{
    private readonly SolidBrush _solidBrush = new(Color.Empty);

    public CSGOWinningTeamLayerHandler(): base("CSGO - Winning Team Effect")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_CSGOWinningTeamLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameStateCsgo csgostate) return EffectLayer.EmptyLayer;

        // Block animations after end of round
        if (csgostate.Map.Phase == MapPhase.Undefined || csgostate.Round.Phase != RoundPhase.Over)
        {
            return EffectLayer.EmptyLayer;
        }

        _solidBrush.Color = Color.White;

        // Triggers directly after a team wins a round
        if (csgostate.Round.WinTeam != RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam == RoundWinTeam.Undefined)
        {
            // Determine round or game winner
            if (csgostate.Map.Phase == MapPhase.GameOver)
            {
                // End of match
                var tScore = csgostate.Map.TeamT.Score;
                var ctScore = csgostate.Map.TeamCT.Score;

                if (tScore > ctScore)
                {
                    _solidBrush.Color = Properties.TColor;
                }
                else if (ctScore > tScore)
                {
                    _solidBrush.Color = Properties.CTColor;
                }
            }
            else
            {
                _solidBrush.Color = csgostate.Round.WinTeam switch
                {
                    // End of round
                    RoundWinTeam.T => Properties.TColor,
                    RoundWinTeam.CT => Properties.CTColor,
                    _ => _solidBrush.Color
                };
            }
        }

        EffectLayer.Fill(_solidBrush);

        return EffectLayer;
    }
}
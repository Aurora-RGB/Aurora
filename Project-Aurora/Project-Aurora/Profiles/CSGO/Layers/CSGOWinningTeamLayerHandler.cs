using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public partial class CSGOWinningTeamLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _ctColor;

    [JsonProperty("_CTColor")]
    public Color CtColor
    {
        get => Logic?.CtColor ?? _ctColor ?? Color.Empty;
        set => _ctColor  = value;
    }

    private Color? _tColor;

    [JsonProperty("_TColor")]
    public Color TColor => Logic?._TColor ?? _tColor ?? Color.Empty;

    public override void Default()
    {
        base.Default();

        _ctColor = Color.FromArgb(33, 155, 221);
        _tColor = Color.FromArgb(221, 99, 33);
    }

}

public class CSGOWinningTeamLayerHandler() : LayerHandler<CSGOWinningTeamLayerHandlerProperties>("CSGO - Winning Team Effect")
{
    protected override UserControl CreateControl()
    {
        return new Control_CSGOWinningTeamLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateCsgo csgostate) return EmptyLayer.Instance;

        // Block animations after end of round
        if (csgostate.Map.Phase == MapPhase.Undefined || csgostate.Round.Phase != RoundPhase.Over)
        {
            return EmptyLayer.Instance;
        }

        var color = Color.White;

        // Triggers directly after a team wins a round
        if (csgostate.Round.WinTeam != RoundWinTeam.Undefined && csgostate.Previously?.Round.WinTeam == RoundWinTeam.Undefined)
        {
            // Determine round or game winner
            if (csgostate.Map.Phase == MapPhase.GameOver)
            {
                // End of match
                var tScore = csgostate.Map.TeamT.Score;
                var ctScore = csgostate.Map.TeamCT.Score;

                if (tScore > ctScore)
                {
                    color = Properties.TColor;
                }
                else if (ctScore > tScore)
                {
                    color = Properties.CtColor;
                }
            }
            else
            {
                color = csgostate.Round.WinTeam switch
                {
                    // End of round
                    RoundWinTeam.T => Properties.TColor,
                    RoundWinTeam.CT => Properties.CtColor,
                    _ => color
                };
            }
        }

        EffectLayer.Fill(in color);

        return EffectLayer;
    }
}
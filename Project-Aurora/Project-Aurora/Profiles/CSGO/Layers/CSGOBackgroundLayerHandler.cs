using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public partial class CSGOBackgroundLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _defaultColor;

    [JsonProperty("_DefaultColor")]
    public Color DefaultColor
    {
        get => Logic?._DefaultColor ?? _defaultColor ?? Color.Empty;
        set => _defaultColor = value;
    }

    private Color? _ctColor;

    [JsonProperty("_CTColor")]
    public Color CtColor
    {
        get => Logic?.CtColor ?? _ctColor ?? Color.Empty;
        set => _ctColor = value;
    }

    private Color? _tColor;

    [JsonProperty("_TColor")]
    public Color TColor
    {
        get => Logic?._TColor ?? _tColor ?? Color.Empty;
        set => _tColor = value;
    }

    private bool? _dimEnabled;

    [JsonProperty("_DimEnabled")]
    public bool DimEnabled
    {
        get => Logic?._DimEnabled ?? _dimEnabled ?? false;
        set => _dimEnabled = value;
    }

    private double? _dimDelay;

    [JsonProperty("_DimDelay")]
    public double DimDelay
    {
        get => Logic?._DimDelay ?? _dimDelay ?? 0.0;
        set => _dimDelay = value;
    }

    private int? _dimAmount;

    [JsonProperty("_DimAmount")]
    public int DimAmount
    {
        get => Logic?._DimAmount ?? _dimAmount ?? 100;
        set => _dimAmount = value;
    }

    public override void Default()
    {
        base.Default();

        _defaultColor = Color.FromArgb(158, 205, 255);
        _ctColor = Color.FromArgb(33, 155, 221);
        _tColor = Color.FromArgb(221, 99, 33);
        _dimEnabled = true;
        _dimDelay = 15;
        _dimAmount = 20;
    }
}

public class CSGOBackgroundLayerHandler() : LayerHandler<CSGOBackgroundLayerHandlerProperties>("CSGO - Background")
{
    private bool _isDimming;
    private double _dimValue = 100.0;
    private long _dimBgAt = 15;
    
    private Color _currentColor = Color.Transparent;

    protected override UserControl CreateControl()
    {
        return new Control_CSGOBackgroundLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateCsgo csgostate) return EmptyLayer.Instance;

        var inGame = csgostate.Previously?.Player.State.Health is > -1 and < 100
                     || (csgostate.Round.WinTeam == RoundWinTeam.Undefined && csgostate.Previously?.Round.WinTeam != RoundWinTeam.Undefined);
        if (csgostate.Player.State.Health == 100 && inGame && csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
        {
            _isDimming = false;
            _dimBgAt = Time.GetMillisecondsSinceEpoch() +  (long)Properties.DimDelay * 1000;
            _dimValue = 100.0;
        }

        var bgColor = csgostate.Player.Team switch
        {
            PlayerTeam.T => Properties.TColor,
            PlayerTeam.CT => Properties.CtColor,
            _ => Properties.DefaultColor
        };

        if (csgostate.Player.Team is PlayerTeam.CT or PlayerTeam.T)
        {
            if (_dimBgAt <= Time.GetMillisecondsSinceEpoch() || csgostate.Player.State.Health == 0)
            {
                _isDimming = true;
                bgColor = ColorUtils.MultiplyColorByScalar(bgColor, GetDimmingValue() / 100);
            }
            else
            {
                _isDimming = false;
                _dimValue = 100.0;
            }
        }

        if (Invalidated)
        {
            _currentColor = Color.Empty;
             Invalidated = false;
        }
        if (_currentColor == bgColor) return EffectLayer;
        _currentColor = bgColor;
        EffectLayer.Fill(bgColor);

        return EffectLayer;
    }

    private double GetDimmingValue()
    {
        if (!_isDimming || !Properties.DimEnabled) return _dimValue = 100.0;
        _dimValue -= 2.0;
        return _dimValue = _dimValue < Math.Abs(Properties.DimAmount - 100) ? Math.Abs(Properties.DimAmount - 100) : _dimValue;

    }
}
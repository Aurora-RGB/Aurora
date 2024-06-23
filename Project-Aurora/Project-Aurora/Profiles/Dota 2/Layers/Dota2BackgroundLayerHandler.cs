using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Dota_2.GSI;
using AuroraRgb.Profiles.Dota_2.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Dota_2.Layers;

public class Dota2BackgroundLayerHandlerProperties : LayerHandlerProperties2Color<Dota2BackgroundLayerHandlerProperties>
{
    private Color? _defaultColor;

    [JsonProperty("_DefaultColor")]
    public Color DefaultColor
    {
        get => Logic?._defaultColor ?? _defaultColor ?? Color.Empty;
        set => _defaultColor = value;
    }

    private Color? _radiantColor;

    [JsonProperty("_RadiantColor")]
    public Color RadiantColor
    {
        get => Logic?._radiantColor ?? _radiantColor ?? Color.Empty;
        set => _radiantColor = value;
    }

    private Color? _direColor;

    [JsonProperty("_DireColor")]
    public Color DireColor
    {
        get => Logic?._direColor ?? _direColor ?? Color.Empty;
        set => _direColor = value;
    }

    private bool? _dimEnabled;

    [JsonProperty("_DimEnabled")]
    public bool DimEnabled
    {
        get => Logic?._dimEnabled ?? _dimEnabled ?? false;
        set => _dimEnabled = value;
    }

    private double? _dimDelay;

    [JsonProperty("_DimDelay")]
    public double DimDelay
    {
        get => Logic?._dimDelay ?? _dimDelay ?? 0.0;
        set => _dimDelay = value;
    }

    public Dota2BackgroundLayerHandlerProperties()
    { }

    public Dota2BackgroundLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();

        _defaultColor = Color.FromArgb(140, 190, 230);
        _radiantColor = Color.FromArgb(0, 140, 30);
        _direColor = Color.FromArgb(200, 0, 0);
        _dimEnabled = true;
        _dimDelay = 15;
    }

}

public class Dota2BackgroundLayerHandler() : LayerHandler<Dota2BackgroundLayerHandlerProperties>("Dota 2 - Background")
{
    private bool _isDimming;
    private double _dimValue = 1.0;
    private int _dimBgAt = 15;

    protected override UserControl CreateControl()
    {
        return new Control_Dota2BackgroundLayer(this);
    }

    private SolidBrush _currentColor = new(Color.Empty);
    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameStateDota2 dota2State) return EffectLayer.EmptyLayer;

        if (dota2State.Previously?.Hero.HealthPercent == 0 && dota2State.Hero.HealthPercent == 100 && !dota2State.Previously.Hero.IsAlive && dota2State.Hero.IsAlive)
        {
            _isDimming = false;
            _dimBgAt = dota2State.Map.GameTime + (int)Properties.DimDelay;
            _dimValue = 1.0;
        }

        var bgColor = dota2State.Player.Team switch
        {
            PlayerTeam.Dire => Properties.DireColor,
            PlayerTeam.Radiant => Properties.RadiantColor,
            _ => Properties.DefaultColor
        };

        if (dota2State.Player.Team is PlayerTeam.Dire or PlayerTeam.Radiant)
        {
            if (_dimBgAt <= dota2State.Map.GameTime || !dota2State.Hero.IsAlive)
            {
                _isDimming = true;
                bgColor = ColorUtils.MultiplyColorByScalar(bgColor, GetDimmingValue());
            }
            else
            {
                _isDimming = false;
                _dimValue = 1.0;
            }
        }

        if (!Invalidated && _currentColor.Color == bgColor) return EffectLayer;

        _currentColor = new SolidBrush(bgColor);
        EffectLayer.Clear();
        EffectLayer.Fill(_currentColor);
        Invalidated = false;

        return EffectLayer;
    }

    private double GetDimmingValue()
    {
        if (!_isDimming || !Properties.DimEnabled) return _dimValue = 1.0;
        _dimValue -= 0.02;
        return _dimValue = _dimValue < 0.0 ? 0.0 : _dimValue;
    }
}
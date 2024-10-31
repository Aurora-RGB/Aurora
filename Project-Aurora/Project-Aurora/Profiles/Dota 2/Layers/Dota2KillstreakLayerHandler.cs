using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Dota_2.GSI;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Dota_2.Layers;

public partial class Dota2KillstreakLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _noKillstreakColor;

    [JsonProperty("_NoKillstreakColor")]
    public Color NoKillstreakColor => Logic?._NoKillstreakColor ?? _noKillstreakColor ?? Color.Empty;

    private Color? _firstKillstreakColor;

    [JsonProperty("_FirstKillstreakColor")]
    public Color FirstKillstreakColor => Logic?._FirstKillstreakColor ?? _firstKillstreakColor ?? Color.Empty;

    private Color? _doubleKillstreakColor;

    [JsonProperty("_DoubleKillstreakColor")]
    public Color DoubleKillstreakColor
    {
        get => Logic?._DoubleKillstreakColor ?? _doubleKillstreakColor ?? Color.Empty;
        set => _decaKillstreakColor = value;
    }

    private Color? _tripleKillstreakColor;

    [JsonProperty("_TripleKillstreakColor")]
    public Color TripleKillstreakColor
    {
        get => Logic?._TripleKillstreakColor ?? _tripleKillstreakColor ?? Color.Empty;
        set => _tripleKillstreakColor = value;
    }

    private Color? _quadKillstreakColor;

    [JsonProperty("_QuadKillstreakColor")]
    public Color QuadKillstreakColor
    {
        get => Logic?._QuadKillstreakColor ?? _quadKillstreakColor ?? Color.Empty;
        set => _quadKillstreakColor = value;
    }

    private Color? _pentaKillstreakColor;

    [JsonProperty("_PentaKillstreakColor")]
    public Color PentaKillstreakColor
    {
        get => Logic?._PentaKillstreakColor ?? _pentaKillstreakColor ?? Color.Empty;
        set => _pentaKillstreakColor = value;
    }

    private Color? _hexaKillstreakColor;

    [JsonProperty("_HexaKillstreakColor")]
    public Color HexaKillstreakColor
    {
        get => Logic?._HexaKillstreakColor ?? _hexaKillstreakColor ?? Color.Empty;
        set => _hexaKillstreakColor = value;
    }

    private Color? _septaKillstreakColor;

    [JsonProperty("_SeptaKillstreakColor")]
    public Color SeptaKillstreakColor
    {
        get => Logic?._SeptaKillstreakColor ?? _septaKillstreakColor ?? Color.Empty;
        set => _septaKillstreakColor = value;
    }

    private Color? _octaKillstreakColor;

    [JsonProperty("_OctaKillstreakColor")]
    public Color OctaKillstreakColor
    {
        get => Logic?._OctaKillstreakColor ?? _octaKillstreakColor ?? Color.Empty;
        set => _octaKillstreakColor = value;
    }

    private Color? _nonaKillstreakColor;

    [JsonProperty("_NonaKillstreakColor")]
    public Color NonaKillstreakColor
    {
        get => Logic?._NonaKillstreakColor ?? _nonaKillstreakColor ?? Color.Empty;
        set => _noKillstreakColor = value;
    }

    private Color? _decaKillstreakColor;

    [JsonProperty("_DecaKillstreakColor")]
    public Color DecaKillstreakColor
    {
        get => Logic?._DecaKillstreakColor ?? _decaKillstreakColor ?? Color.Empty;
        set => _decaKillstreakColor = value;
    }

    public override void Default()
    {
        base.Default();

        _noKillstreakColor = Color.FromArgb(0, 0, 0, 0); //No Streak
        _firstKillstreakColor = Color.FromArgb(0, 0, 0, 0); //First kill
        _doubleKillstreakColor = Color.FromArgb(255, 255, 255); //Double Kill
        _tripleKillstreakColor = Color.FromArgb(0, 255, 0); //Killing Spree
        _quadKillstreakColor = Color.FromArgb(128, 0, 255); //Dominating
        _pentaKillstreakColor = Color.FromArgb(255, 100, 100); //Mega Kill
        _hexaKillstreakColor = Color.FromArgb(255, 80, 0); //Unstoppable
        _septaKillstreakColor = Color.FromArgb(130, 180, 130); //Wicked Sick
        _octaKillstreakColor = Color.FromArgb(255, 0, 255); //Monster Kill
        _nonaKillstreakColor = Color.FromArgb(255, 0, 0); //Godlike
        _decaKillstreakColor = Color.FromArgb(255, 80, 0); //Godlike+
    }

}

public class Dota2KillstreakLayerHandler() : LayerHandler<Dota2KillstreakLayerHandlerProperties>("Dota 2 - Killstreak")
{
    private const long KsDuration = 4000;

    private bool _isPlayingKillStreakAnimation;
    private double _ksBlendAmount;
    private long _ksEndTime;
    private int _currentKillCount;

    protected override UserControl CreateControl()
    {
        return new Control_Dota2KillstreakLayer(this);
    }

    private bool _empty = true;
    public override EffectLayer Render(IGameState gameState)
    {
        if (_isPlayingKillStreakAnimation && Time.GetMillisecondsSinceEpoch() >= _ksEndTime)
        {
            _isPlayingKillStreakAnimation = false;
        }

        if (gameState is not GameStateDota2 dota2State) return EmptyLayer.Instance;
        if(_currentKillCount < dota2State.Player.Kills)
        {    //player got a kill
            _isPlayingKillStreakAnimation = true;

            _ksEndTime = Time.GetMillisecondsSinceEpoch() + KsDuration;
        }
        _currentKillCount = dota2State.Player.Kills;
                
        var ksEffectValue = GetKsEffectValue();
        if (ksEffectValue <= 0 && !_empty)
        {
            return EmptyLayer.Instance;
        }

        if (dota2State.Player.KillStreak < 2) return EmptyLayer.Instance;
        var ksColor = GetKillStreakColor(dota2State.Player.KillStreak);

        if (!(ksEffectValue > 0)) return EmptyLayer.Instance;
        EffectLayer.Clear();
        var color = ColorUtils.BlendColors(Color.Transparent, ksColor, ksEffectValue);
        EffectLayer.Fill(in color);
        _empty = false;

        return EffectLayer;
    }

    private Color GetKillStreakColor(int killstreakCount)
    {
        return killstreakCount switch
        {
            0 => Properties.NoKillstreakColor,
            1 => Properties.FirstKillstreakColor,
            2 => Properties.DoubleKillstreakColor,
            3 => Properties.TripleKillstreakColor,
            4 => Properties.QuadKillstreakColor,
            5 => Properties.PentaKillstreakColor,
            6 => Properties.HexaKillstreakColor,
            7 => Properties.SeptaKillstreakColor,
            8 => Properties.OctaKillstreakColor,
            9 => Properties.NonaKillstreakColor,
            >= 10 => Properties.DecaKillstreakColor,
            _ => Color.Transparent
        };
    }

    private double GetKsEffectValue()
    {
        if (_isPlayingKillStreakAnimation)
        {
            _ksBlendAmount += 0.15;
            return _ksBlendAmount = (_ksBlendAmount > 1.0 ? 1.0 : _ksBlendAmount);
        }

        _ksBlendAmount -= 0.15;
        return _ksBlendAmount = (_ksBlendAmount < 0.0 ? 0.0 : _ksBlendAmount);
    }
}
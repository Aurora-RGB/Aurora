using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Witcher3.GSI;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;
using Witcher3Gsi;

namespace AuroraRgb.Profiles.Witcher3.Layers;

public partial class Witcher3BackgroundLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _defaultColor;

    [JsonProperty("_DefaultColor")]
    public Color DefaultColor
    {
        get => Logic?._DefaultColor ?? _defaultColor ?? Color.Empty;
        set => _defaultColor = value;
    }

    private Color? _quenColor;

    [JsonProperty("_QuenColor")]
    public Color QuenColor
    {
        get => Logic?._QuenColor ?? _quenColor ?? Color.Empty;
        set => _quenColor = value;
    }

    private Color? _igniColor;

    [JsonProperty("_IgniColor")]
    public Color IgniColor
    {
        get => Logic?._IgniColor ?? _igniColor ?? Color.Empty;
        set => _igniColor = value;
    }

    private Color? _aardColor;

    [JsonProperty("_AardColor")]
    public Color AardColor
    {
        get => Logic?._AardColor ?? _aardColor ?? Color.Empty;
        set => _aardColor = value;
    }

    private Color? _yrdenColor;

    [JsonProperty("_YrdenColor")]
    public Color YrdenColor
    {
        get => Logic?._YrdenColor ?? _yrdenColor ?? Color.Empty;
        set => _yrdenColor = value;
    }

    private Color? _axiiColor;

    [JsonProperty("_AxiiColor")]
    public Color AxiiColor
    {
        get => Logic?._AxiiColor ?? _axiiColor ?? Color.Empty;
        set => _axiiColor = value;
    }

    public override void Default()
    {
        base.Default();
        _defaultColor = Color.Gray;
        _quenColor = Color.Yellow;
        _igniColor = Color.Red;
        _aardColor = Color.Blue;
        _yrdenColor = Color.Purple;
        _axiiColor = Color.Green;
    }
}

public class Witcher3BackgroundLayerHandler() : LayerHandler<Witcher3BackgroundLayerHandlerProperties>("Witcher3 - Background")
{
    private readonly SolidBrush _currentColor = new(Color.White);

    protected override UserControl CreateControl()
    {
        return new Control_Witcher3BackgroundLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateWitcher3 witcher3State) return EmptyLayer.Instance;

        var bgColor = witcher3State.Player.ActiveSign switch
        {
            WitcherSign.Aard => Properties.AardColor,
            WitcherSign.Igni => Properties.IgniColor,
            WitcherSign.Quen => Properties.QuenColor,
            WitcherSign.Yrden => Properties.YrdenColor,
            WitcherSign.Axii => Properties.AxiiColor,
            WitcherSign.None => Properties.DefaultColor,
            _ => Properties.DefaultColor
        };

        if (_currentColor.Color == bgColor) return EffectLayer;
        _currentColor.Color = bgColor;
        EffectLayer.FillOver(bgColor);

        return EffectLayer;
    }
}
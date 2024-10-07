using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.LeagueOfLegends.GSI;
using AuroraRgb.Profiles.LeagueOfLegends.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.LeagueOfLegends.Layers;

public partial class LoLBackgroundLayerHandlerProperties : LayerHandlerProperties
{
    private Dictionary<Champion, Color>? _championColors;

    [JsonProperty("_ChampionColors")]
    public Dictionary<Champion, Color> ChampionColors
    {
        get => Logic?._championColors ?? _championColors ?? new Dictionary<Champion, Color>();
        set => _championColors = value;
    }

    public override void Default()
    {
        base.Default();

        ChampionColors = new Dictionary<Champion, Color>(DefaultChampionColors.Colors);
    }
}

[LayerHandlerMeta(Name = "League of Legends Background")]
public class LoLBackgroundLayerHandler() : LayerHandler<LoLBackgroundLayerHandlerProperties>("Lol Background Layer")
{
    private Champion _lastChampion = Champion.None;
    private Color _lastColor = Color.Transparent;
    private int _lastWidth;
    private int _lastHeight;

    public override EffectLayer Render(IGameState gamestate)
    {
        var currentChampion = (gamestate as GameState_LoL)?.Player.Champion ?? Champion.None;
        if (!Properties.ChampionColors.ContainsKey(currentChampion) && DefaultChampionColors.Colors.TryGetValue(currentChampion, out var defaultColor))
            Properties.ChampionColors.Add(currentChampion, defaultColor);

        var currentColor = Properties.ChampionColors.GetValueOrDefault(currentChampion, Color.Transparent);

        //if the player changes champion
        //or if the color is adjusted in the UI
        //or if the canvas size changes due to the layout being changed
        if (currentChampion != _lastChampion || currentColor != _lastColor || 
            _lastWidth != Effects.Canvas.Width || _lastHeight != Effects.Canvas.Height)
        {
            _lastChampion = currentChampion;
            _lastColor = currentColor;
            _lastHeight = Effects.Canvas.Height;
            _lastWidth = Effects.Canvas.Width;
            EffectLayer.FillOver(_lastColor);
            //then we fill the layer again
        }
        //otherwise, we can just return the same layer as it's mostly static
        return EffectLayer;
    }

    protected override UserControl CreateControl()
    {
        return new LoLBackgroundLayer(this);
    }
}
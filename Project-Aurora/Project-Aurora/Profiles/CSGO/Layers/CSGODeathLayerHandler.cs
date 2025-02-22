﻿using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public partial class CSGODeathLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _deathColor;

    [JsonProperty("_DeathColor")]
    public Color DeathColor
    {
        get => Logic?._DeathColor ?? _deathColor ?? Color.Empty;
        set => _deathColor = value;
    }

    private int? _fadeOutAfter;

    [JsonProperty("_FadeOutAfter")]
    public int FadeOutAfter
    {
        get => Logic?._FadeOutAfter ?? _fadeOutAfter ?? 5;
        set => _fadeOutAfter = value;
    }

    public override void Default()
    {
        base.Default();

        _deathColor = Color.Red;
        _fadeOutAfter = 3;
    }

}

public class CSGODeathLayerHandler() : LayerHandler<CSGODeathLayerHandlerProperties>("CSGO - Death Effect")
{
    private bool _isDead;
    private int _fadeAlpha = 255;
    private long _lastTimeMillis;

    protected override UserControl CreateControl()
    {
        return new Control_CSGODeathLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameStateCsgo gameState) return EmptyLayer.Instance;
        var deathColor = Properties.DeathColor;

        // Confirm if CS:GO Player is correct
        if (!gameState.Provider.SteamID.Equals(gameState.Player.SteamID)) return EmptyLayer.Instance;

        // Are they dead?
        if (!_isDead && gameState.Player.State.Health <= 0 && gameState.Previously?.Player.State.Health > 0)
        {
            _isDead = true;
            _lastTimeMillis = Time.GetMillisecondsSinceEpoch();
            _fadeAlpha = 255;
        }

        if (!_isDead)
        {
            return EmptyLayer.Instance;
        }

        var fadeAlpha = GetFadeAlpha();
        if (fadeAlpha <= 0)
        {
            _isDead = false;
            return EmptyLayer.Instance;
        }

        var color = CommonColorUtils.FastColor(deathColor.R, deathColor.G, deathColor.B, (byte)fadeAlpha);
        EffectLayer.Fill(in color);
        return EffectLayer;
    }

    private int GetFadeAlpha()
    {
        var t = Time.GetMillisecondsSinceEpoch() - _lastTimeMillis;
        _lastTimeMillis = Time.GetMillisecondsSinceEpoch();
        _fadeAlpha -= (int)(t / 10);
        _fadeAlpha = Math.Min(_fadeAlpha, 255);
        return _fadeAlpha;
    }
}
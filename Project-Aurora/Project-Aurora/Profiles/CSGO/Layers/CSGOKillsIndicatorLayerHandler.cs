﻿using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public partial class CSGOKillIndicatorLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _regularKillColor;

    [JsonProperty("_RegularKillColor")]
    public Color RegularKillColor
    {
        get => Logic?._RegularKillColor ?? _regularKillColor ?? Color.Empty;
        set => _regularKillColor = value;
    }

    private Color? _headshotKillColor;

    [JsonProperty("_HeadshotKillColor")]
    public Color HeadshotKillColor
    {
        get => Logic?._HeadshotKillColor ?? _headshotKillColor ?? Color.Empty;
        set => _headshotKillColor = value;
    }

    public override void Default()
    {
        base.Default();

        _Sequence = new KeySequence(new[] { DeviceKeys.G1, DeviceKeys.G2, DeviceKeys.G3, DeviceKeys.G4, DeviceKeys.G5 });
        _regularKillColor = Color.FromArgb(255, 204, 0);
        _headshotKillColor = Color.FromArgb(255, 0, 0);
    }

}

public class CSGOKillIndicatorLayerHandler() : LayerHandler<CSGOKillIndicatorLayerHandlerProperties>("CSGO - Kills Indicator")
{
    private enum RoundKillType
    {
        None,
        Regular,
        Headshot
    }

    private readonly List<RoundKillType> _roundKills =
    [
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None,
        RoundKillType.None
    ];
    private int _lastCountedKill;

    protected override UserControl CreateControl()
    {
        return new Control_CSGOKillIndicatorLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateCsgo csgostate) return EmptyLayer.Instance;

        if (!csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID)) return EffectLayer;
        if (csgostate.Round.Phase == RoundPhase.FreezeTime) return EffectLayer;

        if (_lastCountedKill != csgostate.Player.State.RoundKills)
        {
            CalculateKills(csgostate);
        }
        
        for (var pos = 0; pos < Properties.Sequence.Keys.Count; pos++)
        {
            if (pos < _roundKills.Count)
            {
                switch (_roundKills[pos])
                {
                    case RoundKillType.Regular:
                        EffectLayer.Set(Properties.Sequence.Keys[pos], Properties.RegularKillColor);
                        break;
                    case RoundKillType.Headshot:
                        EffectLayer.Set(Properties.Sequence.Keys[pos], Properties.HeadshotKillColor);
                        break;
                    case RoundKillType.None:
                        EffectLayer.Set(Properties.Sequence.Keys[pos], in Color.Empty);
                        break;
                }
            }
        }

        return EffectLayer;
    }

    private void CalculateKills(GameStateCsgo csgostate)
    {
        var roundClearPhase = csgostate.Round.WinTeam == RoundWinTeam.Undefined &&
                              csgostate.Previously?.Round.WinTeam != RoundWinTeam.Undefined;
        var respawned = csgostate.Player.State.Health == 100 &&
                        csgostate.Previously?.Player.State.Health is > -1 and < 100 &&
                        csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID);
            
        if (csgostate.Player.State.RoundKills == 0 || roundClearPhase || respawned)
        {
            for (var i = 0; i < _roundKills.Count; i++)
            {
                _roundKills[i] = RoundKillType.None;
            }
        }

        if (csgostate.Previously?.Player.State.RoundKills != -1 && csgostate.Player.State.RoundKills != -1 &&
            csgostate.Previously?.Player.State.RoundKills < csgostate.Player.State.RoundKills &&
            csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
        {
            var index = csgostate.Player.State.RoundKills - 1;
            if (index >= _roundKills.Count)
            {
                return;
            }
            if (csgostate.Previously.Player.State.RoundKillHS != -1 && csgostate.Player.State.RoundKillHS != -1 &&
                csgostate.Previously.Player.State.RoundKillHS < csgostate.Player.State.RoundKillHS)
                    
                _roundKills[index] = RoundKillType.Headshot;
            else
                _roundKills[index] = RoundKillType.Regular;
        }

        _lastCountedKill = csgostate.Player.State.RoundKills;
    }
}
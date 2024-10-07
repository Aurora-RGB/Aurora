using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Dota_2.GSI;
using AuroraRgb.Profiles.Dota_2.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Dota_2.Layers;

public partial class Dota2AbilityLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _canCastAbilityColor;
    [JsonProperty("_CanCastAbilityColor")]
    public Color CanCastAbilityColor
    {
        get => Logic?._canCastAbilityColor ?? _canCastAbilityColor ?? Color.Empty;
        set => _canCastAbilityColor = value;
    }

    private Color? _canNotCastAbilityColor;
    [JsonProperty("_CanNotCastAbilityColor")]
    public Color CanNotCastAbilityColor
    {
        get => Logic?._canNotCastAbilityColor ?? _canNotCastAbilityColor ?? Color.Empty;
        set => _canNotCastAbilityColor = value;
    }

    private List<DeviceKeys>? _abilityKeys;
    [JsonProperty("_AbilityKeys")]
    public List<DeviceKeys> AbilityKeys
    {
        get => Logic?._abilityKeys ?? _abilityKeys ?? [];
        set => _abilityKeys = value;
    }

    public Dota2AbilityLayerHandlerProperties()
    { }

    public Dota2AbilityLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();

        _canCastAbilityColor = Color.FromArgb(0, 255, 0);
        _canNotCastAbilityColor = Color.FromArgb(255, 0, 0);
        _abilityKeys = [ DeviceKeys.Q, DeviceKeys.W, DeviceKeys.E, DeviceKeys.D, DeviceKeys.F, DeviceKeys.R ];
    }
}

public class Dota2AbilityLayerHandler() : LayerHandler<Dota2AbilityLayerHandlerProperties>("Dota 2 - Abilities")
{
    protected override UserControl CreateControl()
    {
        return new Control_Dota2AbilityLayer(this);
    }

    private readonly HashSet<string> _ignoredAbilities = ["seasonal", "high_five"];

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameStateDota2 dota2State) return EffectLayer.EmptyLayer;
        if (dota2State.Map.GameState != DOTA_GameState.DOTA_GAMERULES_STATE_PRE_GAME &&
            dota2State.Map.GameState != DOTA_GameState.DOTA_GAMERULES_STATE_GAME_IN_PROGRESS)
            return EffectLayer.EmptyLayer;
        for (var index = 0; index < dota2State.Abilities.Count; index++)
        {
            var ability = dota2State.Abilities[index];
            if (_ignoredAbilities.Any(ignoredAbilityName => ability.Name.Contains(ignoredAbilityName)))
                continue;

            if (index >= Properties.AbilityKeys.Count) continue;
            var key = Properties.AbilityKeys[index];

            switch (ability)
            {
                case { CanCast: true, Cooldown: 0, Level: > 0 }:
                    EffectLayer.Set(key, Properties.CanCastAbilityColor);
                    break;
                case { Cooldown: <= 5, Level: > 0 }:
                    EffectLayer.Set(key, ColorUtils.BlendColors(Properties.CanCastAbilityColor, Properties.CanNotCastAbilityColor, ability.Cooldown / 5.0));
                    break;
                default:
                    EffectLayer.Set(key, Properties.CanNotCastAbilityColor);
                    break;
            }
        }
        return EffectLayer;
    }
}
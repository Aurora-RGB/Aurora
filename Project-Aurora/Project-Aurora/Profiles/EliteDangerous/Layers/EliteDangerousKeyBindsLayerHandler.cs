using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Profiles.EliteDangerous.GSI;
using AuroraRgb.Profiles.EliteDangerous.GSI.Nodes;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Devices;
using UserControl = System.Windows.Controls.UserControl;

namespace AuroraRgb.Profiles.EliteDangerous.Layers;

public class ColorGroup
{
    public const string None = "None";
    public const string OtherColor = "OtherColor";
    public const string HudModeCombatColor = "HudModeCombatColor";
    public const string HudModeDiscoveryColor = "HudModeDiscoveryColor";
    public const string UiColor = "UiColor";
    public const string UiAltColor = "UiAltColor";
    public const string ShipStuffColor = "ShipStuffColor";
    public const string CameraColor = "CameraColor";
    public const string DefenceColor = "DefenceColor";
    public const string OffenceColor = "OffenceColor";
    public const string MovementSpeedColor = "MovementSpeedColor";
    public const string MovementSecondaryColor = "MovementSecondaryColor";
    public const string WingColor = "WingColor";
    public const string NavigationColor = "NavigationColor";
    public const string ModeEnableColor = "ModeEnableColor";
    public const string ModeDisableColor = "ModeDisableColor";
}

public partial class EliteDangerousKeyBindsHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _otherColor;

    public Color OtherColor => Logic?._OtherColor ?? _otherColor ?? Color.Empty;

    private Color? _hudModeCombatColor;

    public Color HudModeCombatColor
    {
        get => Logic?._HudModeCombatColor ?? _hudModeCombatColor ?? Color.Empty;
        set => _hudModeCombatColor = value;
    }

    private Color? _hudModeDiscoveryColor;

    public Color HudModeDiscoveryColor
    {
        get => Logic?._HudModeDiscoveryColor ?? _hudModeDiscoveryColor ?? Color.Empty;
        set => _hudModeCombatColor = value;
    }

    private Color? _uiColor;

    public Color UiColor
    {
        get => Logic?._UiColor ?? _uiColor ?? Color.Empty;
        set => _uiColor = value;
    }

    private Color? _uiAltColor;

    public Color UiAltColor
    {
        get => Logic?._UiAltColor ?? _uiAltColor ?? Color.Empty;
        set => _uiAltColor = value;
    }

    private Color? _shipStuffColor;

    public Color ShipStuffColor
    {
        get => Logic?._ShipStuffColor ?? _shipStuffColor ?? Color.Empty;
        set => _shipStuffColor = value;
    }

    private Color? _cameraColor;

    public Color CameraColor
    {
        get => Logic?._CameraColor ?? _cameraColor ?? Color.Empty;
        set => _cameraColor = value;
    }

    private Color? _defenceColor;

    public Color DefenceColor
    {
        get => Logic?._DefenceColor ?? _defenceColor ?? Color.Empty;
        set => _defenceColor = value;
    }

    private Color? _offenceColor;

    public Color OffenceColor
    {
        get => Logic?._OffenceColor ?? _offenceColor ?? Color.Empty;
        set => _offenceColor = value;
    }

    private Color? _movementSpeedColor;

    public Color MovementSpeedColor
    {
        get => Logic?._MovementSpeedColor ?? _movementSpeedColor ?? Color.Empty;
        set => _movementSpeedColor = value;
    }

    private Color? _movementSecondaryColor;

    public Color MovementSecondaryColor
    {
        get => Logic?._MovementSecondaryColor ?? _movementSecondaryColor ?? Color.Empty;
        set => _movementSecondaryColor = value;
    }

    private Color? _wingColor;

    public Color WingColor
    {
        get => Logic?._WingColor ?? _wingColor ?? Color.Empty;
        set => _wingColor = value;
    }

    private Color? _navigationColor;

    public Color NavigationColor
    {
        get => Logic?._NavigationColor ?? _navigationColor ?? Color.Empty;
        set => _navigationColor = value;
    }

    private Color? _modeEnableColor;

    public Color ModeEnableColor
    {
        get => Logic?._ModeEnableColor ?? _modeEnableColor ?? Color.Empty;
        set => _modeEnableColor = value;
    }

    private Color? _modeDisableColor;

    public Color ModeDisableColor
    {
        get => Logic?._ModeDisableColor ?? _modeDisableColor ?? Color.Empty;
        set => _modeDisableColor = value;
    }
    
    public EliteDangerousKeyBindsHandlerProperties(){}

    public EliteDangerousKeyBindsHandlerProperties(Color? movementSpeedColor, Color? movementSecondaryColor)
    {
        _movementSpeedColor = movementSpeedColor;
        _movementSecondaryColor = movementSecondaryColor;
    }

    public override void Default()
    {
        base.Default();
        _otherColor = Color.FromArgb(255, 255, 255);
        _hudModeCombatColor = Color.FromArgb(255, 80, 0);
        _hudModeDiscoveryColor = Color.FromArgb(0, 160, 255);
        _uiColor = Color.FromArgb(255, 80, 0);
        _uiAltColor = Color.FromArgb(255, 115, 70);
        _shipStuffColor = Color.FromArgb(0, 255, 0);
        _cameraColor = Color.FromArgb(71, 164, 79);
        _defenceColor = Color.FromArgb(0, 220, 255);
        _offenceColor = Color.FromArgb(255, 0, 0);
        _movementSpeedColor = Color.FromArgb(136, 0, 255);
        _movementSecondaryColor = Color.FromArgb(255, 0, 255);
        _wingColor = Color.FromArgb(0, 0, 255);
        _navigationColor = Color.FromArgb(255, 220, 0);
        _modeEnableColor = Color.FromArgb(153, 167, 255);
        _modeDisableColor = Color.FromArgb(61, 88, 156);
    }

    public Color GetColorByGroupName(string colorVariableName)
    {
        switch (@colorVariableName)
        {
            case ColorGroup.HudModeCombatColor: return HudModeCombatColor;
            case ColorGroup.HudModeDiscoveryColor: return HudModeDiscoveryColor;
            case ColorGroup.UiColor: return UiColor;
            case ColorGroup.UiAltColor: return UiAltColor;
            case ColorGroup.ShipStuffColor: return ShipStuffColor;
            case ColorGroup.CameraColor: return CameraColor;
            case ColorGroup.DefenceColor: return DefenceColor;
            case ColorGroup.OffenceColor: return OffenceColor;
            case ColorGroup.MovementSpeedColor: return MovementSpeedColor;
            case ColorGroup.MovementSecondaryColor: return MovementSecondaryColor;
            case ColorGroup.WingColor: return WingColor;
            case ColorGroup.NavigationColor: return NavigationColor;
            case ColorGroup.ModeEnableColor: return ModeEnableColor;
            case ColorGroup.ModeDisableColor: return ModeDisableColor;
            case ColorGroup.None: return Color.FromArgb(0, 0, 0, 0);
        }

        return OtherColor;
    }
}

public class EliteDangerousKeyBindsLayerHandler : LayerHandler<EliteDangerousKeyBindsHandlerProperties>
{
    private class KeyBlendState
    {
        public Color colorFrom = Color.Empty;
        public Color colorTo = Color.Empty;
        public double transitionProgress = 0;

        public KeyBlendState(Color colorFrom, Color colorTo)
        {
            this.colorFrom = colorFrom;
            this.colorTo = colorTo;
        }

        public bool Finished()
        {
            return transitionProgress >= 1;
        }

        public bool Increment()
        {
            transitionProgress = Math.Min(1, transitionProgress + 0.08);

            return Finished();
        }

        public Color GetBlendedColor()
        {
            return ColorUtils.BlendColors(colorFrom, colorTo, transitionProgress);
        }

        public bool Equals(KeyBlendState blendState)
        {
            return colorFrom.Equals(blendState.colorFrom) && colorTo.Equals(blendState.colorTo);
        }
    }

    private ControlGroupSet[] controlGroupSets =
    {
        ControlGroupSets.CONTROLS_MAIN,
        ControlGroupSets.CONTROLS_SHIP,
        ControlGroupSets.CONTROLS_SRV,
        ControlGroupSets.CONTROLS_SYSTEM_MAP,
        ControlGroupSets.CONTROLS_GALAXY_MAP,
        ControlGroupSets.CONTROLS_FSS,
        ControlGroupSets.CONTROLS_ADS,
        ControlGroupSets.UI_PANELS,
        ControlGroupSets.UI_PANEL_TABS,
    };

    public EliteDangerousKeyBindsLayerHandler() : base("Elite: Dangerous - Key Binds")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_EliteDangerousKeyBindsLayer(this);
    }

    private Dictionary<DeviceKeys, Color> currentKeyColors = new Dictionary<DeviceKeys, Color>();
    private Dictionary<DeviceKeys, KeyBlendState> keyBlendStates = new Dictionary<DeviceKeys, KeyBlendState>();
    private void SetKey(EffectLayer layer, DeviceKeys key, Color color)
    {
        if (keyBlendStates.ContainsKey(key))
        {
            keyBlendStates.Remove(key);
        }
        currentKeyColors[key] = color;
        layer.Set(key, color);
    }
        
    private void SetKeySmooth(EffectLayer layer, DeviceKeys key, Color color)
    {
        if (!currentKeyColors.ContainsKey(key))
        {
            currentKeyColors[key] = Color.Empty;
        }

        KeyBlendState blendState = new KeyBlendState(currentKeyColors[key], color);
        if (keyBlendStates.ContainsKey(key))
        {
            if (!keyBlendStates[key].colorTo.Equals(color))
            {
                blendState.colorFrom = keyBlendStates[key].GetBlendedColor();
                keyBlendStates[key] = blendState;
            }
        }
        else
        {
            keyBlendStates[key] = blendState;
        }

        keyBlendStates[key].Increment();
        if (keyBlendStates[key].Finished())
        {
            SetKey(layer, key, color);
        }
        else
        {
            layer.Set(key, keyBlendStates[key].GetBlendedColor());
        }
    }

    public override EffectLayer Render(IGameState state)
    {
        GameState_EliteDangerous gameState = state as GameState_EliteDangerous;
        GSI.Nodes.Controls controls = (state as GameState_EliteDangerous).Controls;

        EffectLayer.Clear();
        HashSet<DeviceKeys> leftoverBlendStates = new HashSet<DeviceKeys>(keyBlendStates.Keys);

        long currentTime = Time.GetMillisecondsSinceEpoch();

        if (gameState.Journal.fsdWaitingCooldown && gameState.Status.IsFlagSet(Flag.FSD_COOLDOWN))
        {
            gameState.Journal.fsdWaitingCooldown = false;
        }

        if (gameState.Journal.fsdWaitingSupercruise && gameState.Status.IsFlagSet(Flag.SUPERCRUISE))
        {
            gameState.Journal.fsdWaitingSupercruise = false;
        }

        Color newKeyColor;
        Dictionary<DeviceKeys, Color> smoothColorSets = new Dictionary<DeviceKeys, Color>();
        foreach (ControlGroupSet controlGroupSet in controlGroupSets)
        {
            if (!controlGroupSet.IsSatisfied(gameState)) continue;

            foreach (ControlGroup controlGroup in controlGroupSet.controlGroups)
            {
                if (!controlGroup.IsSatisfied(gameState)) continue;

                foreach (string command in controlGroup.commands)
                {
                    if (!controls.commandToBind.ContainsKey(command)) continue;

                    bool keyWithEffect = KeyPresets.KEY_EFFECTS.ContainsKey(command);

                    foreach (Bind.Mapping mapping in controls.commandToBind[command].mappings)
                    {
                        bool allModifiersPressed = true;
                        foreach (DeviceKeys modifierKey in mapping.modifiers)
                        {
                            SetKey(EffectLayer, modifierKey, Properties.ShipStuffColor);
                            leftoverBlendStates.Remove(modifierKey);
                            if (InputsModule.InputEvents.Result.PressedKeys.Contains(KeyUtils.GetFormsKey(modifierKey)))
                                continue;
                            allModifiersPressed = false;
                            break;
                        }

                        if (!allModifiersPressed) continue;

                        newKeyColor = Properties.GetColorByGroupName(
                            controlGroup.colorGroupName ?? CommandColors.GetColorGroupForCommand(command)
                        );

                        if (keyWithEffect)
                        {
                            SetKey(EffectLayer, mapping.key, KeyPresets.KEY_EFFECTS[command](newKeyColor, gameState, currentTime));
                        }
                        else
                        {
                            smoothColorSets[mapping.key] = newKeyColor;
                        }

                        leftoverBlendStates.Remove(mapping.key);
                    }
                }
            }
        }

        //Apply smooth transitions for keys
        foreach (KeyValuePair<DeviceKeys, Color> smoothKey in smoothColorSets)
        {
            SetKeySmooth(EffectLayer, smoothKey.Key, smoothKey.Value);
        }            
            
        //Fade out inactive keys
        foreach (DeviceKeys key in leftoverBlendStates)
        {
            SetKeySmooth(EffectLayer, key, Color.Empty);
        }

        return EffectLayer;
    }
}
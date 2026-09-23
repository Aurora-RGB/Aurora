using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.RocketLeague.GSI;
using AuroraRgb.Profiles.RocketLeague.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using Common.Devices;

namespace AuroraRgb.Profiles.RocketLeague;

public class RocketLeagueBMProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers =
        [
            new Layer("Controller Throttle", new PercentGradientLayerHandler
            {
                Properties = new PercentGradientLayerHandlerProperties
                {
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence([
                        DeviceKeys.TILDE, DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                        DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE,
                        DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS, DeviceKeys.BACKSPACE
                    ]),
                    Gradient = new EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/Speed"),
                    MaxVariablePath = new VariablePath("100"),
                },
            }),

            new Layer("Boost Indicator (Peripheral)", new PercentGradientLayerHandler
            {
                Properties = new PercentGradientLayerHandlerProperties
                {
                    PercentType = PercentEffectType.AllAtOnce,
                    _Sequence = new KeySequence([DeviceKeys.Peripheral, DeviceKeys.Peripheral_Logo]),
                    Gradient = new EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/Boost"),
                    MaxVariablePath = new VariablePath("100")
                },
            }),

            new Layer("Boost Indicator", new PercentGradientLayerHandler
            {
                Properties = new PercentGradientLayerHandlerProperties
                {
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence([
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5,
                        DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10,
                        DeviceKeys.F11, DeviceKeys.F12
                    ]),
                    Gradient = new EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/Boost"),
                    MaxVariablePath = new VariablePath("100"),
                },
            }),

            new Layer("Boost Background", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.Black,
                    _Sequence = new KeySequence([
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5,
                        DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10,
                        DeviceKeys.F11, DeviceKeys.F12
                    ])
                }
            }),

            new Layer("Goal Explosion", new RocketLeagueGoalExplosionLayerHandler()),
            new Layer("Score Split", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence =
                        {
                            Freeform = new FreeFormObject(0, 0, 980, 230),
                            Type = KeySequenceType.FreeForm
                        },
                        VariablePath = new VariablePath("YourTeam/Goals"),
                        MaxVariablePath = new VariablePath("Game/TotalGoals"),
                        _PrimaryColor = Color.Transparent,
                        SecondaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicDouble(nameof(PercentLayerHandlerProperties._Value), new IfElseNumeric(new BooleanAnd([ //if match is tied 0 - 0
                                    new BooleanMathsComparison(new NumberGSINumeric("YourTeam/Goals"), new NumberConstant(0)),
                                    new BooleanMathsComparison(new NumberGSINumeric("OpponentTeam/Goals"), new NumberConstant(0))
                                ]
                            ),
                            new NumberConstant(1), //then set the value to 1, so it is split 50-50
                            new NumberGSINumeric("YourTeam/Goals") //otherwise set to our goals
                        )
                    )
                    .SetDynamicDouble(nameof(PercentLayerHandlerProperties._MaxValue), new IfElseNumeric(new BooleanAnd([ //if match is tied 0 - 0
                                    new BooleanMathsComparison(new NumberGSINumeric("YourTeam/Goals"), new NumberConstant(0)),
                                    new BooleanMathsComparison(new NumberGSINumeric("OpponentTeam/Goals"), new NumberConstant(0))
                                ]
                            ),
                            new NumberConstant(2), //then set the max to 2, so it is split 50-50
                            new NumberGSINumeric("Game/TotalGoals") //otherwise set to total goals
                        )
                    )
                    .SetDynamicColor(nameof(PercentLayerHandlerProperties._PrimaryColor),
                        new NumberGSINumeric("YourTeam/PrimaryAlpha"),
                        new NumberGSINumeric("YourTeam/PrimaryRed"),
                        new NumberGSINumeric("YourTeam/PrimaryGreen"),
                        new NumberGSINumeric("YourTeam/PrimaryBlue"))
                    .SetDynamicColor(nameof(PercentLayerHandlerProperties.SecondaryColor),
                        new NumberGSINumeric("OpponentTeam/SecondaryAlpha"),
                        new NumberGSINumeric("OpponentTeam/PrimaryRed"),
                        new NumberGSINumeric("OpponentTeam/PrimaryGreen"),
                        new NumberGSINumeric("OpponentTeam/PrimaryBlue"))
                    .SetDynamicBoolean(nameof(LayerHandlerProperties._Enabled), new BooleanGSIEnum("GameStatus", RlStatus.InGame))
            ),

            new Layer("Background Layer", new BreathingLayerHandler
                {
                    Properties = new BreathingLayerHandlerProperties
                    {
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        _PrimaryColor = Color.Blue,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicColor(nameof(BreathingLayerHandlerProperties._PrimaryColor),
                        new NumberGSINumeric("HighlightedTeam/PrimaryAlpha"),
                        new NumberGSINumeric("HighlightedTeam/PrimaryRed"),
                        new NumberGSINumeric("HighlightedTeam/PrimaryGreen"),
                        new NumberGSINumeric("HighlightedTeam/PrimaryBlue"))
                    .SetDynamicColor(nameof(BreathingLayerHandlerProperties.SecondaryColor),
                        new NumberGSINumeric("HighlightedTeam/SecondaryAlpha"),
                        new NumberGSINumeric("HighlightedTeam/SecondaryRed"),
                        new NumberGSINumeric("HighlightedTeam/SecondaryGreen"),
                        new NumberGSINumeric("HighlightedTeam/SecondaryBlue"))
            )
        ];
    }
}
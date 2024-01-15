﻿using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using Aurora.Profiles.RocketLeague.Layers;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Boolean;
using Common.Devices;

namespace Aurora.Profiles.RocketLeague
{
    public class RocketLeagueBMProfile : ApplicationProfile
    {
        public RocketLeagueBMProfile() : base()
        {

        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Boost Indicator (Peripheral)", new PercentGradientLayerHandler()
                {
                    Properties = new PercentGradientLayerHandlerProperties()
                    {
                        PercentType = PercentEffectType.AllAtOnce,
                        _Sequence = new KeySequence(new DeviceKeys[] { DeviceKeys.Peripheral, DeviceKeys.Peripheral_Logo } ),
                        Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        BlinkThreshold = 0.0,
                        BlinkDirection = false,
                        VariablePath = new VariablePath("Player/Boost"),
                        MaxVariablePath = new VariablePath("1")
                    },
                }),
                new Layer("Boost Indicator", new PercentGradientLayerHandler()
                {
                    Properties = new PercentGradientLayerHandlerProperties()
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5,
                            DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10,
                            DeviceKeys.F11, DeviceKeys.F12
                        }),
                        Gradient = new EffectsEngine.EffectBrush(new ColorSpectrum(Color.Yellow, Color.Red).SetColorAt(0.75f, Color.OrangeRed)),
                        BlinkThreshold = 0.0,
                        BlinkDirection = false,
                        VariablePath = new VariablePath("Player/Boost"),
                        MaxVariablePath = new VariablePath("1"),
                    },
                }),
                new Layer("Boost Background", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Black,
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5,
                            DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10,
                            DeviceKeys.F11, DeviceKeys.F12
                        })
                    }
                }),
                new Layer("Goal Explosion", new RocketLeagueGoalExplosionLayerHandler()),
                new Layer("Score Split", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        VariablePath = new VariablePath("YourTeam/Goals"),
                        MaxVariablePath = new VariablePath("Match/TotalGoals"),
                        _PrimaryColor = Color.Transparent,
                        _SecondaryColor = Color.Transparent
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicDouble("_Value", new IfElseNumeric( new BooleanAnd( new List<Evaluatable<bool>>{//if match is tied 0 - 0
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("YourTeam/Goals"), new NumberConstant(0)),
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("OpponentTeam/Goals"), new NumberConstant(0))
                                                                                }
                                                                ),
                                                                new NumberConstant(1),//then set the value to 1, so it is split 50-50
                                                                new NumberGSINumeric("YourTeam/Goals")//otherwise set to our goals
                                                )
                    )
                    .SetDynamicDouble("_MaxValue", new IfElseNumeric( new BooleanAnd( new List<Evaluatable<bool>>{//if match is tied 0 - 0
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("YourTeam/Goals"), new NumberConstant(0)),
                                                                                    new BooleanMathsComparison(new NumberGSINumeric("OpponentTeam/Goals"), new NumberConstant(0))
                                                                                }
                                                                ),
                                                                new NumberConstant(2),//then set the max to 2, so it is split 50-50
                                                                new NumberGSINumeric("Match/TotalGoals")//otherwise set to total goals
                                                )
                    )
                    .SetDynamicColor("_PrimaryColor", new NumberConstant(1),
                                                      new NumberGSINumeric("YourTeam/Red"),
                                                      new NumberGSINumeric("YourTeam/Green"),
                                                      new NumberGSINumeric("YourTeam/Blue"))
                    .SetDynamicColor("_SecondaryColor", new NumberConstant(1),
                                                        new NumberGSINumeric("OpponentTeam/Red"),
                                                        new NumberGSINumeric("OpponentTeam/Green"),
                                                        new NumberGSINumeric("OpponentTeam/Blue"))
                    .SetDynamicBoolean("_Enabled", new BooleanGSINumeric("Game/Status", ComparisonOperator.NEQ, -1))
                ),
                new Layer("Background Layer", new SolidFillLayerHandler()
                {
                    Properties = new SolidFillLayerHandlerProperties
                    {
                        _PrimaryColor = Color.Blue
                    }
                }),
            };
        }
    }
}

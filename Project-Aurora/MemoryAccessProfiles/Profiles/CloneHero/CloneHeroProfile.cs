using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Desktop;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using DK = Common.Devices.DeviceKeys;

namespace MemoryAccessProfiles.Profiles.CloneHero;

public class CloneHeroProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers =
        [
            new Layer("Menu Typing Effect", new InteractiveLayerHandler
                {
                    Properties = new InteractiveLayerHandlerProperties
                    {
                        RandomPrimaryColor = false,
                        RandomSecondaryColor = false,
                        EffectSpeed = 20,
                        WaitOnKeyUp = false,
                        InteractiveEffect = InteractiveEffects.KeyPress,
                        EffectWidth = 1,
                        UsePressBuffer = null,
                        SecondaryColor = Color.FromArgb(0, 9, 43, 83),
                        _PrimaryColor = Color.FromArgb(243, 244, 2)
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsAtMenu"))
            ),
            new Layer("Menu Indicator", new GradientLayerHandler
                {
                    Properties = new GradientLayerHandlerProperties
                    {
                        GradientConfig =
                        {
                            Primary = Color.FromArgb(123, 1, 227),
                            Secondary = Color.FromArgb(39, 73, 167),
                            Speed = 3,
                            Angle = 0,
                            AnimationType = AnimationType.TranslateXy,
                            AnimationReverse = false,
                            Brush = new EffectBrush(EffectBrush.BrushType.Linear, EffectBrush.BrushWrap.Repeat)
                            {
                                Start = new PointF
                                {
                                    X = 0,
                                    Y = -0.5f
                                },
                                End = new PointF
                                {
                                    X = 1,
                                    Y = 1
                                },
                                Center = new PointF
                                {
                                    X = 0,
                                    Y = 0
                                },
                                ColorGradients = new SortedDictionary<double, Color>
                                {
                                    { 0, Color.FromArgb(9, 43, 83) },
                                    { 0.298960835f, Color.FromArgb(187, 18, 194) },
                                    { 0.685535133f, Color.FromArgb(17, 185, 217) },
                                    { 1, Color.FromArgb(9, 43, 83) }
                                },
                            }
                        },
                        SecondaryColor = Color.FromArgb(76, 143, 247),
                        _PrimaryColor = Color.FromArgb(156, 65, 212),
                        _Sequence = new KeySequence(new FreeFormObject(-26.54f, -10.15f, 1008.21f, 257.18f))

                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsAtMenu"))
            ),
            new Layer("Combo Break", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationMix = new AnimationMix(new[]
                    {
                        new AnimationTrack("Filled Rectangle Track", 0.5f)
                            .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                            .SetFrame(0.4f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                    }),
                    _AnimationDuration = 0.4f,
                    _AnimationRepeat = 1,
                    _TriggerMode = AnimationTriggerMode.OnLow,
                    TriggerPath = new VariablePath("Player/NoteStreak"),
                    _StackMode = AnimationStackMode.Stack
                }
            }),
            new Layer("Orange Fret", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(255, 134, 0),
                        _Sequence = new KeySequence(new[] { DK.O, DK.P, DK.L, DK.SEMICOLON, DK.PERIOD, DK.FORWARD_SLASH })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsOrangePressed"))
            ),
            new Layer("Blue Fret", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.Blue,
                        _Sequence = new KeySequence(new[] { DK.U, DK.I, DK.J, DK.K, DK.M, DK.COMMA })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsBluePressed"))
            ),
            new Layer("Yellow Fret", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new[] { DK.T, DK.Y, DK.G, DK.H, DK.B, DK.N })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsYellowPressed"))
            ),
            new Layer("Red Fret", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new[] { DK.E, DK.R, DK.D, DK.F, DK.C, DK.V })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsRedPressed"))
            ),
            new Layer("Green Fret", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(0, 255, 69),
                        _Sequence = new KeySequence(new[] { DK.Q, DK.W, DK.A, DK.S, DK.Z, DK.X })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsGreenPressed"))
            ),
            new Layer("FC Indicator", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(208, 181, 0),
                        _Sequence = new KeySequence(new[]
                        {
                            DK.OPEN_BRACKET, DK.CLOSE_BRACKET, DK.APOSTROPHE
                        }),
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsFC"))
            ),
            new Layer("Star Power Indicator", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(200, 100, 100, 100),
                        _Sequence = new KeySequence(new FreeFormObject(-0.78f, 35.13f, 551.5f, 180.7f))
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(0, 227, 255), new BooleanGSIBoolean("Player/IsStarPowerActive"))
                    )
            ),
            new Layer("Star Power Percent", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new FreeFormObject(646.37f, 41.39f, 195f, 148.65f, -90)),
                    _PrimaryColor = Color.FromArgb(0, 227, 255),
                    SecondaryColor = Color.Black,
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/StarPowerPercent"),
                    MaxVariablePath = new VariablePath("100"),
                },
            }),
            new Layer("Note Hit StarPower", new AnimationLayerHandler
                {
                    Properties = new AnimationLayerHandlerProperties
                    {
                        _AnimationMix = new AnimationMix(new[]
                        {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledCircle(70, 19, 7, Color.FromArgb(0, 227, 255), 12, 0.0057f))
                                .SetFrame(0.26f, new AnimationFilledCircle(69, -3, 4, Color.FromArgb(0, 227, 255), 8, 0.1347f))
                        }),
                        _AnimationDuration = 0.4f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnHigh,
                        TriggerPath = new VariablePath("Player/Score"),
                        _StackMode = AnimationStackMode.Stack,
                        _forceKeySequence = true,
                        _Sequence = new KeySequence(new[]
                        {
                            DK.PRINT_SCREEN, DK.SCROLL_LOCK, DK.PAUSE_BREAK, DK.INSERT, DK.HOME, DK.HOME, DK.PAGE_UP,
                            DK.DELETE, DK.END, DK.PAGE_DOWN, DK.ARROW_UP, DK.ARROW_LEFT, DK.ARROW_DOWN, DK.ARROW_RIGHT
                        })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Player/IsStarPowerActive"))
            ),
            new Layer("Note Hit", new AnimationLayerHandler
                {
                    Properties = new AnimationLayerHandlerProperties
                    {
                        _AnimationMix = new AnimationMix(new[]
                        {
                            new AnimationTrack("Filled Rectangle Track", 0.5f)
                                .SetFrame(0, new AnimationFilledCircle(70, 19, 7, Color.FromArgb(246, 125, 0), 12, 0.0057f))
                                .SetFrame(0.26f, new AnimationFilledCircle(69, -3, 4, Color.FromArgb(246, 125, 0), 8, 0.1347f))
                        }),
                        _AnimationDuration = 0.4f,
                        _AnimationRepeat = 1,
                        _TriggerMode = AnimationTriggerMode.OnHigh,
                        TriggerPath = new VariablePath("Player/Score"),
                        _StackMode = AnimationStackMode.Stack,
                        _forceKeySequence = true,
                        _Sequence = new KeySequence(new[]
                        {
                            DK.PRINT_SCREEN, DK.SCROLL_LOCK, DK.PAUSE_BREAK, DK.INSERT, DK.HOME, DK.HOME, DK.PAGE_UP,
                            DK.DELETE, DK.END, DK.PAGE_DOWN, DK.ARROW_UP, DK.ARROW_LEFT, DK.ARROW_DOWN, DK.ARROW_RIGHT
                        })
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanNot(new BooleanGSIBoolean("Player/IsStarPowerActive")))
            ),
            new Layer("Note Streak 4x", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, 36)),
                        _PrimaryColor = Color.FromArgb(199, 0, 255),
                        SecondaryColor = Color.Transparent,
                        BlinkThreshold = 0.0,
                        BlinkDirection = false,
                        VariablePath = new VariablePath("Player/NoteStreak4x"),
                        MaxVariablePath = new VariablePath("1"),
                    },
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(0, 227, 255), new BooleanGSIBoolean("Player/IsStarPowerActive"))
                    )
            ),
            new Layer("Note Streak 3x", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, 36)),
                        _PrimaryColor = Color.FromArgb(0, 255, 59),
                        SecondaryColor = Color.Transparent,
                        BlinkThreshold = 0.0,
                        BlinkDirection = false,
                        VariablePath = new VariablePath("Player/NoteStreak3x"),
                        MaxVariablePath = new VariablePath("10"),
                    },
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(0, 227, 255), new BooleanGSIBoolean("Player/IsStarPowerActive"))
                    )
            ),
            new Layer("Note Streak 2x", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, 36)),
                        _PrimaryColor = Color.FromArgb(255, 146, 0),
                        SecondaryColor = Color.Transparent,
                        BlinkThreshold = 0.0,
                        BlinkDirection = false,
                        VariablePath = new VariablePath("Player/NoteStreak2x"),
                        MaxVariablePath = new VariablePath("10"),
                    },
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(0, 227, 255), new BooleanGSIBoolean("Player/IsStarPowerActive"))
                    )
            ),
            new Layer("Note Streak 1x", new PercentLayerHandler
                {
                    Properties = new PercentLayerHandlerProperties
                    {
                        PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new FreeFormObject((float)-2.75, (float)-2.61, (float)553.3, 36)),
                        _PrimaryColor = Color.FromArgb(255, 247, 44),
                        SecondaryColor = Color.Transparent,
                        BlinkThreshold = 0.0,
                        BlinkDirection = false,
                        VariablePath = new VariablePath("Player/NoteStreak1x"),
                        MaxVariablePath = new VariablePath("10"),
                    }
                }, new OverrideLogicBuilder()
                    .SetLookupTable("_PrimaryColor", new OverrideLookupTableBuilder<Color>()
                        .AddEntry(Color.FromArgb(0, 227, 255), new BooleanGSIBoolean("Player/IsStarPowerActive"))
                    )
            )
        ];
    }
}
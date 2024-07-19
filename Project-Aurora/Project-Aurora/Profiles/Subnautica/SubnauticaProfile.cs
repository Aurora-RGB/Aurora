using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using DK = Common.Devices.DeviceKeys;

namespace AuroraRgb.Profiles.Subnautica;

public class SubnauticaProfile : ApplicationProfile {
    public override void Reset() {
        base.Reset();

        Layers =
        [
            new Layer("PDA Open", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(170, 0, 0, 0),
                        _Sequence = new KeySequence(new FreeFormObject(0, -50, 980, 280))
                    }
                },
                new OverrideLogicBuilder().SetDynamicBoolean(nameof(LayerHandlerProperties._Enabled),
                    new BooleanGSIBoolean("Player/PDAopened"))
            ),
            new Layer("PDA Close Animation", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationDuration = .5f,
                    _AnimationRepeat = 1,
                    _AnimationMix = new AnimationMix(new[]
                    {
                        new AnimationTrack("Rectangle", 1)
                            .SetFrame(0, new AnimationFilledRectangle(new Rectangle(0, 0, 1000, 60), Color.FromArgb(170, 0, 0, 0)))
                            .SetFrame(.5f, new AnimationFilledRectangle(new Rectangle(0, 70, 1000, 60), Color.FromArgb(170, 0, 0, 0)))
                    }),
                    _TriggerMode = AnimationTriggerMode.OnTrue,
                    TriggerPath = new VariablePath("Player/PDAclosing"),
                    _StackMode = AnimationStackMode.Ignore,
                    _Sequence = new KeySequence(new FreeFormObject(0, -50, 980, 280))
                }
            }),
            new Layer("PDA Open Animation", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationDuration = .5f,
                    _AnimationRepeat = 1,
                    _AnimationMix = new AnimationMix(new[]
                    {
                        new AnimationTrack("Rectangle", 1)
                            .SetFrame(0, new AnimationFilledRectangle(new Rectangle(0, 70, 1000, 60), Color.FromArgb(170, 0, 0, 0)))
                            .SetFrame(.5f, new AnimationFilledRectangle(new Rectangle(0, 0, 1000, 60), Color.FromArgb(170, 0, 0, 0)))
                    }),
                    _TriggerMode = AnimationTriggerMode.OnTrue,
                    TriggerPath = new VariablePath("Player/PDAopening"),
                    _StackMode = AnimationStackMode.Ignore,
                    _Sequence = new KeySequence(new FreeFormObject(0, -50, 980, 280))
                }
            }),
            new Layer("Health", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/Health"),
                    MaxVariablePath = new VariablePath("100"),
                    _PrimaryColor = Color.FromArgb(255, 0, 0),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.F1, DK.F2, DK.F3, DK.F4, DK.F5, DK.F6, DK.F7, DK.F8, DK.F9, DK.F10, DK.F11, DK.F12
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Food", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/Food"),
                    MaxVariablePath = new VariablePath("100"),
                    _PrimaryColor = Color.FromArgb(139, 69, 19),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.Q, DK.W, DK.E, DK.R, DK.T, DK.Y, DK.U, DK.I, DK.O, DK.P
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Water", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/Water"),
                    MaxVariablePath = new VariablePath("100"),
                    _PrimaryColor = Color.FromArgb(0, 0, 255),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.A, DK.S, DK.D, DK.F, DK.G, DK.H, DK.J, DK.K, DK.L
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Oxygen", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/OxygenAvailable"),
                    MaxVariablePath = new VariablePath("Player/OxygenCapacity"),
                    _PrimaryColor = Color.FromArgb(0, 170, 65),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE, DK.ZERO, DK.MINUS, DK.EQUALS
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Background", new PercentGradientLayerHandler
            {
                Properties = new PercentGradientLayerHandlerProperties
                {
                    Gradient = new EffectBrush(new ColorSpectrum(Color.FromArgb(46, 176, 255), Color.FromArgb(0, 3, 53)))
                    {
                        Start = new PointF(0, 0),
                        End = new PointF(1, 0),
                    },
                    VariablePath = new VariablePath("Player/DepthLevel"),
                    MaxVariablePath = new VariablePath("-40"),
                    _PrimaryColor = Color.FromArgb(29, 131, 176),
                    SecondaryColor = Color.Transparent,
                    PercentType = PercentEffectType.AllAtOnce,
                    _Sequence = new KeySequence(new FreeFormObject(0, -36, 980, 265))
                }
            })
        ];
    }
}
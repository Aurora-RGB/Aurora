using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;

namespace AuroraRgb.Profiles.Osu;

public class OsuProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();

        // Big thanks to @Ant on the Aurora Discord for creating this default profile :)

        Layers =
        [
            new Layer("Typing", new InteractiveLayerHandler
            {
                Properties = new InteractiveLayerHandlerProperties
                {
                    InteractiveEffect = Desktop.InteractiveEffects.KeyPress,
                    _PrimaryColor = Color.FromArgb(255, 0, 240),
                    SecondaryColor = Color.Black,
                    EffectSpeed = 20,
                    EffectWidth = 5
                }
            }),
            new Layer("Drawing", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.DeepSkyBlue,
                    _Sequence = new KeySequence(new[] { DeviceKeys.C })
                }
            }),
            new Layer("Clickers", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(255, 0, 240),
                    _Sequence = new KeySequence(new[] { DeviceKeys.Z, DeviceKeys.LEFT_ALT })
                }
            }),
            new Layer("Accuracy", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.Lime,
                    SecondaryColor = Color.Red,
                    VariablePath = new VariablePath("Game/Accuracy"),
                    MaxVariablePath = new VariablePath("100"),
                    _Sequence = new KeySequence(new[]
                    {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE, DeviceKeys.SIX,
                        DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO
                    }),
                    PercentType = PercentEffectType.Progressive_Gradual
                }
            }),
            new Layer("Miss", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationMix = new AnimationMix(new[]
                    {
                        new AnimationTrack("Filled Rectangle Track", 0.5f)
                            .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                            .SetFrame(0.5f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                    }),
                    _AnimationDuration = 0.5f,
                    _AnimationRepeat = 1,
                    _TriggerMode = AnimationTriggerMode.OnChange,
                    TriggerPath = new VariablePath("Game/CountMiss"),
                    _StackMode = AnimationStackMode.Stack
                }
            }),
            new Layer("Combo Break", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationMix = new AnimationMix(new[]
                    {
                        new AnimationTrack("Filled Rectangle Track", 0.5f)
                            .SetFrame(0, new AnimationFilledRectangle(0, 0, 800, 300, Color.Red))
                            .SetFrame(0.5f, new AnimationFilledRectangle(0, 0, 800, 300, Color.FromArgb(0, 255, 0, 0)))
                    }),
                    _AnimationDuration = 0.5f,
                    _AnimationRepeat = 1,
                    _TriggerMode = AnimationTriggerMode.OnLow,
                    TriggerPath = new VariablePath("Game/CountMiss"),
                    _StackMode = AnimationStackMode.Stack
                }
            }),
            new Layer("Healthbar", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(0, 160, 255),
                    SecondaryColor = Color.Black,
                    VariablePath = new VariablePath("Game/HP"),
                    MaxVariablePath = new VariablePath("200"),
                    _Sequence = new KeySequence(new[]
                    {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7,
                        DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    PercentType = PercentEffectType.Progressive_Gradual
                }
            }),
            new Layer("Score 300", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationMix = CreateCircleAnimation(Color.FromArgb(0, 170, 255), 5),
                    _AnimationDuration = 1,
                    _AnimationRepeat = 1,
                    _TriggerMode = AnimationTriggerMode.OnHigh,
                    TriggerPath = new VariablePath("Game/Count300"),
                    _StackMode = AnimationStackMode.Stack
                }
            }),
            new Layer("Score 100", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationMix = CreateCircleAnimation(Color.Lime, 4),
                    _AnimationDuration = 1,
                    _AnimationRepeat = 1,
                    _TriggerMode = AnimationTriggerMode.OnHigh,
                    TriggerPath = new VariablePath("Game/Count100"),
                    _StackMode = AnimationStackMode.Stack
                }
            }),
            new Layer("Score 50", new AnimationLayerHandler
            {
                Properties = new AnimationLayerHandlerProperties
                {
                    _AnimationMix = CreateCircleAnimation(Color.Orange, 3),
                    _AnimationDuration = 1,
                    _AnimationRepeat = 1,
                    _TriggerMode = AnimationTriggerMode.OnHigh,
                    TriggerPath = new VariablePath("Game/Count50"),
                    _StackMode = AnimationStackMode.Stack
                }
            }),
            new Layer("Equalizer background", new EqualizerLayerHandler
            {
                Properties = new EqualizerLayerHandlerProperties
                {
                    ViewType = EqualizerPresentationType.GradientColorShift,
                    MaxAmplitude = 25,
                    Gradient = new EffectBrush(new ColorSpectrum(Color.Magenta, Color.Cyan, Color.Magenta)),
                    Frequencies = [60, 170, 310, 600, 1000, 1500, 2000, 2500],
                },
                _Opacity = .2f
            })
        ];
    }

    // Little helper function to create the score circle animations
    private AnimationMix CreateCircleAnimation(Color c, int w) => new(new[]
    {
        new AnimationTrack("Circle Track", 1)
            .SetFrame(0, new AnimationCircle(10, 20, 0, c, w))
            .SetFrame(1, new AnimationCircle(70, 20, 100, c, w))
    });
}
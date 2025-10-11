using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Settings.Overrides.Logic.Boolean;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

namespace AuroraRgb.Profiles.BlackOps6;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
public class Bo6Profile : ApplicationProfile
{
    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        if (Layers.Any(lyr => lyr.Handler is IcueSdkLayerHandler)) return;
        Layers.Add(new Layer("iCUE Lighting", new IcueSdkLayerHandler()));
    }

    public override void Reset()
    {
        base.Reset();
        OverlayLayers =
        [
            new Layer("Loading Animation", new RadialLayerHandler
                {
                    Properties = new RadialLayerProperties
                    {
                        Brush = new SegmentedRadialBrushFactory(new ColorStopCollection
                        {
                            { 0.0f, Color.Red },
                            { 0.10797343f, Color.Orange },
                            { 0.2690457f, Color.FromArgb(0, 121, 212, 0) },
                            { 0.42602244f, Color.FromArgb(93, 222, 0) },
                            { 0.5946844f, Color.Aqua },
                            { 0.7819433f, Color.FromArgb(0, 68, 187, 187) },
                            { 1.0f, Color.Red }
                        })
                    },
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("SDKL_SmokeBlackout"))
            ),
            new Layer("Endgame", new BreathingLayerHandler
                {
                    Properties = new BreathingLayerHandlerProperties
                    {
                        _PrimaryColor = CommonColorUtils.FastColor(35, 5, 215),
                        SecondaryColor = CommonColorUtils.FastColor(255, 0, 29),
                        CurveFunction = CurveFunction.SineSquared,
                        EffectSpeed = 15,
                        Sequence = new KeySequence
                        {
                            Keys = [DeviceKeys.ARROW_LEFT, DeviceKeys.ARROW_DOWN, DeviceKeys.ARROW_RIGHT, DeviceKeys.ARROW_UP],
                        },
                    }
                }, new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanOr(
                            [
                                new BooleanIcueState("BO6_Victory"),
                                new BooleanIcueState("BO6_Defeat")
                            ]
                        )
                    )
            ),
        ];
        Layers =
        [
            new Layer("Death", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence
                        {
                            Keys =
                            [
                                DeviceKeys.ESC,
                                DeviceKeys.TILDE,
                                DeviceKeys.TAB,
                                DeviceKeys.CAPS_LOCK,
                                DeviceKeys.LEFT_SHIFT,
                                DeviceKeys.LEFT_CONTROL,
                                DeviceKeys.LEFT_WINDOWS,
                                DeviceKeys.LEFT_ALT,
                                DeviceKeys.SPACE,
                                DeviceKeys.RIGHT_ALT,
                                DeviceKeys.FN_Key,
                                DeviceKeys.APPLICATION_SELECT,
                                DeviceKeys.RIGHT_CONTROL,
                                DeviceKeys.RIGHT_SHIFT,
                                DeviceKeys.ENTER,
                                DeviceKeys.BACKSPACE,
                                DeviceKeys.F12,
                                DeviceKeys.F11,
                                DeviceKeys.F10,
                                DeviceKeys.F9,
                                DeviceKeys.F8,
                                DeviceKeys.F7,
                                DeviceKeys.F6,
                                DeviceKeys.F5,
                                DeviceKeys.F4,
                                DeviceKeys.F3,
                                DeviceKeys.F2,
                                DeviceKeys.F1,
                            ],
                            Type = KeySequenceType.Sequence,
                        }
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO6_Death"))
            ),
            new Layer("Near Death", new BlinkingLayerHandler
                {
                    Properties = new BlinkingLayerHandlerProperties
                    {
                        _PrimaryColor = Color.Red,
                        SecondaryColor = CommonColorUtils.FastColor(238, 104, 62),
                        _Sequence = new KeySequence
                        {
                            Keys =
                            [
                                DeviceKeys.ESC,
                                DeviceKeys.TILDE,
                                DeviceKeys.TAB,
                                DeviceKeys.CAPS_LOCK,
                                DeviceKeys.LEFT_SHIFT,
                                DeviceKeys.LEFT_CONTROL,
                                DeviceKeys.LEFT_WINDOWS,
                                DeviceKeys.LEFT_ALT,
                                DeviceKeys.SPACE,
                                DeviceKeys.RIGHT_ALT,
                                DeviceKeys.FN_Key,
                                DeviceKeys.APPLICATION_SELECT,
                                DeviceKeys.RIGHT_CONTROL,
                                DeviceKeys.RIGHT_SHIFT,
                                DeviceKeys.ENTER,
                                DeviceKeys.BACKSPACE,
                                DeviceKeys.F12,
                                DeviceKeys.F11,
                                DeviceKeys.F10,
                                DeviceKeys.F9,
                                DeviceKeys.F8,
                                DeviceKeys.F7,
                                DeviceKeys.F6,
                                DeviceKeys.F5,
                                DeviceKeys.F4,
                                DeviceKeys.F3,
                                DeviceKeys.F2,
                                DeviceKeys.F1,
                            ],
                            Type = KeySequenceType.Sequence,
                        },
                        EffectSpeed = 10,
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO6_NearDeath"))
            ),
            new Layer("On Kill", new SimpleParticleLayerHandler
                {
                    Properties = new SimpleParticleLayerProperties
                    {
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        MinSpawnTime = 0.1f,
                        MaxSpawnTime = 0.1f,
                        MinSpawnAmount = 1,
                        MaxSpawnAmount = 1,
                        MinInitialVelocityX = 0.0f,
                        MaxInitialVelocityX = 0.0f,
                        MinInitialVelocityY = -1.0f,
                        MaxInitialVelocityY = -1.0f,
                        MinLifetime = 0.0f,
                        MaxLifetime = 3.0f,
                        AccelerationX = 0.0f,
                        AccelerationY = 0.3f,
                        DragX = 0.0f,
                        DragY = 0.80147064f,
                        MinSize = 30.0f,
                        MaxSize = 35.0f,
                        DeltaSize = 7.0f,
                        SpawnLocation = ParticleSpawnLocations.BottomEdge,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0f, Color.FromArgb(250, 104, 0) },
                            { 1f, Color.FromArgb(0, 250, 103, 0) }
                        },
                        SpawningEnabled = false,
                        PrimaryColor = Color.FromArgb(41, 126, 81),
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("SpawningEnabled", new BooleanIcueEventTriggered("SDKL_FlashOrange", 1.5))
            ),
            new Layer("Main Gradient", new GradientLayerHandler
                {
                    Properties = new GradientLayerHandlerProperties
                    {
                        GradientConfig = new LayerEffectConfig
                        {
                            Primary = Color.FromArgb(117, 226, 176),
                            Secondary = Color.FromArgb(211, 112, 172),
                            Speed = 2.5f,
                            Angle = 0.0f,
                            GradientSize = 28.0f,
                            AnimationType = AnimationType.TranslateXy,
                            Brush = new EffectBrush(EffectBrush.BrushType.Linear, EffectBrush.BrushWrap.Repeat)
                            {
                                Start = new PointF(0, -0.5f),
                                Center = new PointF(0, 0),
                                End = new PointF(1, 1),
                                ColorGradients = new SortedDictionary<double, Color>
                                {
                                    { 0, Color.FromArgb(0, 0, 0, 0) },
                                    { 0.2709030100334449, Color.FromArgb(250, 104, 0) },
                                    { 0.5000000000000002, Color.FromArgb(0, 0, 0, 0) },
                                    { 0.7722169135212613, Color.FromArgb(250, 104, 0) },
                                    { 1, Color.FromArgb(0, 0, 0, 0) }
                                },
                            },
                        },
                        SecondaryColor = Color.FromArgb(213, 204, 113),
                        _PrimaryColor = Color.FromArgb(82, 165, 152),
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO6_ThemeMain"))
            ),
            new Layer("Main Radial", new RadialLayerHandler
                {
                    Properties = new RadialLayerProperties
                    {
                        Brush = new SegmentedRadialBrushFactory(new ColorStopCollection
                        {
                            { 0.058139537f, Color.FromArgb(0, 0, 0, 0) },
                            { 0.18936878f, Color.FromArgb(255, 179, 0) },
                            { 0.3122924f, Color.FromArgb(0, 0, 0, 0) },
                            { 0.45910186f, Color.FromArgb(0, 0, 0, 0) },
                            { 0.5714286f, Color.Orange },
                            { 0.679402f, Color.FromArgb(0, 0, 0, 0) },
                            { 0.7669741f, Color.FromArgb(0, 0, 0, 0) },
                            { 0.85714287f, Color.Orange },
                            { 0.9593023f, Color.FromArgb(0, 0, 0, 0) },
                        }),
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        AnimationSpeed = 60,
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO6_ThemeMain"))
            ),
            new Layer("Background", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(250, 104, 0),
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    },
                },
                new OverrideLogicBuilder()
                    .SetLookupTable(
                        "_LayerOpacity",
                        new OverrideLookupTableBuilder<double>()
                            .AddEntry(0.75, new BooleanIcueState("BO6_ThemeMain"))
                            .AddEntry(0.15, new BooleanConstant(true))
                    )
            ),
        ];
    }
}
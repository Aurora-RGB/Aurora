using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Settings.Overrides.Logic.Boolean;
using AuroraRgb.Settings.Overrides.Logic.Number;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

namespace AuroraRgb.Profiles.BlackOps7;

public class Bo7Profile : ApplicationProfile
{
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
                                new BooleanIcueState("BO7_Victory"),
                                new BooleanIcueState("BO7_Defeat")
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
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO7_Death"))
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
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO7_NearDeath"))
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
            new Layer("Gradient", new GradientLayerHandler
                {
                    Properties =
                    {
                        GradientConfig = new LayerEffectConfig
                        {
                            Speed = 3f,
                            Angle = 0.0f,
                            GradientSize = 24.25f,
                            AnimationType = AnimationType.TranslateXy,
                            Brush = new EffectBrush(EffectBrush.BrushType.Linear, EffectBrush.BrushWrap.Repeat)
                            {
                                Start = new PointF(0, -0.5f),
                                Center = new PointF(0, 0),
                                End = new PointF(1, 1),
                                ColorGradients = new SortedDictionary<double, Color>
                                {
                                    { .0f, Color.FromArgb(0, 250, 103, 0) },
                                    { 0.27f, Color.FromArgb(0, 250, 103, 0) },
                                    { 0.5f, Color.FromArgb(250, 103, 0) },
                                    { 0.74f, Color.FromArgb(0, 250, 103, 0) },
                                    { 1f, Color.FromArgb(0, 250, 103, 0) },
                                },
                            },
                        },
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO7_ThemeMain"))
            ),
            new Layer("Gradient Reversed", new GradientLayerHandler
                {
                    Properties =
                    {
                        GradientConfig = new LayerEffectConfig
                        {
                            Speed = 4.25f,
                            Angle = 0.0f,
                            GradientSize = 24.25f,
                            AnimationType = AnimationType.TranslateXy,
                            AnimationReverse = true,
                            Brush = new EffectBrush(EffectBrush.BrushType.Linear, EffectBrush.BrushWrap.Repeat)
                            {
                                Start = new PointF(0, -0.5f),
                                Center = new PointF(0, 0),
                                End = new PointF(1, 1),
                                ColorGradients = new SortedDictionary<double, Color>
                                {
                                    { .0f, Color.FromArgb(0, 250, 103, 0) },
                                    { 0.27f, Color.FromArgb(0, 250, 103, 0) },
                                    { 0.5f, Color.FromArgb(250, 103, 0) },
                                    { 0.74f, Color.FromArgb(0, 250, 103, 0) },
                                    { 1f, Color.FromArgb(0, 250, 103, 0) },
                                },
                            },
                        },
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("BO7_ThemeMain"))
            ),
            new Layer("Background", new SolidColorLayerHandler
                {
                    Properties = new LayerHandlerProperties
                    {
                        _PrimaryColor = Color.FromArgb(76, 208, 208),
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    },
                },
                new OverrideLogicBuilder()
                    .SetLookupTable(
                        "_LayerOpacity",
                        new OverrideLookupTableBuilder<double>()
                            .AddEntry(0.75, new BooleanIcueState("BO7_ThemeMain"))
                            .AddEntry(0.15, new BooleanConstant(true))
                    )
            ),
        ];
    }
}
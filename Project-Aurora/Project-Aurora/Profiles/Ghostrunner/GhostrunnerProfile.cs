using System.Collections.Generic;
using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Settings.Overrides.Logic.Boolean;
using AuroraRgb.Settings.Overrides.Logic.Number;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;

namespace AuroraRgb.Profiles.Ghostrunner;

public class GhostrunnerProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers =
        [
            new Layer("Sensory", new BinaryCounterLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.FromArgb(154, 147, 66),
                        Sequence = new KeySequence
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.ESC, DeviceKeys.TILDE, DeviceKeys.TAB, DeviceKeys.CAPS_LOCK, DeviceKeys.LEFT_SHIFT, DeviceKeys.LEFT_CONTROL,
                                DeviceKeys.LEFT_WINDOWS, DeviceKeys.BACKSLASH_UK, DeviceKeys.A, DeviceKeys.Q, DeviceKeys.ONE, DeviceKeys.F1, DeviceKeys.F2,
                                DeviceKeys.TWO, DeviceKeys.W, DeviceKeys.S, DeviceKeys.Z, DeviceKeys.LEFT_ALT, DeviceKeys.SPACE, DeviceKeys.X, DeviceKeys.D,
                                DeviceKeys.E, DeviceKeys.THREE, DeviceKeys.F3, DeviceKeys.FOUR, DeviceKeys.F4, DeviceKeys.FIVE, DeviceKeys.R, DeviceKeys.F,
                                DeviceKeys.C, DeviceKeys.V, DeviceKeys.G, DeviceKeys.T, DeviceKeys.SIX, DeviceKeys.F5, DeviceKeys.SEVEN, DeviceKeys.Y,
                                DeviceKeys.H, DeviceKeys.B, DeviceKeys.N, DeviceKeys.J, DeviceKeys.U, DeviceKeys.EIGHT, DeviceKeys.F6, DeviceKeys.F7,
                                DeviceKeys.NINE, DeviceKeys.I, DeviceKeys.K, DeviceKeys.M, DeviceKeys.COMMA, DeviceKeys.RIGHT_ALT,
                                DeviceKeys.PERIOD, DeviceKeys.L, DeviceKeys.O, DeviceKeys.ZERO, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.MINUS,
                                DeviceKeys.P, DeviceKeys.SEMICOLON, DeviceKeys.FORWARD_SLASH, DeviceKeys.FN_Key, DeviceKeys.APPLICATION_SELECT,
                                DeviceKeys.APOSTROPHE, DeviceKeys.OPEN_BRACKET, DeviceKeys.EQUALS, DeviceKeys.F10,
                                DeviceKeys.F11, DeviceKeys.BACKSPACE, DeviceKeys.CLOSE_BRACKET, DeviceKeys.HASHTAG, DeviceKeys.RIGHT_SHIFT,
                                DeviceKeys.RIGHT_CONTROL, DeviceKeys.ENTER, DeviceKeys.F12
                            ]
                        }
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("GHST_Sensory"))
                    .SetDynamicDouble("_Value",
                        new NumberMathsOperation(
                            new NumberMathsOperation(
                                new NumberMathsOperation(
                                    new NumberGSINumeric("LocalPCInfo/Time/MillisecondsSinceEpoch"),
                                    MathsOperator.Div,
                                    16000
                                ),
                                MathsOperator.Mod,
                                20
                            ),
                            MathsOperator.Mul,
                            20
                        )
                    )
            ),
            new Layer("Splash Blue?", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        PrimaryColor = Color.FromArgb(100, 0, 0, 255),
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueEventTriggered("SDKL_SplashBlue", 2))
            ),
            new Layer("Splash Gray?", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        PrimaryColor = Color.FromArgb(100, 128, 128, 128),
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueEventTriggered("SDKL_SplashGray", 2))
            ),
            new Layer("Splash Yellow?", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        PrimaryColor = Color.FromArgb(100, 255, 255, 0),
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueEventTriggered("SDKL_SplashYellow", 2))
            ),
            new Layer("Dead", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.FromArgb(255, 0, 74),
                        Sequence =
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.ESC, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.K, DeviceKeys.L,
                                DeviceKeys.SEMICOLON, DeviceKeys.FORWARD_SLASH, DeviceKeys.RIGHT_SHIFT, DeviceKeys.RIGHT_CONTROL, DeviceKeys.F12,
                                DeviceKeys.F11, DeviceKeys.EQUALS, DeviceKeys.MINUS, DeviceKeys.P, DeviceKeys.O, DeviceKeys.I, DeviceKeys.J, DeviceKeys.H,
                                DeviceKeys.V, DeviceKeys.C, DeviceKeys.X, DeviceKeys.Z, DeviceKeys.BACKSLASH_UK, DeviceKeys.LEFT_CONTROL,
                            ],
                        },
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("GHST_Death"))
            ),
            new Layer("On Pulse Blue?", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.Blue,
                        Sequence =
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.TAB, DeviceKeys.Q, DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I,
                                DeviceKeys.O, DeviceKeys.P, DeviceKeys.OPEN_BRACKET, DeviceKeys.CLOSE_BRACKET, DeviceKeys.ENTER, DeviceKeys.DELETE,
                                DeviceKeys.END,
                                DeviceKeys.PAGE_DOWN, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT, DeviceKeys.NUM_NINE, DeviceKeys.NUM_PLUS,
                                DeviceKeys.CAPS_LOCK,
                                DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.F, DeviceKeys.G, DeviceKeys.H, DeviceKeys.J, DeviceKeys.K, DeviceKeys.L,
                                DeviceKeys.SEMICOLON, DeviceKeys.APOSTROPHE, DeviceKeys.HASHTAG, DeviceKeys.NUM_FOUR, DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX,
                            ]
                        }
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicFloat("_LayerOpacity", new NumberIcueEventFade("SDKL_PulseBarBlue", 2))
            ),
            new Layer("On Deflect", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.White,
                        Sequence =
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.TAB, DeviceKeys.Q, DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I,
                                DeviceKeys.O, DeviceKeys.P, DeviceKeys.OPEN_BRACKET, DeviceKeys.CLOSE_BRACKET, DeviceKeys.ENTER, DeviceKeys.DELETE,
                                DeviceKeys.END,
                                DeviceKeys.PAGE_DOWN, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT, DeviceKeys.NUM_NINE, DeviceKeys.NUM_PLUS,
                                DeviceKeys.CAPS_LOCK,
                                DeviceKeys.A, DeviceKeys.S, DeviceKeys.D, DeviceKeys.F, DeviceKeys.G, DeviceKeys.H, DeviceKeys.J, DeviceKeys.K, DeviceKeys.L,
                                DeviceKeys.SEMICOLON, DeviceKeys.APOSTROPHE, DeviceKeys.HASHTAG, DeviceKeys.NUM_FOUR, DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX,
                            ]
                        }
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicFloat("_LayerOpacity", new NumberIcueEventFade("SDKL_PulseBarWhite", 2))
            ),
            new Layer("On Kill", new SimpleParticleLayerHandler
                {
                    Properties =
                    {
                        SpawnLocation = ParticleSpawnLocations.Random,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0.0f, Color.Red },
                            { 1.0f, Color.FromArgb(0, 255, 0, 0) },
                        },
                        MinSpawnTime = 0.1f,
                        MaxSpawnTime = 0.1f,
                        MinSpawnAmount = 1,
                        MaxSpawnAmount = 1,
                        MinInitialVelocityX = 0.0f,
                        MaxInitialVelocityX = 0.0f,
                        MinInitialVelocityY = 0.0f,
                        MaxInitialVelocityY = 0.0f,
                        MinLifetime = 1.0f,
                        MaxLifetime = 2.0f,
                        AccelerationX = 0.0f,
                        AccelerationY = 0.0f,
                        DragX = 0.0f,
                        DragY = 0.0f,
                        MinSize = 30.0f,
                        MaxSize = 40.0f,
                        DeltaSize = 0.0f,
                        Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("SpawningEnabled", new BooleanIcueEventTriggered("SDKL_Melee", 0.6))
            ),
            new Layer("AlertYellow", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.Yellow,
                        Sequence =
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.ESC, DeviceKeys.TILDE, DeviceKeys.TAB, DeviceKeys.CAPS_LOCK, DeviceKeys.LEFT_SHIFT, DeviceKeys.LEFT_CONTROL,
                                DeviceKeys.LEFT_WINDOWS, DeviceKeys.LEFT_ALT, DeviceKeys.SPACE, DeviceKeys.RIGHT_ALT, DeviceKeys.FN_Key,
                                DeviceKeys.APPLICATION_SELECT, DeviceKeys.RIGHT_CONTROL, DeviceKeys.RIGHT_SHIFT, DeviceKeys.ENTER, DeviceKeys.BACKSPACE,
                                DeviceKeys.F12, DeviceKeys.F11, DeviceKeys.F10, DeviceKeys.F9, DeviceKeys.F8, DeviceKeys.F7, DeviceKeys.F6,
                                DeviceKeys.F5, DeviceKeys.F4, DeviceKeys.F3, DeviceKeys.F2, DeviceKeys.F1
                            ]
                        },
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueEventTriggered("SDKL_AlertEdgesYellow", 2))
                    .SetDynamicFloat("_LayerOpacity", new NumberIcueEventFade("SDKL_AlertEdgesYellow", 2))
            ),
            new Layer("AlertOrange", new SolidColorLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.FromArgb(255, 180, 0),
                        Sequence =
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.ESC, DeviceKeys.TILDE, DeviceKeys.TAB, DeviceKeys.CAPS_LOCK, DeviceKeys.LEFT_SHIFT, DeviceKeys.LEFT_CONTROL,
                                DeviceKeys.LEFT_WINDOWS, DeviceKeys.LEFT_ALT, DeviceKeys.SPACE, DeviceKeys.RIGHT_ALT, DeviceKeys.FN_Key,
                                DeviceKeys.APPLICATION_SELECT, DeviceKeys.RIGHT_CONTROL, DeviceKeys.RIGHT_SHIFT, DeviceKeys.ENTER, DeviceKeys.BACKSPACE,
                                DeviceKeys.F12, DeviceKeys.F11, DeviceKeys.F10, DeviceKeys.F9, DeviceKeys.F8, DeviceKeys.F7, DeviceKeys.F6,
                                DeviceKeys.F5, DeviceKeys.F4, DeviceKeys.F3, DeviceKeys.F2, DeviceKeys.F1
                            ]
                        },
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueEventTriggered("SDKL_AlertEdgesOrange", 2))
                    .SetDynamicFloat("_LayerOpacity", new NumberIcueEventFade("SDKL_AlertEdgesOrange", 2))
            ),
            new Layer("Zipline (Right)", new GradientLayerHandler
                {
                    Properties =
                    {
                        GradientConfig =
                        {
                            Speed = 10,
                            Angle = -45,
                            GradientSize = 38.75f,
                            AnimationType = AnimationType.TranslateXy,
                            AnimationReverse = true,
                            Brush = new EffectBrush(EffectBrush.BrushType.Linear, EffectBrush.BrushWrap.Repeat)
                            {
                                Start = new PointF(0, -0.5f),
                                Center = new PointF(0, 0),
                                End = new PointF(1, 1),
                                ColorGradients = new SortedDictionary<double, Color>
                                {
                                    { 0, Color.FromArgb(0, 77, 16, 190) },
                                    { 0.25, Color.FromArgb(0, 255, 179, 0) },
                                    { 0.5, Color.FromArgb(255, 180, 0) },
                                    { 0.75, Color.FromArgb(0, 255, 179, 0) },
                                    { 1, Color.FromArgb(0, 145, 155, 176) },
                                },
                            },
                        },
                        Sequence =
                        {
                            Type = KeySequenceType.FreeForm,
                            Freeform =
                            {
                                X = 290, Y = 0,
                                Width = 550,
                                Height = 230
                            }
                        },
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("GHST_Zipline"))
            ),
            new Layer("Zipline (Left)", new GradientLayerHandler
                {
                    Properties =
                    {
                        GradientConfig =
                        {
                            Speed = 10,
                            Angle = 45,
                            GradientSize = 38.75f,
                            AnimationType = AnimationType.TranslateXy,
                            AnimationReverse = false,
                            Brush = new EffectBrush(EffectBrush.BrushType.Linear, EffectBrush.BrushWrap.Repeat)
                            {
                                Start = new PointF(0, -0.5f),
                                Center = new PointF(0, 0),
                                End = new PointF(1, 1),
                                ColorGradients = new SortedDictionary<double, Color>
                                {
                                    { 0, Color.FromArgb(0, 77, 16, 190) },
                                    { 0.25, Color.FromArgb(0, 255, 179, 0) },
                                    { 0.5, Color.FromArgb(255, 180, 0) },
                                    { 0.75, Color.FromArgb(0, 255, 179, 0) },
                                    { 1, Color.FromArgb(0, 145, 155, 176) },
                                },
                            },
                        },
                        Sequence =
                        {
                            Type = KeySequenceType.FreeForm,
                            Freeform =
                            {
                                X = 0, Y = 0,
                                Width = 290,
                                Height = 230
                            }
                        },
                    },
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("GHST_Zipline"))
            ),
            new Layer("Main Menu particles", new SimpleParticleLayerHandler
                {
                    Properties =
                    {
                        SpawnLocation = ParticleSpawnLocations.Random,
                        ParticleColorStops = new ColorStopCollection
                        {
                            { 0.0f, Color.Transparent },
                            { 0.25f, Color.DeepSkyBlue },
                            { 0.5f, Color.Purple },
                            { 0.75f, Color.FromArgb(0, 131, 200) },
                            { 1.0f, Color.FromArgb(0, 0, 131, 200) },
                        },
                        MinSpawnTime = 0,
                        MaxSpawnTime = 2,
                        MinSpawnAmount = 7,
                        MaxSpawnAmount = 7,
                        MinLifetime = 1,
                        MaxLifetime = 5,
                        MinInitialVelocityX = 0,
                        MaxInitialVelocityX = 0,
                        MinInitialVelocityY = 0,
                        MaxInitialVelocityY = 0,
                        AccelerationX = 0f,
                        AccelerationY = 0.0f,
                        DragX = 0f,
                        DragY = 0f,
                        MinSize = 8,
                        MaxSize = 8,
                        DeltaSize = 0,
                        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("GHST_Menu"))
            ),
            new Layer("Main Menu binary", new BinaryCounterLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.Maroon,
                        Sequence = new KeySequence
                        {
                            Type = KeySequenceType.Sequence,
                            Keys =
                            [
                                DeviceKeys.ESC, DeviceKeys.TILDE, DeviceKeys.TAB, DeviceKeys.CAPS_LOCK, DeviceKeys.LEFT_SHIFT, DeviceKeys.LEFT_CONTROL,
                                DeviceKeys.LEFT_WINDOWS, DeviceKeys.BACKSLASH_UK, DeviceKeys.A, DeviceKeys.Q, DeviceKeys.ONE, DeviceKeys.F1, DeviceKeys.F2,
                                DeviceKeys.TWO, DeviceKeys.W, DeviceKeys.S, DeviceKeys.Z, DeviceKeys.LEFT_ALT, DeviceKeys.SPACE, DeviceKeys.X, DeviceKeys.D,
                                DeviceKeys.E, DeviceKeys.THREE, DeviceKeys.F3, DeviceKeys.FOUR, DeviceKeys.F4, DeviceKeys.FIVE, DeviceKeys.R, DeviceKeys.F,
                                DeviceKeys.C, DeviceKeys.V, DeviceKeys.G, DeviceKeys.T, DeviceKeys.SIX, DeviceKeys.F5, DeviceKeys.SEVEN, DeviceKeys.Y,
                                DeviceKeys.H, DeviceKeys.B, DeviceKeys.N, DeviceKeys.J, DeviceKeys.U, DeviceKeys.EIGHT, DeviceKeys.F6, DeviceKeys.F7,
                                DeviceKeys.NINE, DeviceKeys.I, DeviceKeys.K, DeviceKeys.M, DeviceKeys.COMMA, DeviceKeys.RIGHT_ALT,
                                DeviceKeys.PERIOD, DeviceKeys.L, DeviceKeys.O, DeviceKeys.ZERO, DeviceKeys.F8, DeviceKeys.F9, DeviceKeys.MINUS,
                                DeviceKeys.P, DeviceKeys.SEMICOLON, DeviceKeys.FORWARD_SLASH, DeviceKeys.FN_Key, DeviceKeys.APPLICATION_SELECT,
                                DeviceKeys.APOSTROPHE, DeviceKeys.OPEN_BRACKET, DeviceKeys.EQUALS, DeviceKeys.F10,
                                DeviceKeys.F11, DeviceKeys.BACKSPACE, DeviceKeys.CLOSE_BRACKET, DeviceKeys.HASHTAG, DeviceKeys.RIGHT_SHIFT,
                                DeviceKeys.RIGHT_CONTROL, DeviceKeys.ENTER, DeviceKeys.F12
                            ]
                        }
                    }
                },
                new OverrideLogicBuilder()
                    .SetDynamicBoolean("_Enabled", new BooleanIcueState("GHST_Menu"))
                    .SetDynamicDouble("_Value",
                        new NumberMathsOperation(
                            new NumberMathsOperation(
                                new NumberMathsOperation(
                                    new NumberGSINumeric("LocalPCInfo/Time/MillisecondsSinceEpoch"),
                                    MathsOperator.Div,
                                    16000
                                ),
                                MathsOperator.Mod,
                                20
                            ),
                            MathsOperator.Mul,
                            20
                        )
                    )
            ),
            new Layer("Zone Background", new BreathingLayerHandler
                {
                    Properties =
                    {
                        PrimaryColor = Color.Transparent,
                        SecondaryColor = Color.FromArgb(5, 50, 7),
                        EffectSpeed = 4,
                        CurveFunction = CurveFunction.Sine,
                        Sequence = new KeySequence(Effects.Canvas.WholeFreeForm),
                        _LayerOpacity = 0.3,
                    }
                },
                new OverrideLogicBuilder()
                    .SetLookupTable(
                        "SecondaryColor",
                        new OverrideLookupTableBuilder<Color>()
                            .AddEntry(Color.FromArgb(24, 150, 255), new BooleanIcueState("GHST_CyberVoid"))
                            .AddEntry(Color.FromArgb(255, 47, 0), new BooleanIcueState("GHST_CyberVoidRed"))
                            .AddEntry(Color.FromArgb(35, 117, 46), new BooleanIcueState("GHST_CyberVoidDestination"))
                            .AddEntry(Color.FromArgb(255, 40, 0), new BooleanIcueState("GHST_IndustryRed"))
                            .AddEntry(Color.Maroon, new BooleanIcueState("GHST_Dharma"))
                            .AddEntry(Color.FromArgb(5, 50, 7), new BooleanIcueState("GHST_IndustryGreen"))
                            .AddEntry(Color.FromArgb(13, 14, 30), new BooleanIcueState("GHST_IndustryMain"))
                            .AddEntry(Color.FromArgb(133, 61, 11), new BooleanIcueState("GHST_IndustryOrange"))
                    )
            ),
            new Layer("Background", new SolidFillLayerHandler
            {
                Properties =
                {
                    PrimaryColor = Color.FromArgb(0, 28, 64)
                }
            }),
        ];
    }
}
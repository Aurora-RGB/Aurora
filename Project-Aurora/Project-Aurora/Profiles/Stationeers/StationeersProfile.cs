using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using DK = Common.Devices.DeviceKeys;

namespace AuroraRgb.Profiles.Stationeers;

public class StationeersProfile : ApplicationProfile {
    public override void Reset() {
        base.Reset();

        Layers =
        [
            new Layer("Health", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/Health"),
                    MaxVariablePath = new VariablePath("200"),
                    _PrimaryColor = Color.FromArgb(25, 0, 51),
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
                    _PrimaryColor = Color.FromArgb(255, 110, 0),
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
                        DK.A, DK.S, DK.D, DK.F, DK.G, DK.H, DK.J, DK.K, DK.L , DK.SEMICOLON
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Oxygen", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/OxygenTankLevel"),
                    MaxVariablePath = new VariablePath("Player/OxygenTankCapacity"),
                    _PrimaryColor = Color.FromArgb(0, 153, 153),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.ONE, DK.TWO, DK.THREE, DK.FOUR, DK.FIVE, DK.SIX, DK.SEVEN, DK.EIGHT, DK.NINE, DK.ZERO, DK.MINUS, DK.EQUALS
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Waste", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/WasteTankLevel"),
                    MaxVariablePath = new VariablePath("Player/WasteTankCapacity"),
                    _PrimaryColor = Color.FromArgb(255, 0, 0),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.Z, DK.X, DK.C, DK.V, DK.B, DK.N, DK.M, DK.COMMA,DK.PERIOD
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Temperature", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    VariablePath = new VariablePath("Player/Temperature"),
                    MaxVariablePath = new VariablePath("100"),
                    _PrimaryColor = Color.FromArgb(255, 0, 0),
                    SecondaryColor = Color.Transparent,
                    _Sequence = new KeySequence(new[]
                    {
                        DK.NUM_ZERO, DK.NUM_PERIOD,DK.NUM_ONE, DK.NUM_TWO, DK.NUM_THREE, DK.NUM_FOUR, DK.NUM_FIVE, DK.NUM_SIX, DK.NUM_SEVEN, DK.NUM_EIGHT, DK.NUM_NINE
                    }),
                    BlinkThreshold = 0.25
                }
            }),
            new Layer("Background", new PercentGradientLayerHandler
            {
                Properties = new PercentGradientLayerHandlerProperties
                {
                    Gradient = new EffectBrush(new ColorSpectrum(Color.FromArgb(0, 0, 0), Color.FromArgb(0, 255, 0)))
                    {
                        Start = new PointF(0, 0),
                        End = new PointF(1, 0),
                    },
                    VariablePath = new VariablePath("Player/Battery"),
                    MaxVariablePath = new VariablePath("100"),
                    _PrimaryColor = Color.FromArgb(29, 131, 176),
                    SecondaryColor = Color.Transparent,
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new FreeFormObject(0, -36, 980, 265))
                }
            })
        ];
    }
}
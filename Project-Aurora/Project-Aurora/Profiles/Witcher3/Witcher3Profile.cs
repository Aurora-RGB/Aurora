using System.Collections.ObjectModel;
using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Witcher3.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;

namespace AuroraRgb.Profiles.Witcher3;

public class Witcher3Profile : ApplicationProfile
{

    public override void Reset()
    {
        base.Reset();
        Layers = new ObservableCollection<Layer>
        {
            new("Health Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor =  Color.Red,
                    SecondaryColor = Color.DarkRed,
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[] {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/CurrentHealth"),
                    MaxVariablePath = new VariablePath("Player/MaximumHealth")
                },
            }),
            new("Toxicity Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor =  Color.Olive,
                    SecondaryColor = Color.DarkOliveGreen,
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[] {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                        DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                        DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/Toxicity"),
                    MaxVariablePath = new VariablePath("100")
                },
            }),
            new("Stamina Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor =  Color.Orange,
                    SecondaryColor = Color.DarkOrange,
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[] {
                        DeviceKeys.NUM_ONE, DeviceKeys.NUM_TWO, DeviceKeys.NUM_THREE, DeviceKeys.NUM_FOUR,
                        DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT,
                        DeviceKeys.NUM_NINE
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/Stamina"),
                    MaxVariablePath = new VariablePath("100")
                },
            }),
            new("BG", new Witcher3BackgroundLayerHandler()),
        };
    }
}
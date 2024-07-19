using System.Drawing;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;

namespace MemoryAccessProfiles.Profiles.Borderlands2;

public class Borderlands2Profile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
        {
            new Layer("Health Indicator", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor = Color.Red,
                    SecondaryColor = Color.DarkRed,
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR, DeviceKeys.FIVE,
                        DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT, DeviceKeys.NINE, DeviceKeys.ZERO,
                        DeviceKeys.MINUS, DeviceKeys.EQUALS
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/CurrentHealth"),
                    MaxVariablePath = new VariablePath("Player/MaximumHealth")
                },
            }),
            new Layer("Shield Indicator", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor =  Color.Cyan,
                    SecondaryColor = Color.DarkCyan,
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/CurrentShield"),
                    MaxVariablePath = new VariablePath("Player/MaximumShield")
                },
            }),
            new Layer("Borderlands 2 Background", new SolidFillLayerHandler(){
                Properties = new SolidFillLayerHandlerProperties()
                {
                    _PrimaryColor = Color.LightGoldenrodYellow
                }
            })
        };
    }
}
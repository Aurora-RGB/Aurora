using System.Drawing;
using AuroraRgb.Profiles.Payday_2.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;

namespace AuroraRgb.Profiles.Payday_2;

public class PD2Profile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers =
        [
            new Layer("Payday 2 Flashbang", new PD2FlashbangLayerHandler()),
            new Layer("Health Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(0, 255, 0),
                    SecondaryColor = Color.FromArgb(255, 0, 0),
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence([
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    ]),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("LocalPlayer/Health/Current"),
                    MaxVariablePath = new VariablePath("LocalPlayer/Health/Max")
                },
            }),

            new Layer("Ammo Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(0, 0, 255),
                    SecondaryColor = Color.FromArgb(255, 0, 0),
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence([
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                        DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                        DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                    ]),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("LocalPlayer/Weapons/SelectedWeapon/Current_Clip"),
                    MaxVariablePath = new VariablePath("LocalPlayer/Weapons/SelectedWeapon/Max_Clip")
                },
            }),

            new Layer("Payday 2 States", new PD2StatesLayerHandler()),
            new Layer("Payday 2 Background", new PD2BackgroundLayerHandler())
        ];
    }
}
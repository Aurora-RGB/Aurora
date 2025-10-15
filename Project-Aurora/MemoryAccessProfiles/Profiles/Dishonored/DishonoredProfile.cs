using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;

namespace MemoryAccessProfiles.Profiles.Dishonored;

public class DishonoredProfile : ApplicationProfile
{
    public DishonoredProfile() : base()
    {
            
    }

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
                    SecondaryColor = Color.FromArgb(255,70,0,0),
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
            new Layer("Mana Indicator", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor =  Color.Blue,
                    SecondaryColor = Color.FromArgb(255,0,0,70),
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/CurrentMana"),
                    MaxVariablePath = new VariablePath("Player/MaximumMana")
                },
            }),
            new Layer("Mana Potions", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor =  Color.Blue,
                    SecondaryColor = Color.FromArgb(255,0,0,70),
                    PercentType = PercentEffectType.Progressive,
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.DELETE, DeviceKeys.END, DeviceKeys.PAGE_DOWN,
                        DeviceKeys.INSERT, DeviceKeys.HOME, DeviceKeys.PAGE_UP,
                        DeviceKeys.PRINT_SCREEN, DeviceKeys.SCROLL_LOCK, DeviceKeys.PAUSE_BREAK
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/ManaPots"),
                    MaxVariablePath = new VariablePath("9")
                },
            }),
            new Layer("Health Potions", new PercentLayerHandler()
            {
                Properties = new PercentLayerHandlerProperties()
                {
                    _PrimaryColor =  Color.Red,
                    SecondaryColor = Color.FromArgb(255,70,0,0),
                    PercentType = PercentEffectType.Progressive,
                    _Sequence = new KeySequence(new DeviceKeys[] {
                        DeviceKeys.NUM_ONE, DeviceKeys.NUM_TWO, DeviceKeys.NUM_THREE, DeviceKeys.NUM_FOUR,
                        DeviceKeys.NUM_FIVE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT,
                        DeviceKeys.NUM_NINE
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Player/HealthPots"),
                    MaxVariablePath = new VariablePath("9")
                },
            }),
            new Layer("Background", new SolidFillLayerHandler(){
                Properties = new SolidFillLayerHandlerProperties()
                {
                    _PrimaryColor = Color.Gray
                }
            })
        };
    }
}
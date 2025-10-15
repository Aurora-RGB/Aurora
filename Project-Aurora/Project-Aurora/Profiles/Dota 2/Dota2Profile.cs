using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Dota_2.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using Common.Devices;

namespace AuroraRgb.Profiles.Dota_2;

public class Dota2Profile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        Layers =
        [
            new Layer("Dota 2 Respawn", new Dota2RespawnLayerHandler()),
            new Layer("Health Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(0, 255, 0),
                    SecondaryColor = Color.FromArgb(0, 60, 0),
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[]
                    {
                        DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                        DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                        DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Hero/Health"),
                    MaxVariablePath = new VariablePath("Hero/MaxHealth")
                },
            }),
            new Layer("Mana Indicator", new PercentLayerHandler
            {
                Properties = new PercentLayerHandlerProperties
                {
                    _PrimaryColor = Color.FromArgb(0, 125, 255),
                    SecondaryColor = Color.FromArgb(0, 0, 60),
                    PercentType = PercentEffectType.Progressive_Gradual,
                    _Sequence = new KeySequence(new[]
                    {
                        DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                        DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                        DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                    }),
                    BlinkThreshold = 0.0,
                    BlinkDirection = false,
                    VariablePath = new VariablePath("Hero/Mana"),
                    MaxVariablePath = new VariablePath("Hero/MaxMana")
                },
            }),
            new Layer("Dota 2 Ability Keys", new Dota2AbilityLayerHandler()),
            new Layer("Dota 2 Item Keys", new Dota2ItemLayerHandler()),
            new Layer("Dota 2 Hero Ability Effects", new Dota2HeroAbilityEffectsLayerHandler()),
            new Layer("Dota 2 Killstreaks", new Dota2KillstreakLayerHandler()),
            new Layer("Dota 2 Background", new Dota2BackgroundLayerHandler())
        ];
    }
}
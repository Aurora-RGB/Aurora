using System.Drawing;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Discord.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Settings.Overrides.Logic;
using Common.Devices;

namespace AuroraRgb.Profiles.Discord;

public class DiscordProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();

        //makes the color red when mic is muted
        var overrideLookupTableBuilder = new OverrideLookupTableBuilder<Color>();
        overrideLookupTableBuilder.AddEntry(Color.Red, new BooleanGSIBoolean("User/SelfMute"));

        OverlayLayers =
        [
            new("Mic Status", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.Lime,
                    _Sequence = new KeySequence([DeviceKeys.PAUSE_BREAK])
                }
            }, new OverrideLogicBuilder()
                .SetDynamicBoolean("_Enabled", new StringComparison(
                        new StringGSIString { VariablePath = new VariablePath("Voice/Name") },
                        "",
                        StringComparisonOperator.NotEqual
                    )
                ).SetLookupTable("_PrimaryColor", overrideLookupTableBuilder)),

            new("Mentions", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.Yellow,
                    _Sequence = new KeySequence([DeviceKeys.PRINT_SCREEN])
                }
            }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/Mentions"))),

            new("Unread Messages", new SolidColorLayerHandler
            {
                Properties = new LayerHandlerProperties
                {
                    _PrimaryColor = Color.LightBlue,
                    _Sequence = new KeySequence([DeviceKeys.PRINT_SCREEN, DeviceKeys.SCROLL_LOCK, DeviceKeys.PAUSE_BREAK])
                }
            }, new OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("User/UnreadMessages"))),

            new ("Voice Activity", new DiscordVoiceActivityLayerHandler
            {
                Properties = new DiscordVoiceActivityLayerHandlerProperties
                {
                    _Sequence = new KeySequence([
                        DeviceKeys.INSERT, DeviceKeys.HOME, DeviceKeys.PAGE_UP, DeviceKeys.DELETE, DeviceKeys.END, DeviceKeys.PAGE_DOWN
                    ])
                }
            }),
        ];
    }
}
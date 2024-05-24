using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Settings;
using Common.Devices;
using Common.Utils;

namespace AuroraRgb.Modules;

public sealed class InputsModule : AuroraModule
{
    private static readonly TaskCompletionSource<IInputEvents> Tcs = new();
    /// <summary>
    /// Input event subscriptions
    /// </summary>
    public static Task<IInputEvents> InputEvents { get; } = Tcs.Task;
    
    private const bool VolumeToBrightnessReady = false;
    
    public override async Task InitializeAsync()
    {
        await Initialize();
    }

    protected override async Task Initialize()
    {
        if (Global.Configuration.EnableInputCapture)
        {
            Global.logger.Information("Loading Input Hooking");
            Tcs.SetResult(new InputEvents());
            if (VolumeToBrightnessReady)
            {
                Global.Configuration.PropertyChanged += SetupVolumeAsBrightness;
                SetupVolumeAsBrightness(Global.Configuration,
                    new PropertyChangedEventArgs(nameof(Global.Configuration.UseVolumeAsBrightness)));
            }
            Global.logger.Information("Loaded Input Hooking");
        }
        else
        {
            Tcs.SetResult(new NoopInputEvents());
        }

        DesktopUtils.StartSessionWatch();

        Global.key_recorder = new KeyRecorder(await InputEvents);
    }

    public override async ValueTask DisposeAsync()
    {
        Global.key_recorder?.Dispose();
        if (InputEvents.IsCompletedSuccessfully)
        {
            (await InputEvents).Dispose();
        }
    }

    private static async void SetupVolumeAsBrightness(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName != nameof(Global.Configuration.UseVolumeAsBrightness)) return;

        var inputEvents = await InputEvents;

        inputEvents.KeyDown -= InterceptVolumeAsBrightness;
        if (Global.Configuration.UseVolumeAsBrightness)
        {
            inputEvents.KeyDown += InterceptVolumeAsBrightness;
        }
    }
    
    private static async void InterceptVolumeAsBrightness(object? sender, KeyboardKeyEventArgs e)
    {
        if (!(await InputEvents).Alt) return;
        
        e.Intercepted = true;
        await Task.Factory.StartNew(async () =>
            {
                var brightness = Global.Configuration.GlobalBrightness;
                brightness += e.GetDeviceKey() == DeviceKeys.VOLUME_UP ? 0.05f : -0.05f;
                Global.Configuration.GlobalBrightness = Math.Max(0f, Math.Min(1f, brightness));

                await ConfigManager.SaveAsync(Global.Configuration);
            }
        );
    }
}
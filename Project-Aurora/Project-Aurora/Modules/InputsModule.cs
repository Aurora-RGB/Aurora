using System.Threading.Tasks;
using AuroraRgb.Modules.Inputs;
using Common.Utils;

namespace AuroraRgb.Modules;

public sealed class InputsModule : AuroraModule
{
    private static readonly TaskCompletionSource<IInputEvents> Tcs = new();
    /// <summary>
    /// Input event subscriptions
    /// </summary>
    public static Task<IInputEvents> InputEvents { get; } = Tcs.Task;

    public override async Task InitializeAsync()
    {
        await Initialize();
    }

    protected override Task Initialize()
    {
        if (Global.Configuration.EnableInputCapture)
        {
            Global.logger.Information("Loading Input Hooking");

            var inputEvents = new InputEvents();
            Global.key_recorder = new KeyRecorder(inputEvents);
            Tcs.SetResult(inputEvents);
            Global.logger.Information("Loaded Input Hooking");
        }
        else
        {
            var inputEvents = new NoopInputEvents();
            Global.key_recorder = new KeyRecorder(inputEvents);
            Tcs.SetResult(inputEvents);
        }

        DesktopUtils.StartSessionWatch();
        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        Global.key_recorder?.Dispose();
        if (InputEvents.IsCompletedSuccessfully)
        {
            (await InputEvents).Dispose();
        }
    }
}
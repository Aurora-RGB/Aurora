using System.Threading.Tasks;
using AuroraRgb.Modules.Layouts;
using AuroraRgb.Settings;
using Lombok.NET;
using RazerSdkReader;

namespace AuroraRgb.Modules;

public sealed partial class LayoutsModule(Task<ChromaReader?> rzSdk, Task onlineSettingsLayoutsUpdate) : AuroraModule
{
    private KeyboardLayoutManager? _layoutManager;
    private readonly TaskCompletionSource<KeyboardLayoutManager> _taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<KeyboardLayoutManager> LayoutManager => _taskCompletionSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading KB Layouts");
        _layoutManager = new KeyboardLayoutManager(rzSdk);
        Global.kbLayout = _layoutManager;
        await onlineSettingsLayoutsUpdate;
        await Global.kbLayout.Initialize();
        Global.logger.Information("Loaded KB Layouts");
        _taskCompletionSource.SetResult(_layoutManager);
    }

    [Async]
    public override void Dispose()
    {
        //noop
    }
}
using System.Threading.Tasks;
using AuroraRgb.Modules.Layouts;
using AuroraRgb.Modules.Razer;

namespace AuroraRgb.Modules;

public sealed class LayoutsModule(Task<ChromaSdkManager> rzSdk, Task onlineSettingsLayoutsUpdate) : AuroraModule
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

    public override ValueTask DisposeAsync()
    {
        _layoutManager?.Dispose();
        return ValueTask.CompletedTask;
    }
}
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules;

public sealed class PluginsModule : AuroraModule
{
    private readonly TaskCompletionSource<PluginManager> _pluginManagerSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<PluginManager> PluginManager => _pluginManagerSource.Task;

    private PluginManager? _pluginManager;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading Plugins");
        _pluginManager = new PluginManager();
        await _pluginManager.Initialize(CancellationToken.None);
        _pluginManagerSource.SetResult(_pluginManager);
        Global.logger.Information("Loaded Plugins");
    }

    public override ValueTask DisposeAsync()
    {
        _pluginManager?.SaveSettings();
        return ValueTask.CompletedTask;
    }
}
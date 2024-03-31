using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Settings;
using Lombok.NET;

namespace AuroraRgb.Modules;

public sealed partial class PluginsModule : AuroraModule
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

    [Async]
    public override void Dispose()
    {
        _pluginManager?.SaveSettings();
    }
}
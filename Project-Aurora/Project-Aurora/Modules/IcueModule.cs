using System.ComponentModel;
using System.Threading.Tasks;
using AuroraRgb.Modules.Icue;
using AuroraRgb.Modules.ProcessMonitor;

namespace AuroraRgb.Modules;

public sealed class IcueModule(Task<RunningProcessMonitor> processMonitorTask) : AuroraModule
{
    private RunningProcessMonitor? _processMonitor;
    public static AuroraIcueServer AuroraIcueServer { get; } = new();

    public override async Task InitializeAsync()
    {
        _processMonitor = await processMonitorTask;
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        if (Global.Configuration.EnableIcueTakeover)
        {
            await Initialize();
        }
    }

    protected override async Task Initialize()
    {
        _processMonitor = await processMonitorTask;
        AuroraIcueServer.RunServer(_processMonitor);
    }

    private void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.EnableIcueTakeover))
        {
            return;
        }

        if (Global.Configuration.EnableIcueTakeover)
        {
            AuroraIcueServer.RunServer(_processMonitor!);
        }
        else
        {
            AuroraIcueServer.Dispose();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await AuroraIcueServer.DisposeAsync();
    }
}
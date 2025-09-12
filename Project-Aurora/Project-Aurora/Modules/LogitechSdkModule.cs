using System.ComponentModel;
using System.Threading.Tasks;
using AuroraRgb.Modules.Logitech;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules;

public sealed class LogitechSdkModule(Task<RunningProcessMonitor> runningProcessMonitor) : AuroraModule
{
    public static LogitechSdkListener LogitechSdkListener { get; } = new();

    public static AuroraLightsyncSettings LightsyncConfig { get; private set; } = new();

    protected override async Task Initialize()
    {
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        LightsyncConfig = await ConfigManager.LoadLightsyncConfig();

        if (!Global.Configuration.EnableLightsyncTakeover)
        {
            Global.logger.Information("Lightsync initialization skipped because of user setting");
            LogitechSdkListener.Dispose();
            return;
        }

        Global.logger.Information("Initializing Lightsync...");
        await LogitechSdkListener.Initialize(runningProcessMonitor, LightsyncConfig);
        Global.logger.Information("Initialized Lightsync");
    }

    private async void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.EnableLightsyncTakeover))
        {
            return;
        }

        if (Global.Configuration.EnableLightsyncTakeover)
        {
            await LogitechSdkListener.Initialize(runningProcessMonitor, LightsyncConfig);
        }
        else
        {
            LogitechSdkListener.Dispose();
        }
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.HardwareMonitor;
using AuroraRgb.Nodes;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules;

public sealed class HardwareMonitorModule : AuroraModule
{
    
    protected override Task Initialize()
    {
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        if (Global.Configuration.EnableHardwareInfo)
        {
            LocalPcInformation.HardwareMonitor = new HardwareMonitor.HardwareMonitor();
        }

        return Task.CompletedTask;
    }

    private static void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.EnableHardwareInfo))
        {
            return;
        }

        LocalPcInformation.HardwareMonitor.Dispose();
        if (Global.Configuration.EnableHardwareInfo)
        {
            LocalPcInformation.HardwareMonitor = new HardwareMonitor.HardwareMonitor();
        }
        else
        {
            LocalPcInformation.HardwareMonitor = new NoopHardwareMonitor();
        }
    }


    public override ValueTask DisposeAsync()
    {
        LocalPcInformation.HardwareMonitor.Dispose();
        LocalPcInformation.HardwareMonitor = new NoopHardwareMonitor();

        return ValueTask.CompletedTask;
    }
}
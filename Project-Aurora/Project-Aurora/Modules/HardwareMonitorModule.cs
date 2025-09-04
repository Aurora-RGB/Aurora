using System.ComponentModel;
using System.Threading.Tasks;
using AuroraRgb.Modules.HardwareMonitor;
using AuroraRgb.Nodes;
using AuroraRgb.Utils;
using LibreHardwareMonitor.Hardware;

namespace AuroraRgb.Modules;

public sealed class HardwareMonitorModule : AuroraModule
{
    protected override Task Initialize()
    {
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        if (Global.Configuration.EnableHardwareInfo && Global.Configuration.EnableWinRing0Monitor)
        {
            LocalPcInformation.HardwareMonitor = new HardwareMonitor.HardwareMonitor();
        }

        if (!Global.Configuration.EnableAmdCpuMonitor)
        {
            UnsecureDrivers.DeleteDriver(UnsecureDrivers.InpOutDriverName, true);
        }

        if (!Global.Configuration.EnableWinRing0Monitor)
        {
            UnsecureDrivers.DeleteDriver(UnsecureDrivers.WinRing0DriverName, true);
        }

        return Task.CompletedTask;
    }

    private static void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.EnableHardwareInfo) && e.PropertyName != nameof(Global.Configuration.EnableWinRing0Monitor))
        {
            return;
        }

        LocalPcInformation.HardwareMonitor.Dispose();
        if (Global.Configuration.EnableHardwareInfo && Global.Configuration.EnableWinRing0Monitor)
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

    private static void DeleteDrivers()
    {
        var computer = new Computer
        {
            IsCpuEnabled = false,
            IsGpuEnabled = false,
            IsMemoryEnabled = false,
            IsNetworkEnabled = false,
        };
        computer.Close();
    }
}
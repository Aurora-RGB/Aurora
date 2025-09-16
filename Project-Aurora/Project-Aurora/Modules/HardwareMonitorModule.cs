using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.HardwareMonitor;
using AuroraRgb.Nodes;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules;

public sealed class HardwareMonitorModule : AuroraModule
{
    private const string PromptMessage = "Would you like Aurora to automatically uninstall unsecure drivers?\n(inputx64 and WinRing0)";
    
    protected override Task Initialize()
    {
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        if (Global.Configuration.EnableHardwareInfo)
        {
            LocalPcInformation.HardwareMonitor = new HardwareMonitor.HardwareMonitor();
        }

        if (Global.Configuration.AutoRemoveUnsecureDrivers is null)
        {
            var result = MessageBox.Show(PromptMessage, "AuroraRgb", MessageBoxButton.YesNo);
            Global.Configuration.AutoRemoveUnsecureDrivers = result switch
            {
                MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => true,
            };
        }

        if(Global.Configuration.AutoRemoveUnsecureDrivers ?? true)
        {
            UnsecureDrivers.DeleteDriver(UnsecureDrivers.InpOutDriverName, true);
            if (!Global.Configuration.EnableWinRing0Monitor)
            {
                UnsecureDrivers.DeleteDriver(UnsecureDrivers.WinRing0DriverName, true);
            }
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
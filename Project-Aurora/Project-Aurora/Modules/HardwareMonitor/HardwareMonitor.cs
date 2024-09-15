using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace AuroraRgb.Modules.HardwareMonitor;

public interface IHardwareMonitor : IDisposable
{
    HardwareMonitor.GpuUpdater Gpu { get; }
    HardwareMonitor.CpuUpdater Cpu { get; }
    HardwareMonitor.RamUpdater Ram { get; }
    HardwareMonitor.NetUpdater Net { get; }
    
    List<string> NetworkAdapters { get; }
}

public sealed class NoopHardwareMonitor : IHardwareMonitor
{
    private readonly Lazy<HardwareMonitor.GpuUpdater> _gpu = new(() => new ([]));
    private readonly Lazy<HardwareMonitor.CpuUpdater> _cpu = new(() => new ([]));
    private readonly Lazy<HardwareMonitor.NetUpdater> _net = new(() => new ([]));
    private readonly Lazy<HardwareMonitor.RamUpdater> _ram = new(() => new ([]));

    public HardwareMonitor.GpuUpdater Gpu => _gpu.Value;

    public HardwareMonitor.CpuUpdater Cpu => _cpu.Value;

    public HardwareMonitor.RamUpdater Ram => _ram.Value;

    public HardwareMonitor.NetUpdater Net => _net.Value;

    public List<string> NetworkAdapters => [];

    public void Dispose()
    {
    }
}

public sealed partial class HardwareMonitor: IHardwareMonitor
{
    private static readonly IEnumerable<IHardware> Hardware;

    private static readonly Lazy<GpuUpdater> _gpu = new(() => new GpuUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public GpuUpdater Gpu => _gpu.Value;

    private static readonly Lazy<CpuUpdater> _cpu = new(() => new CpuUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public CpuUpdater Cpu => _cpu.Value;

    private static readonly Lazy<RamUpdater> _ram = new(() => new RamUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    public RamUpdater Ram => _ram.Value;

    private static readonly Lazy<NetUpdater> _net = new(() => new NetUpdater(Hardware), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Computer _computer;
    public NetUpdater Net => _net.Value;

    public List<string> NetworkAdapters => Net.GetNetworkDevices().Select(n => n.Name).ToList();

#pragma warning disable CA1810 // Initialize reference type static fields inline
    static HardwareMonitor()
#pragma warning restore CA1810 // Initialize reference type static fields inline
    {
        var isAmd = CpuBrandFinder.IsAmd();
        var isCpuEnabled = !isAmd || Global.Configuration.EnableAmdCpuMonitor;
        _computer = new Computer
        {
            IsCpuEnabled = isCpuEnabled,
            IsGpuEnabled = isCpuEnabled,
            IsMemoryEnabled = true,
            IsNetworkEnabled = true
        };
        try
        {
            _computer.Open();
            Hardware = _computer.Hardware;
        }
        catch (Exception e)
        {
            Global.logger.Error("Error instantiating hardware monitor:", e);
        }
    }

    public static bool TryDump()
    {
        var lines = new List<string>();
        foreach (var hw in Hardware)
        {
            lines.Add("-----");
            lines.Add(hw.Name);
            lines.Add("Sensors:");
            lines.AddRange(
                hw.Sensors.OrderBy(s => s.Identifier)
                    .Select(sensor => $"Name: {sensor.Name}, Id: {sensor.Identifier}, Type: {sensor.SensorType}"));
            lines.Add("-----");
        }
        try
        {
            File.WriteAllLines(Path.Combine(Global.LogsDirectory, "sensors.txt"), lines);
            return true;
        }
        catch (IOException e)
        {
            Global.logger.Error(e, "Failed to write sensors dump");
            return false;
        }
    }

    public void Dispose()
    {
        _computer.Close();
    }
}
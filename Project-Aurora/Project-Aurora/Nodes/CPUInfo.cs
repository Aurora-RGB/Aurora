namespace AuroraRgb.Nodes;

public class CPUInfo : Node
{
    /// <summary>
    /// Represents the CPU usage from 0 to 100
    /// </summary>
    public static float Usage => LocalPcInformation.HardwareMonitor.Cpu.CpuLoad;

    /// <summary>
    /// Represents the temperature of the cpu die in celsius
    /// </summary>
    public static float Temperature => LocalPcInformation.HardwareMonitor.Cpu.CpuTemp;

    /// <summary>
    /// Represents the CPU power draw in watts
    /// </summary>
    public static float PowerUsage => LocalPcInformation.HardwareMonitor.Cpu.CpuPower;
}
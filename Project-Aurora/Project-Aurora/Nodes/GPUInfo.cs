namespace AuroraRgb.Nodes;

public class GPUInfo : Node
{
    public static float Usage => LocalPcInformation.HardwareMonitor.Gpu.GpuLoad;
    public static float Temperature => LocalPcInformation.HardwareMonitor.Gpu.GpuCoreTemp;
    public static float PowerUsage => LocalPcInformation.HardwareMonitor.Gpu.GpuPower;
    public static float FanRPM => LocalPcInformation.HardwareMonitor.Gpu.GpuFan;
}
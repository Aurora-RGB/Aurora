using System.ComponentModel;

namespace AuroraRgb.Settings;

public enum CpuAffinityMode
{
    [Description("Windows default (all processors)")]
    WindowsDefault,
    [Description("Automatic (prefer efficiency cores)")]
    Automatic,
    [Description("Efficiency cores only")]
    EfficiencyCores,
    [Description("Physical cores (one thread per core)")]
    PhysicalCores,
    [Description("Performance cores only")]
    PerformanceCores,
    [Description("Manual processor selection")]
    Manual,
}

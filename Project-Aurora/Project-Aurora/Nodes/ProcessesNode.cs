using AuroraRgb.Modules;

namespace AuroraRgb.Nodes;

public class ProcessesNode : Node
{
    public static readonly ProcessesNode Instance = new();
    /// <summary>
    /// Returns focused window's name.
    /// </summary>
    public static string ActiveWindowName => ProcessesModule.ActiveProcessMonitor.Result.ProcessTitle;

    /// <summary>
    /// Returns focused window's process name.
    /// </summary>
    public static string ActiveProcess => ProcessesModule.ActiveProcessMonitor.Result.ProcessName;

    private ProcessesNode()
    {}
}
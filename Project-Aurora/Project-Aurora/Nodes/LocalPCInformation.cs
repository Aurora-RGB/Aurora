using System.Collections.Generic;
using AuroraRgb.Modules;
using AuroraRgb.Modules.HardwareMonitor;
using AuroraRgb.Profiles;

namespace AuroraRgb.Nodes;

/// <summary>
/// Class representing local computer information
/// </summary>
public class LocalPcInformation : Node
{
    [GameStateIgnore]
    public static IHardwareMonitor HardwareMonitor { get; set; } = new NoopHardwareMonitor();

    private TimeNode? _time;
    public TimeNode Time => _time ??= new TimeNode();

    private AudioNode? _audio;
    public AudioNode Audio => _audio ??= new AudioNode();

    private static CPUInfo? _cpuInfo;
    public static CPUInfo CPU => _cpuInfo ??= new CPUInfo();

    private static RAMInfo? _ramInfo;
    public static RAMInfo RAM => _ramInfo ??= new RAMInfo();

    private static GPUInfo? _gpuInfo;
    public static GPUInfo GPU => _gpuInfo ??= new GPUInfo();

    private static NETInfo? _netInfo;
    public static NETInfo NET => _netInfo ??= new NETInfo();

    public Controllers Controllers { get; } = new();
    public RazerDevices RazerDevices { get; } = new();

    #region Cursor Position

    private static CursorPositionNode? _cursorPosition;
    public static CursorPositionNode CursorPosition => _cursorPosition ??= new CursorPositionNode();

    #endregion

    #region Battery Properties

    private static BatteryNode? _battery;
    public static BatteryNode Battery => _battery ??= new BatteryNode();

    #endregion

    #region Media Properties

    private static MediaNode? _media;
    public static MediaNode Media => _media ??= new MediaNode();

    #endregion

    /// <summary>
    /// Returns focused window's name.
    /// </summary>
    public static string ActiveWindowName => ProcessesModule.ActiveProcessMonitor.Result.ProcessTitle;

    /// <summary>
    /// Returns focused window's process name.
    /// </summary>
    public static string ActiveProcess => ProcessesModule.ActiveProcessMonitor.Result.ProcessName;

    private CelestialData? _celestialData;
    public CelestialData CelestialData => _celestialData ??= new CelestialData();

    private DesktopNode? _desktop;
    public DesktopNode Desktop => _desktop ??= new DesktopNode();

    public static List<string> NetworkAdapters => HardwareMonitor.NetworkAdapters;
}
﻿using Aurora.Modules.HardwareMonitor;
using Aurora.Modules.ProcessMonitor;

namespace Aurora.Nodes;

/// <summary>
/// Class representing local computer information
/// </summary>
public class LocalPcInformation : Node
{
    public static IHardwareMonitor HardwareMonitor { get; set; } = new NoopHardwareMonitor();

    private TimeNode? _time;
    public TimeNode Time => _time ??= new TimeNode();

    private AudioNode? _audio;
    public AudioNode Audio => _audio ??= new AudioNode();

    private static CPUInfo? _cpuInfo;
    public CPUInfo CPU => _cpuInfo ??= new CPUInfo();

    private static RAMInfo? _ramInfo;
    public RAMInfo RAM => _ramInfo ??= new RAMInfo();

    private static GPUInfo? _gpuInfo;
    public GPUInfo GPU => _gpuInfo ??= new GPUInfo();

    private static NETInfo? _netInfo;
    public NETInfo NET => _netInfo ??= new NETInfo();

    #region Cursor Position

    private static CursorPositionNode? _cursorPosition;
    public CursorPositionNode CursorPosition => _cursorPosition ??= new CursorPositionNode();

    #endregion

    #region Battery Properties

    private static BatteryNode? _battery;
    public BatteryNode Battery => _battery ??= new BatteryNode();

    #endregion

    #region Media Properties

    private static MediaNode? _media;
    public MediaNode Media => _media ??= new MediaNode();

    #endregion

    /// <summary>
    /// Returns focused window's name.
    /// </summary>
    public string ActiveWindowName => ActiveProcessMonitor.Instance.ProcessTitle;

    /// <summary>
    /// Returns focused window's process name.
    /// </summary>
    public string ActiveProcess => ActiveProcessMonitor.Instance.ProcessName;

    private static CelestialData? _celestialData;
    public CelestialData CelestialData => _celestialData ??= new CelestialData();

    private DesktopNode? _desktop;
    public DesktopNode Desktop => _desktop ??= new DesktopNode();
}
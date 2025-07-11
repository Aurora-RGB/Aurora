﻿using System.Collections.Generic;
using AuroraRgb.Modules.HardwareMonitor;
using AuroraRgb.Profiles;

namespace AuroraRgb.Nodes;

// info by LibreHardwareMonitor
/// <summary>
/// Class representing local computer information
/// </summary>
public class LocalPcInformation : Node
{
    [GameStateIgnore]
    public static IHardwareMonitor HardwareMonitor { get; set; } = new NoopHardwareMonitor();

    private TimeNode? _time;
    public TimeNode Time => _time ??= new TimeNode();

    private static CPUInfo? _cpuInfo;
    public static CPUInfo CPU => _cpuInfo ??= new CPUInfo();

    private static RAMInfo? _ramInfo;
    public static RAMInfo RAM => _ramInfo ??= new RAMInfo();

    private static GPUInfo? _gpuInfo;
    public static GPUInfo GPU => _gpuInfo ??= new GPUInfo();

    private static NETInfo? _netInfo;
    public static NETInfo NET => _netInfo ??= new NETInfo();

    private static BatteryNode? _battery;
    public static BatteryNode Battery => _battery ??= new BatteryNode();

    public static List<string> NetworkAdapters => HardwareMonitor.NetworkAdapters;
}
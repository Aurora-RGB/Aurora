using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AuroraRgb.Settings;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules;

public sealed partial class PerformanceModeModule : AuroraModule
{
    public static string AffinityStatus { get; private set; } = "CPU scheduling has not been initialized.";
    public static event EventHandler? AffinityChanged;

    protected override Task Initialize()
    {
        Global.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        UpdatePriority();
        UpdateAffinity();
        return Task.CompletedTask;
    }

    public static void UpdatePriority()
    {
        try
        {
            using var currentProcess = Process.GetCurrentProcess();
            var efficiency = Global.Configuration.ProcessPriority == AuroraProcessPriority.EfficiencyMode;
            currentProcess.PriorityClass = Global.Configuration.ProcessPriority switch
            {
                AuroraProcessPriority.High => ProcessPriorityClass.High,
                AuroraProcessPriority.EfficiencyMode => ProcessPriorityClass.Idle,
                _ => ProcessPriorityClass.Normal,
            };
            SetEcoQoS(currentProcess, efficiency);
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            Global.logger.Warning(exception, "Unable to change Aurora process priority");
        }
    }

    private static void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Configuration.ProcessPriority))
            UpdatePriority();
        if (e.PropertyName is nameof(Configuration.CpuAffinityMode) or nameof(Configuration.ManualCpuAffinity))
            UpdateAffinity();
    }

    public static void UpdateAffinity()
    {
        try
        {
            var mode = Global.Configuration.CpuAffinityMode;
            var topology = mode == CpuAffinityMode.WindowsDefault ? [] : CpuSetAffinity.GetCpuSets();
            var selected = CpuSetAffinity.Select(topology, mode, Global.Configuration.ManualCpuAffinity);
            CpuSetAffinity.Apply(selected);
            AffinityStatus = selected.Length > 0
                ? $"Applied to Aurora: {selected.Length} logical processors. Changes take effect immediately."
                : mode switch
                {
                    CpuAffinityMode.WindowsDefault => "Windows manages all processors for Aurora.",
                    CpuAffinityMode.Manual => "No selected processors are available. Windows manages placement; select processors below.",
                    _ => "No distinct core classes or matching processors found. Windows manages placement.",
                };
        }
        catch (Exception exception) when (exception is Win32Exception or EntryPointNotFoundException or
                                           DllNotFoundException or System.IO.InvalidDataException or OverflowException)
        {
            try
            {
                CpuSetAffinity.Apply([]);
                AffinityStatus = "CPU selection unavailable. Windows manages placement.";
            }
            catch (Exception resetException) when (resetException is Win32Exception or EntryPointNotFoundException or DllNotFoundException)
            {
                AffinityStatus = "Windows could not change CPU selection. The previous selection may still be active.";
                Global.logger.Warning(resetException, "Unable to clear Aurora CPU Sets");
            }
            Global.logger.Warning(exception, "Unable to apply Aurora CPU Sets");
        }
        Global.logger.Information("CPU scheduling: {Status}", AffinityStatus);
        AffinityChanged?.Invoke(null, EventArgs.Empty);
    }

    public override ValueTask DisposeAsync()
    {
        Global.Configuration.PropertyChanged -= ConfigurationOnPropertyChanged;
        return ValueTask.CompletedTask;
    }

    private static void SetEcoQoS(Process currentProcess, bool enabled)
    {
        var state = new ProcessPowerThrottlingState
        {
            Version = 1,
            // EXECUTION_SPEED = 1. Zero masks restore Windows heuristics when leaving Efficiency Mode.
            // Leave timer resolution under Windows control so RGB timing remains usable.
            ControlMask = enabled ? 1u : 0u,
            StateMask = enabled ? 1u : 0u,
        };
        if (!SetProcessInformation(currentProcess.Handle, ProcessPowerThrottling, ref state, (uint)Marshal.SizeOf(state)))
            Global.logger.Warning("Failed to update EcoQoS: {Error}", Marshal.GetLastPInvokeError());
    }

    private const int ProcessPowerThrottling = 4;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessInformation(IntPtr process, int informationClass,
        ref ProcessPowerThrottlingState information, uint informationSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessPowerThrottlingState
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }
}

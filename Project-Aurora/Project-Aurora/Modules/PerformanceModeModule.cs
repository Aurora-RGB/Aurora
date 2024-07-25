using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules;

public sealed partial class PerformanceModeModule : AuroraModule
{
    protected override Task Initialize()
    {
        UpdatePriority();

        return Task.CompletedTask;
    }

    public static void UpdatePriority()
    {
        var currentProcess = Process.GetCurrentProcess();
        switch (Global.Configuration.ProcessPriority)
        {
            case AuroraProcessPriority.High:
                currentProcess.PriorityClass = ProcessPriorityClass.High;
                break;
            case AuroraProcessPriority.EfficiencyMode:
                currentProcess.PriorityClass = ProcessPriorityClass.Idle;
                EnableEcoQoS(currentProcess);
                break;
            default:
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
                break;
        }
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
    
    private static void EnableEcoQoS(Process currentProcess)
    {
        var ecoQoS = new ProcessPowerThrottlingState
        {
            Version = ProcessPowerThrottlingCurrentVersion,
            ControlMask = ProcessPowerThrottlingExecutionStateEcoQos,
            StateMask = ProcessPowerThrottlingExecutionStateEcoQos
        };

        var success = SetProcessInformation(currentProcess.Handle, ProcessPowerThrottling, ref ecoQoS, (uint)Marshal.SizeOf(ecoQoS));
        if (!success)
        {
            Global.logger.Error("Failed to enable EcoQoS.");
        }
    }

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessInformation(IntPtr hProcess, int processInformationClass, ref ProcessPowerThrottlingState processInformation, uint processInformationSize);

    private const int ProcessPowerThrottling = 9; // PROCESS_INFORMATION_CLASS value for ProcessPowerThrottling

    private const uint ProcessPowerThrottlingCurrentVersion = 1;
    private const uint ProcessPowerThrottlingExecutionStateEcoQos = 0x00000008;

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessPowerThrottlingState
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }
}
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Modules.Razer;
using ByteSizeLib;

namespace AuroraRgb.Modules;

public sealed class PerformanceMonitor(Task<RunningProcessMonitor> runningProcessMonitor) : AuroraModule
{
    private const string RzChromaStreamStartProcessName = "rzchromastreamserver.exe";
    private const string RazerChromeServerProcessName = "RzChromaStreamServer";

    private static readonly bool EnableChromaMonitor = false;

    private readonly SmartThreadPool _threadPool = new(new STPStartInfo
    {
        AreThreadsBackground = true,
        ThreadPriority = ThreadPriority.BelowNormal,
        IdleTimeout = 1000,
    })
    {
        Name = "Performance Monitor Thread",
        Concurrency = 1,
        MaxThreads = 1,
    };

    private readonly TaskCompletionSource _endTrigger = new();
    private bool _working;

    private PerformanceCounter? _auroraMemCounter;
    private PerformanceCounter? _rzStreamCpuCounter;

    protected override Task Initialize()
    {
        Task.Run(BackgroundInitialize);

        GC.AddMemoryPressure((long)ByteSize.FromMegaBytes(666).Bytes);
        
        return Task.CompletedTask;
    }

    private async Task BackgroundInitialize()
    {
        InitializeAurora();
        if (EnableChromaMonitor)
        {
            await InitializeRazerStreamApi();
        }

        _working = true;
        _threadPool.QueueWorkItem((Action)MonitorPerformance);
        return;

        void MonitorPerformance()
        {
            CheckAuroraMemory();
            CheckRazerStreamApi();

            Task.WaitAny(Task.Delay(TimeSpan.FromSeconds(20)), _endTrigger.Task);

            if (_working)
            {
                _threadPool.QueueWorkItem((Action)MonitorPerformance);
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        (await runningProcessMonitor).ProcessStarted -= ProcessMonitorOnProcessStarted;
        (await runningProcessMonitor).ProcessStopped -= ProcessMonitorOnProcessStopped;

        _working = false;
        _endTrigger.SetResult();
        _threadPool.Join();
    }

    private void InitializeAurora()
    {
        _auroraMemCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);
    }

    private void CheckAuroraMemory()
    {
        var currentRam = _auroraMemCounter?.NextValue() / 1024 / 1024;
        if (!(currentRam > 3500)) return; // around 2,5GB limit
        Global.logger.Fatal("Aurora memory limit exceeded! Closing Aurora to preserve system stability");
        Environment.Exit(10);
    }

    private async Task InitializeRazerStreamApi()
    {
        if ((await runningProcessMonitor).IsProcessRunning(RzChromaStreamStartProcessName))
        {
            _rzStreamCpuCounter = new PerformanceCounter("Process", "% Processor Time", RazerChromeServerProcessName, true);
        }
        (await runningProcessMonitor).ProcessStarted += ProcessMonitorOnProcessStarted;
        (await runningProcessMonitor).ProcessStopped += ProcessMonitorOnProcessStopped;
    }

    private void ProcessMonitorOnProcessStarted(object? sender, ProcessStarted e)
    {
        if (e.ProcessName != RazerChromeServerProcessName)
        {
            return;
        }

        _rzStreamCpuCounter = new PerformanceCounter("Process", "% Processor Time", RazerChromeServerProcessName, true);
    }

    private void ProcessMonitorOnProcessStopped(object? sender, ProcessStopped e)
    {
        if (e.ProcessName != RazerChromeServerProcessName)
        {
            return;
        }

        _rzStreamCpuCounter?.Dispose();
        _rzStreamCpuCounter = null;
    }

    private void CheckRazerStreamApi()
    {
        if (_rzStreamCpuCounter == null)
        {
            return;
        }

        try
        {
            var cpuUsage = _rzStreamCpuCounter.NextValue();
            if (cpuUsage > 80)
            {
                ChromaInstallationUtils.DisableChromaBloat();
            }
        }
        catch { /* ignore */ }
    }
}
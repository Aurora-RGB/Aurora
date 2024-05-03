using System;
using System.Threading.Tasks;
using AuroraRgb.Modules.ProcessMonitor;
using RazerSdkReader;

namespace AuroraRgb.Modules.Razer;

public class ChromaSdkStateChangedEventArgs(ChromaReader? chromaReader) : EventArgs
{
    public ChromaReader? ChromaReader => chromaReader;
}

public sealed class ChromaSdkManager : IDisposable
{
    private const string RzServiceProcessName = "rzsdkservice.exe";

    public event EventHandler<ChromaSdkStateChangedEventArgs>? StateChanged;

    public ChromaReader? ChromaReader { get; private set; }

    internal async Task Initialize()
    {
        try
        {
            var chromaReader = TryLoadChroma();
            ChromaReader = chromaReader;
            StateChanged?.Invoke(this, new ChromaSdkStateChangedEventArgs(ChromaReader));
        }
        catch (Exception exc)
        {
            Global.logger.Fatal(exc, "RazerSdkManager failed to load!");
        }

        var runningProcessMonitor = await ProcessesModule.RunningProcessMonitor;
        runningProcessMonitor.ProcessStarted += RunningProcessMonitorOnProcessStarted;
        runningProcessMonitor.ProcessStopped += RunningProcessMonitorOnProcessStopped;
    }

    private void RunningProcessMonitorOnProcessStarted(object? sender, ProcessStarted e)
    {
        if (e.ProcessName != RzServiceProcessName)
        {
            return;
        }

        Global.logger.Information("Chroma service opened. Enabling Chroma integration...");

        var chromaReader = TryLoadChroma();
        ChromaReader = chromaReader;
        StateChanged?.Invoke(this, new ChromaSdkStateChangedEventArgs(ChromaReader));
    }

    private void RunningProcessMonitorOnProcessStopped(object? sender, ProcessStopped e)
    {
        if (e.ProcessName != RzServiceProcessName)
        {
            return;
        }

        if (ChromaReader == null)
        {
            return;
        }

        Global.logger.Information("Chroma service is closed. Disabling Chroma integration...");

        ChromaReader.Dispose();
        ChromaReader = null;
        StateChanged?.Invoke(this, new ChromaSdkStateChangedEventArgs(null));
    }

    private static ChromaReader TryLoadChroma()
    {
        var chromaReader = new ChromaReader();
        chromaReader.Exception += RazerSdkReaderOnException;
        RzHelper.Initialize(chromaReader);

        chromaReader.Start();
        return chromaReader;
    }

    private static void RazerSdkReaderOnException(object? sender, RazerSdkReaderException e)
    {
        Global.logger.Error(e, "Chroma Reader Error");
    }

    public void Dispose()
    {
        if (ChromaReader == null)
        {
            return;
        }

        ChromaReader.Dispose();
        ChromaReader = null;
    }
}
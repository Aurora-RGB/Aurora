using System.Diagnostics;

namespace AuroraDeviceManager.Utils;

public sealed class PerformanceMeasurer : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public void Measure(string measuredMethod)
    {
        Global.Logger.Information("{Method} took {Elapsed}", measuredMethod, _stopwatch.Elapsed);
        _stopwatch.Restart();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
    }
}
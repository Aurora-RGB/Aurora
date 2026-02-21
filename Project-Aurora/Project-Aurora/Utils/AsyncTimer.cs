namespace AuroraRgb.Utils;

using System;
using System.Threading;
using System.Threading.Tasks;

public sealed class AsyncTimer : IAsyncDisposable
{
    private readonly Func<CancellationToken, Task> _callback;
    private readonly TimeSpan _period;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task? _runningTask;

    public AsyncTimer(Func<CancellationToken, Task> callback, TimeSpan period)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _period = period;

        // Start the timer loop
        _runningTask = TimerLoopAsync(_cts.Token);
    }

    private async Task TimerLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_period, token);
                try
                {
                    await _callback(token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // Timer was canceled, exit gracefully
                }
                catch (Exception ex)
                {
                    // Log or handle callback exceptions
                    Global.logger.Warning(ex, "Exception while executing callback");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timer canceled
        }
    }

    public ValueTask DisposeAsync()
    {
        // don't block since this will be called from timer callback itself
        _ = _cts.CancelAsync();
        _cts.Dispose();
        return ValueTask.CompletedTask;
    }
}

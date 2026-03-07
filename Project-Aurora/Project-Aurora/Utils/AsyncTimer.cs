namespace AuroraRgb.Utils;

using System;
using System.Threading;
using System.Threading.Tasks;

public sealed class AsyncTimer : IAsyncDisposable
{
    private readonly Func<CancellationToken, Task> _callback;
    private readonly TimeSpan _period;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _runningTask;

    public AsyncTimer(Func<CancellationToken, Task> callback, TimeSpan period)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _period = period;

        // Start the timer loop
        _runningTask = Task.Run(TimerLoopAsync);
    }

    private async Task TimerLoopAsync()
    {
        var cancelToken = _cts.Token;
        try
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await Task.Delay(_period, cancelToken);
                // run in a separate stack
                await Task.Run(async () =>
                {
                    try
                    {
                        await _callback(_cts.Token);
                    }
                    catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
                    {
                        // Timer was canceled, exit gracefully
                    }
                    catch (Exception ex)
                    {
                        // Log or handle callback exceptions
                        Global.logger.Warning(ex, "Exception while executing callback");
                    }
                }, cancelToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Timer canceled
        }
    }

    public async ValueTask DisposeAsync()
    {
        // don't block since this will be called from timer callback itself
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}

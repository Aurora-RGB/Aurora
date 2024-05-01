using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Modules.ProcessMonitor;

namespace AuroraRgb.Modules;

public sealed class ProcessesModule : AuroraModule
{
    public static Task<ActiveProcessMonitor> ActiveProcessMonitor => ActiveProcess.Task;
    public static Task<RunningProcessMonitor> RunningProcessMonitor => RunningProcess.Task;

    private static readonly TaskCompletionSource<ActiveProcessMonitor> ActiveProcess = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static readonly TaskCompletionSource<RunningProcessMonitor> RunningProcess = new(TaskCreationOptions.RunContinuationsAsynchronously);
    
    protected override async Task Initialize()
    {
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ActiveProcess.SetResult(new ActiveProcessMonitor());
            RunningProcess.SetResult(new RunningProcessMonitor());
        }, DispatcherPriority.Send);
    }

    public override async ValueTask DisposeAsync()
    {
        if (ActiveProcessMonitor.IsCompletedSuccessfully)
        {
            (await ActiveProcessMonitor).Dispose();
        }
        if (RunningProcessMonitor.IsCompletedSuccessfully)
        {
            (await ActiveProcessMonitor).Dispose();
        }
    }
}
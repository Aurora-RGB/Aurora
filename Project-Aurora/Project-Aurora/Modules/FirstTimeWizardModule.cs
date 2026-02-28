using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Controls;

namespace AuroraRgb.Modules;

public sealed class FirstTimeWizardModule : AuroraModule
{
    public Task<bool> AutoGsiValueTask { get; }
    
    private readonly TaskCompletionSource<bool> _tcs = new ();

    public FirstTimeWizardModule()
    {
        AutoGsiValueTask = _tcs.Task;
    }

    protected override async Task Initialize()
    {
        var autoGsiSetting = Global.Configuration.AutoInstallGsi;
        if (autoGsiSetting != null)
        {
            _tcs.SetResult(autoGsiSetting.Value);
            return;
        }
        
        var windowClosedTaskSource = new TaskCompletionSource();
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var firstTimeWindow = new WindowFirstTime();
            firstTimeWindow.Closed += (_, _) => windowClosedTaskSource.TrySetResult();
            firstTimeWindow.Show();
        }, DispatcherPriority.Normal);
        
        await windowClosedTaskSource.Task;

        _tcs.TrySetResult(Global.Configuration.AutoInstallGsi ?? false);
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
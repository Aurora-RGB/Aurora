using System;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.GameStateListen;

namespace AuroraRgb.Modules;

public sealed class IpcListenerModule : AuroraModule
{
    private readonly TaskCompletionSource<IpcListener?> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IpcListener? _listener;

    public Task<IpcListener?> IpcListener => _taskSource.Task;

    protected override Task Initialize()
    {
        if (!Global.Configuration.EnableIpcListener)
        {
            Global.logger.Information("IpcListener is disabled");
            _taskSource.SetResult(null);
            return Task.CompletedTask;
        }
        Global.logger.Information("Starting IpcListener");
        try
        {
            _listener = new IpcListener();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "IpcListener Exception");
            MessageBox.Show("IpcListener Exception.\r\n" +
                            "Ipc pipe could not be created. Wrapper integrations won't work." +
                            "\r\n" + exc);
            _taskSource.SetResult(null);
            return Task.CompletedTask;
        }

        if (!_listener.Start())
        {
            Global.logger.Error("IpcListener could not start");
            MessageBox.Show("IpcListener could not start. Try running this program as Administrator.\r\n" +
                            "Wrapper integrations won't work.");
            _taskSource.SetResult(null);
            return Task.CompletedTask;
        }

        Global.logger.Information("Listening for wrapper integration calls...");
        _taskSource.SetResult(_listener);
        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        if (_listener != null)
            await _listener.Stop();
    }
}
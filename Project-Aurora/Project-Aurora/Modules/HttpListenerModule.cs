using System;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.GameStateListen;

namespace AuroraRgb.Modules;

public sealed class HttpListenerModule : AuroraModule
{
    private readonly TaskCompletionSource<AuroraHttpListener?> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private AuroraHttpListener? _listener;

    public Task<AuroraHttpListener?> HttpListener => _taskSource.Task;

    protected override Task Initialize()
    {
        _taskSource.SetResult(DoInitialize());
        return Task.CompletedTask;
    }

    private AuroraHttpListener? DoInitialize()
    {
        if (!Global.Configuration.EnableHttpListener)
        {
            Global.logger.Information("HttpListener is disabled");
            return null;
        }
        try
        {
            _listener = new AuroraHttpListener(9088);

            if (_listener.Start()) return _listener;

            Global.logger.Error("GameStateListener could not start");
            MessageBox.Show("HttpListener could not start. Try running this program as Administrator.\r\n" +
                            "Http socket could not be created. Games using this integration won't work");
            return null;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "HttpListener Exception");
            MessageBox.Show("HttpListener Exception.\r\n" +
                            "Http socket could not be created. Games using this integration won't work" +
                            "\r\n" + exc);
            return _listener;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_listener != null)
            await _listener.Stop();
    }
}
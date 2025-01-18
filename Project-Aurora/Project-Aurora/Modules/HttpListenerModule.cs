using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
            var ips = GetListenIps();
            _listener = new AuroraHttpListener(9088, ips);

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

    private static IEnumerable<string> GetListenIps()
    {
        var enabledInterfaceNames = Global.Configuration.HttpListenInterfaceNames;
        return NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => enabledInterfaceNames.Contains(netInterface.Name))
            .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
            .Where(InterIp)
            .Select(GetIp);
    }

    private static bool InterIp(UnicastIPAddressInformation arg)
    {
        return arg.Address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6;
    }

    private static string GetIp(UnicastIPAddressInformation ip)
    {
        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return $"[{ip.Address}]";
        }
        return ip.Address.ToString();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_listener != null)
            await _listener.Stop();
    }
}
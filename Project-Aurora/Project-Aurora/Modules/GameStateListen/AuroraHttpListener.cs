using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.GameStateListen.Http;
using AuroraRgb.Profiles;
using Common.Utils;

namespace AuroraRgb.Modules.GameStateListen;

public sealed class JsonGameStateEventArgs(string gameId, string json) : EventArgs
{
    public string GameId { get; } = gameId;
    public string Json { get; } = json;
}

public sealed class AuroraHttpListener
{
    private static readonly WebHeaderCollection WebHeaderCollection = new()
    {
        ["Access-Control-Allow-Origin"] = "*",
        ["Access-Control-Allow-Private-Network"] = "true",
    };

    private bool _isRunning;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    private readonly HttpListener _netListener;
    private readonly int _port;
    private readonly SingleConcurrentThread _readThread;
    private readonly SingleConcurrentThread _readThread2;
    
    private readonly FrozenDictionary<string, AuroraEndpoint> _endpoints;
    private readonly FrozenDictionary<Regex, AuroraRegexEndpoint> _regexEndpoints;
    private readonly HashSet<string> _allowedMethods;

    public IGameState CurrentGameState
    {
        get;
        internal set
        {
            field = value;
            NewGameState?.Invoke(this, value);
        }
    } = new NewtonsoftGameState("{}");

    /// <summary>
    ///  Event for handing a newly received game state
    /// </summary>
    public event EventHandler<IGameState>? NewGameState;

    public event EventHandler<JsonGameStateEventArgs>? NewJsonGameState;

    /// <summary>
    /// A GameStateListener that listens for connections on http://127.0.0.1:port/
    /// </summary>
    public AuroraHttpListener(int port, IEnumerable<string> listenIps)
    {
        _port = port;
        _netListener = new HttpListener();
        _netListener.Prefixes.Add($"http://127.0.0.1:{port}/");
        foreach (var listenIp in listenIps)
        {
            _netListener.Prefixes.Add($"http://{listenIp}:{port}/");
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _readThread = new SingleConcurrentThread("Http Read Thread 1", AsyncRead1, ExceptionCallback);
        _readThread2 = new SingleConcurrentThread("Http Read Thread 2", AsyncRead2, ExceptionCallback);

        _endpoints = HttpEndpointFactory.CreateEndpoints(this);
        _regexEndpoints = HttpEndpointFactory.CreateRegexEndpoints(this);
        _allowedMethods = _endpoints.Values
            .Union<IAuroraEndpoint>(_regexEndpoints.Values)
            .SelectMany(endpoint => endpoint.AvailableMethods)
            .ToHashSet();
    }

    private static void ExceptionCallback(object? sender, SingleThreadExceptionEventArgs eventArgs)
    {
        Global.logger.Error(eventArgs.Exception, "Error reading http response");
    }

    /// <summary>
    /// Starts listening for GameState requests
    /// </summary>
    public bool Start()
    {
        ServicePointManager.UseNagleAlgorithm = false;
        if (_isRunning) return false;

        try
        {
            _netListener.Start();
        }
        catch (HttpListenerException exc)
        {
            Global.logger.Error(exc, "Could not start HttpListener");

            if (exc.ErrorCode == 5) //Access Denied
                MessageBox.Show("Access error during start of network listener.\r\n\r\n" +
                                "To fix this issue, please run the following commands as admin in Command Prompt:r\n" +
                                $"   netsh http add urlacl url=http://127.0.0.1:{_port}/ user=Everyone listen=yes",
                    "Aurora - Error");

            return false;
        }

        _isRunning = true;

        _readThread.Trigger();
        return true;
    }

    private async Task AsyncRead1()
    {
        try
        {
            var context = await _netListener.GetContextAsync().WaitAsync(_cancellationToken);
            if (!_cancellationToken.IsCancellationRequested)
            {
                _readThread2.Trigger();
            }

            ProcessContext(context);
        }
        catch (TaskCanceledException)
        {
            // stop
        }
    }

    private async Task AsyncRead2()
    {
        try
        {
            var context = await _netListener.GetContextAsync().WaitAsync(_cancellationToken);
            if (!_cancellationToken.IsCancellationRequested)
            {
                _readThread.Trigger();
            }

            ProcessContext(context);
        }
        catch (TaskCanceledException)
        {
            // stop
        }
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public Task Stop()
    {
        _isRunning = false;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _netListener.Close();
        return Task.CompletedTask;
    }

    private void ProcessContext(HttpListenerContext context)
    {
        var path = context.Request.Url!.LocalPath;

        // find the exact path match
        if (_endpoints.TryGetValue(path, out var endpoint) && endpoint.HandleRequest(context))
            return;

        // find a regex path match
        try
        {
            var regexHandled = _regexEndpoints
                .Select(kv => (kv.Key.Match(path), kv.Value))
                .Where(kv => kv.Item1.Success)
                .Select(kv => kv.Value.HandleRequest(context, kv.Item1))
                .FirstOrDefault(k => k);

            if (regexHandled) return;
            var ctxResponse = context.Response;
            ctxResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            ctxResponse.Close();
            return;
        }
        catch (InvalidOperationException)
        {
            // no match
        }
        
        var method = context.Request.HttpMethod;
        if (!_allowedMethods.Contains(method))
        {
            var ctxResponse = context.Response;
            ctxResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            ctxResponse.Close();
            return;
        }

        // no match
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.NotFound;
        response.Headers = WebHeaderCollection;
        response.Close([], true);
    }

    public void OnNewJsonGameState(string gameId, string json)
    {
        var eventArgs = new JsonGameStateEventArgs(gameId, json);
        NewJsonGameState?.Invoke(this, eventArgs);
    }
}
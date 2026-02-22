using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.GameStateListen.Http;
using AuroraRgb.Profiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AuroraRgb.Modules.GameStateListen;

public sealed class JsonGameStateEventArgs(string gameId, Stream json) : EventArgs
{
    public string GameId { get; } = gameId;
    public Stream Json { get; } = json;
}

public sealed class AuroraHttpListener
{
    private static readonly FrozenDictionary<string, string> WebHeaderCollection = new Dictionary<string, string>
    {
        ["Access-Control-Allow-Origin"] = "*",
        ["Access-Control-Allow-Private-Network"] = "true",
    }.ToFrozenDictionary();

    private bool _isRunning;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    private readonly int _port;

    private readonly FrozenDictionary<string, AuroraEndpoint> _endpoints;
    private readonly FrozenDictionary<Regex, AuroraRegexEndpoint> _regexEndpoints;
    private readonly HashSet<string> _allowedMethods;

    private readonly IWebHost _netListener;

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
        _netListener = new WebHostBuilder()
            .UseKestrel(options => { options.ListenAnyIP(port); })
            .Configure(app => { app.Run(ProcessContext); })
            .Build();

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _endpoints = HttpEndpointFactory.CreateEndpoints(this);
        _regexEndpoints = HttpEndpointFactory.CreateRegexEndpoints(this);
        _allowedMethods = _endpoints.Values
            .Union<IAuroraEndpoint>(_regexEndpoints.Values)
            .SelectMany(endpoint => endpoint.AvailableMethods)
            .ToHashSet();
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
        return true;
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public async Task Stop()
    {
        _isRunning = false;
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();

        await _netListener.StopAsync(_cancellationToken);
    }

    private async Task ProcessContext(HttpContext context)
    {
        var path = context.Request.Path.Value;

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
            return;
        }
        catch (InvalidOperationException)
        {
            // no match
        }

        var method = context.Request.Method;
        if (!_allowedMethods.Contains(method))
        {
            Respond405(context);
            return;
        }

        // no match
        var response = Respond404(context);
        AddResponseHeaders(response);
    }

    private static void Respond405(HttpContext context)
    {
        var ctxResponse = context.Response;
        ctxResponse.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
    }

    private static HttpResponse Respond404(HttpContext context)
    {
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.NotFound;
        return response;
    }

    private static void AddResponseHeaders(HttpResponse response)
    {
        foreach (var keyValuePair in WebHeaderCollection)
        {
            response.Headers.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }

    public void OnNewJsonGameState(string gameId, string json)
    {
        var textStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var eventArgs = new JsonGameStateEventArgs(gameId, textStream);
        NewJsonGameState?.Invoke(this, eventArgs);
    }

    public void OnNewJsonGameState(string gameId, Stream json)
    {
        var eventArgs = new JsonGameStateEventArgs(gameId, json);
        NewJsonGameState?.Invoke(this, eventArgs);
    }
}
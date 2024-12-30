using System;
using System.Collections.Frozen;
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
    private IGameState _currentGameState = new NewtonsoftGameState("{}");
    private readonly HttpListener _netListener;
    private readonly int _port;
    private readonly SingleConcurrentThread _readThread;
    private readonly SingleConcurrentThread _readThread2;
    private readonly FrozenDictionary<string, AuroraEndpoint> _endpoints;
    private readonly FrozenDictionary<Regex, AuroraRegexEndpoint> _regexEndpoints;

    public IGameState CurrentGameState
    {
        get => _currentGameState;
        internal set
        {
            _currentGameState = value;
            NewGameState?.Invoke(this, value);
        }
    }

    /// <summary>
    ///  Event for handing a newly received game state
    /// </summary>
    public event EventHandler<IGameState>? NewGameState;

    public event EventHandler<JsonGameStateEventArgs>? NewJsonGameState;

     /// <summary>
    /// A GameStateListener that listens for connections on http://127.0.0.1:port/
    /// </summary>
    /// <param name="port"></param>
    public AuroraHttpListener(int port)
    {
        _port = port;
        _netListener = new HttpListener();
        _netListener.Prefixes.Add($"http://127.0.0.1:{port}/");

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _readThread = new SingleConcurrentThread("Http Read Thread 1", AsyncRead1, ExceptionCallback);
        _readThread2 = new SingleConcurrentThread("Http Read Thread 2", AsyncRead2, ExceptionCallback);
        
        _endpoints = HttpEndpointFactory.CreateEndpoints(this);
        _regexEndpoints = HttpEndpointFactory.CreateRegexEndpoints(this);
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
            
            if (exc.ErrorCode == 5)//Access Denied
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
        
        // find exact path match
        if (_endpoints.TryGetValue(path, out var endpoint))
        {
            endpoint.HandleRequest(context);
            return;
        }

        // find regex path match
        try
        {
            var (match, regexEndpoint) = _regexEndpoints
                .Select(kv => (kv.Key.Match(path), kv.Value))
                .First(kv => kv.Item1.Success);
            regexEndpoint.HandleRequest(context, match);
            return;
        }catch(InvalidOperationException)
        {
            // no match
        }
        
        // no match
        var response = context.Response;
        response.StatusCode = context.Request.HttpMethod switch
        {
             "OPTIONS" => (int)HttpStatusCode.OK,
            _ => (int)HttpStatusCode.NotFound
        };
        response.Headers = WebHeaderCollection;
        response.Close([], true);
    }

    public void OnNewJsonGameState(string gameId, string json)
    {
        var eventArgs = new JsonGameStateEventArgs(gameId, json);
        NewJsonGameState?.Invoke(this, eventArgs);
    }
}
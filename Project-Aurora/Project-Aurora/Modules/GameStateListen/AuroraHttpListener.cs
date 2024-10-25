using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Profiles;
using Common.Utils;

namespace AuroraRgb.Modules.GameStateListen;

public sealed class JsonGameStateEventArgs(string gameId, string json) : EventArgs
{
    public string GameId { get; } = gameId;
    public string Json { get; } = json;
}

public sealed partial class AuroraHttpListener
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

    public IGameState CurrentGameState
    {
        get => _currentGameState;
        private set
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
        
        _readThread = new SingleConcurrentThread("Http Read Thread", AsyncRead);
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

    private async Task AsyncRead()
    {
        var context = await _netListener.GetContextAsync();
        if (!_cancellationToken.IsCancellationRequested)
        {
            _readThread.Trigger();
        }
        ProcessContext(context);
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public Task Stop()
    {
        _isRunning = false;
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _netListener.Abort();
        _netListener.Close();
        return Task.CompletedTask;
    }

    private void ProcessContext(HttpListenerContext context)
    {
        var json = ReadContent(context, out var path);
        try
        {
            var response = TryProcessRequest(json, path);
            if (response != null)
            {
                CurrentGameState = response;
            }
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "[NetworkListener] ReceiveGameState error on: {Path}", path);
            Global.logger.Debug("JSON: {Json}", json);
        }
    }

    private NewtonsoftGameState? TryProcessRequest(string json, string path)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        if (!path.StartsWith("/gameState/"))
        {
            return new NewtonsoftGameState(json);
        }

        var match = GameStateRegex().Match(path);
        if (!match.Success)
        {
            return new NewtonsoftGameState(json);
        }
 
        var gameIdGroup = match.Groups[1];
        var gameId = gameIdGroup.Value;

        var eventArgs = new JsonGameStateEventArgs(gameId, json);
        NewJsonGameState?.Invoke(this, eventArgs);
        
        // set announce false to prevent LSM from setting it to a profile
        // also return NewtonsoftGameState for compatibility and GSI window 
        return new NewtonsoftGameState(json, false);
    }

    private static string ReadContent(HttpListenerContext context, out string path)
    {
        var request = context.Request;
        string json;

        using (var sr = new StreamReader(request.InputStream))
        {
            json = sr.ReadToEnd();
        }

        // immediately respond to the game, don't let it wait for response
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength64 = 0;
        response.Headers = WebHeaderCollection;
        response.Close([], true);
        
        path = request.Url.LocalPath;

        return json;
    }

    // https://regex101.com/r/N3BMIu/1
    [GeneratedRegex(@"\/gameState\/([a-zA-Z0-9_]*)\??\/?.*")]
    private static partial Regex GameStateRegex();
}
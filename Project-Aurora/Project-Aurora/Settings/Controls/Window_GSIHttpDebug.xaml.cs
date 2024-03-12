using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Profiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuroraRgb.Settings.Controls;

/// <summary>
/// A window that logs the latest new game state recieved via HTTP, regardless of the provider.
/// </summary>
public partial class Window_GSIHttpDebug
{
    private static Window_GSIHttpDebug? _httpDebugWindow;

    private readonly AuroraHttpListener _httpListener;

    /// <summary>
    /// Opens the HttpDebugWindow if not already opened. If opened bring it to the foreground. 
    /// </summary>
    /// <param name="httpListener"></param>
    public static void Open(Task<AuroraHttpListener?> httpListener)
    {
        var httpListenerResult = httpListener.Result;
        if (httpListenerResult == null)
        {
            return;
        }
        if (_httpDebugWindow == null)
        {
            _httpDebugWindow = new Window_GSIHttpDebug(httpListenerResult);
            _httpDebugWindow.Show();
        }
        else
        {
            _httpDebugWindow.Activate();
        }
    }

    private Timer? _timeDisplayTimer;
    private DateTime? _lastRequestTime;

    private Window_GSIHttpDebug(AuroraHttpListener httpListener)
    {
        _httpListener = httpListener;
        Closed += Window_Closed;
        InitializeComponent();
        DataContext = Global.Configuration;
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e) {
        // When the window has opened and loaded, start listening to the NetworkListener for when it
        // recieves new GameStates.
        _httpListener.NewGameState += Net_listener_NewGameState;

        // If a gamestate is already stored by the network listener, display it to the user immediately.
        SetJsonText(_httpListener.CurrentGameState.Json);

        // Start a timer to update the time displays for the request
        _timeDisplayTimer = new Timer(_ => Dispatcher.BeginInvoke(() => {
            if (_lastRequestTime.HasValue)
                CurRequestTime.Text = _lastRequestTime + " (" + (DateTime.UtcNow - _lastRequestTime).Value.TotalSeconds.ToString("0.00") + "s ago)";
        }, DispatcherPriority.DataBind), null, 0, 50);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        // When the window closes, we need to clean up our listener, otherwise it'll keep listening and
        // this window will never get garbage collected.
        _httpListener.NewGameState -= Net_listener_NewGameState;

        // Destory the timer to ensure it doesn't keep running and eating memory.
        _timeDisplayTimer?.Dispose();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        //Set the HttpDebugWindow instance to null if it got closed.
        if (_httpDebugWindow != null && _httpDebugWindow.Equals(this))
            _httpDebugWindow = null;
    }

    private void Net_listener_NewGameState(object? sender, IGameState gamestate)
    {
        // This needs to be invoked due to the UI thread being different from the networking thread.
        // Without this, an exception is thrown trying to update the text box.
        Dispatcher.BeginInvoke(() => SetJsonText(gamestate.Json), DispatcherPriority.DataBind);
        // Also record the time this request came in
        _lastRequestTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the text of the body preview text box to the given (json) string.
    /// </summary>
    private void SetJsonText(string json) {
        // Pretty-print the JSON (add new lines and indentations)
        BodyPreviewTxt.Text = JToken.Parse(json).ToString(Formatting.Indented);
    }
}
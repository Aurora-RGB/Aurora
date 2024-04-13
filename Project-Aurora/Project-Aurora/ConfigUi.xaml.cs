using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using AuroraRgb.Controls;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common;
using PropertyChanged;
using RazerSdkReader;
using Application = AuroraRgb.Profiles.Application;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Timers.Timer;

namespace AuroraRgb;

[DoNotNotify]
partial class ConfigUi : INotifyPropertyChanged
{
    private readonly  Control_Settings _settingsControl;
    private readonly Control_LayerControlPresenter _layerPresenter = new();
    private readonly Control_ProfileControlPresenter _profilePresenter = new();

    private readonly AuroraControlInterface _controlInterface;

    private DateTime _lastActivated = DateTime.UtcNow;
    private readonly TimeSpan _renderTimeout = TimeSpan.FromMinutes(5);
    private readonly SimpleColor _desktopColorScheme = new(0, 0, 0, 0);

    private SimpleColor _previousColor = SimpleColor.Transparent;
    private SimpleColor _currentColor = SimpleColor.Transparent;

    private double _transitionAmount;

    private FrameworkElement? _selectedManager;

    private readonly Timer _virtualKeyboardTimer = new(8);
    private readonly Func<Task> _keyboardTimerCallback;

    public static readonly DependencyProperty FocusedApplicationProperty = DependencyProperty.Register(
        nameof(FocusedApplication), typeof(Application), typeof(ConfigUi),
        new PropertyMetadata(null, FocusedProfileChanged));

    private readonly Task<KeyboardLayoutManager> _layoutManager;
    private readonly Task<LightingStateManager> _lightingStateManager;

    private readonly TransparencyComponent _transparencyComponent;

    private bool _keyboardUpdating;
    private readonly Func<Task> _updateKeyboardLayouts;

    private static bool IsDragging { get; set; }

    public readonly Uri BaseUri;
    
    public Application? FocusedApplication
    {
        get => GetValue(FocusedApplicationProperty) as Application;
        set
        {
            SetValue(FocusedApplicationProperty, value);
            _lightingStateManager.Result.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
        }
    }

    private CancellationTokenSource _keyboardUpdateCancel = new();

    public ConfigUi(Task<ChromaReader?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener,
        Task<IpcListener?> ipcListener, Task<DeviceManager> deviceManager, Task<LightingStateManager> lightingStateManager, AuroraControlInterface controlInterface)
    {
        BaseUri = BaseUriHelper.GetBaseUri(this);
        
        _layoutManager = layoutManager;
        _lightingStateManager = lightingStateManager;
        _controlInterface = controlInterface;

        _settingsControl = new Control_Settings(rzSdkManager, pluginManager, httpListener, deviceManager, ipcListener);
        
        _updateKeyboardLayouts = async () =>
        {
            if (_keyboardUpdating)
            {
                return;
            }
            _keyboardUpdating = true;

            if (Global.key_recorder?.IsRecording() ?? false)
            {
                KeyboardRecordMessage.Visibility = Visibility.Visible;
                KeyboardViewBorder.BorderBrush = Brushes.Red;
            }
            else
            {
                KeyboardRecordMessage.Visibility = Visibility.Hidden;
                KeyboardViewBorder.BorderBrush = Brushes.Transparent;
            }

            var keyLights = Global.effengine.GetKeyboardLights();
            (await _layoutManager).SetKeyboardColors(keyLights, _keyboardUpdateCancel.Token);

            _keyboardUpdating = false;
        };
        
        InitializeComponent();

        _transparencyComponent = new TransparencyComponent(this, bg_grid);

        ctrlProfileManager.ProfileSelected += CtrlProfileManager_ProfileSelected;

        _settingsControl.DataContext = this;

        _keyboardTimerCallback = KeyboardTimerCallback;
        _virtualKeyboardTimer.Elapsed += virtual_keyboard_timer_Tick;

        _profilesStack = new Control_ProfilesStack(httpListener, lightingStateManager);
        _profilesStack.FocusedAppChanged += ProfilesStackOnFocusedAppChanged;
        _profilesStack.SettingsSelected += ProfilesStackOnSettingsSelected;
        
        ProfileStackGrid.Children.Add(_profilesStack);
    }

    private void ProfilesStackOnSettingsSelected(object? sender, EventArgs e)
    {
        FocusedApplication = null;
        SelectedControl = _settingsControl;

        _previousColor = _currentColor;
        _currentColor = _desktopColorScheme;
        _transitionAmount = 0.0;
    }

    private void ProfilesStackOnFocusedAppChanged(object? sender, FocusedAppChangedEventArgs e)
    {
        TransitionToProfile(e.Application, e.ImageBitmap);
    }

    private async Task KeyboardTimerCallback()
    {
        if (DateTime.UtcNow - _lastActivated > _renderTimeout)
        {
            return;
        }

        _transitionAmount += _keyboardTimer.Elapsed.TotalSeconds;
        if (IsDragging)
        {
            _transitionAmount = 0.4;
            _previousColor = SimpleColor.Transparent;
        }
        if (_transitionAmount <= 1.0f)
        {
            var smooth = 1 - Math.Pow(1 - Math.Min(_transitionAmount, 1d), 4);
            var a = ColorUtils.BlendColors(_previousColor, _currentColor, smooth);
            _transparencyComponent.SetBackgroundColor(a);
        }
        
        if (_keyboardUpdating)
        {
            return;
        }

        await _keyboardUpdateCancel.CancelAsync();
        _keyboardUpdateCancel.Dispose();
        _keyboardUpdateCancel = new CancellationTokenSource();

        await Dispatcher.BeginInvoke(_updateKeyboardLayouts, IsDragging ? DispatcherPriority.Background : DispatcherPriority.Render);
    }

    public void Display()
    {
        ShowInTaskbar = true;
        if (Top <= 0)
        {
            Top = 0;
        }

        if (Left <= 0)
        {
            Left = 0;
        }
        if (WindowStyle != WindowStyle.None)
        {
            WindowStyle = WindowStyle.None;
        }
        Show();
        Activate();
    }

    private void CtrlProfileManager_ProfileSelected(ApplicationProfile profile)
    {
        _profilePresenter.Profile = profile;

        if (_selectedManager == ctrlProfileManager)
            SelectedControl = _profilePresenter;   
    }

    private async void KbLayout_KeyboardLayoutUpdated(object? sender)
    {
        var keyboardLayoutManager = await _layoutManager;
        var virtualKb = await keyboardLayoutManager.VirtualKeyboard;

        KeyboardGrid.Children.Clear();
        KeyboardGrid.Children.Add(virtualKb);
        KeyboardGrid.Children.Add(new LayerEditor());

        KeyboardGrid.Width = virtualKb.Width;
        KeyboardGrid.Height = virtualKb.Height;
        KeyboardGrid.UpdateLayout();

        KeyboardViewbox.MaxWidth = virtualKb.Width + 50;
        KeyboardViewbox.MaxHeight = virtualKb.Height + 50;
        KeyboardViewbox.UpdateLayout();

        UpdateLayout();
    }

    private async void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }
        
        (await _layoutManager).KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;

        if (windowHwndSource != null)
        {
            windowHwndSource.RemoveHook(WndProcDrag);
            windowHwndSource.Dispose();
            windowHwndSource = null;
        }
        var handle = new WindowInteropHelper(this).Handle;
        // Subclass the window to intercept messages
        windowHwndSource = HwndSource.FromHwnd(handle);
        windowHwndSource?.AddHook(WndProcDrag);

        _virtualKeyboardTimer.Start();

        KeyboardRecordMessage.Visibility = Visibility.Hidden;

        _currentColor = SimpleColor.Black;

        var keyboardLayoutManager = await _layoutManager;
        var virtualKb = await keyboardLayoutManager.VirtualKeyboard;

        KeyboardGrid.Children.Clear();
        KeyboardGrid.Children.Add(virtualKb);
        KeyboardGrid.Children.Add(new LayerEditor());

        KeyboardGrid.Width = virtualKb.Width;
        KeyboardGrid.Height = virtualKb.Height;
        KeyboardGrid.UpdateLayout();

        KeyboardViewbox.MaxWidth = virtualKb.Width + 30;
        KeyboardViewbox.MaxHeight = virtualKb.Height + 30;
        KeyboardViewbox.UpdateLayout();

        await GenerateProfileStack();
        UpdateManagerStackFocus(ctrlLayerManager, true);
    }

   private static IntPtr WndProcDrag(IntPtr winHandle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
   {
       const int wmEnterSizeMove = 0x0231;
       const int wmExitSizeMove = 0x0232;
       switch (msg)
       {
           case wmEnterSizeMove:
               IsDragging = true;
               break;
           case wmExitSizeMove:
               IsDragging = false;
               break;
       }

       return IntPtr.Zero;
   }

    private async Task GenerateProfileStack()
    {
        await _profilesStack.GenerateProfileStack();
    }

    private async void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        _virtualKeyboardTimer.Stop();

        (await _layoutManager).KeyboardLayoutUpdated -= KbLayout_KeyboardLayoutUpdated;

        KeyboardGrid.Children.Clear();

        windowHwndSource?.RemoveHook(WndProcDrag);
        windowHwndSource?.Dispose();
        windowHwndSource = null;
    }

    private readonly Stopwatch _keyboardTimer = Stopwatch.StartNew();
    private void virtual_keyboard_timer_Tick(object? sender, EventArgs e)
    {
        if (Visibility != Visibility.Visible) return;
        _keyboardTimerCallback.Invoke();
        _keyboardTimer.Restart();
    }

    ////Misc

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (FocusedApplication != null)
        { 
            await FocusedApplication.SaveAll();
        }

        switch (Global.Configuration.CloseMode)
        {
            case AppExitMode.Ask:
            {
                var result = MessageBox.Show("Would you like to Exit Aurora?",
                    "Aurora", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    await MinimizeApp();
                    e.Cancel = true;
                }
                else
                {
                    await _controlInterface.ShutdownDevices();
                    _controlInterface.ExitApp();
                }

                break;
            }
            case AppExitMode.Minimize:
                await MinimizeApp();
                e.Cancel = true;
                break;
            default:
                await _controlInterface.ShutdownDevices();
                _controlInterface.ExitApp();
                break;
        }
    }

    private async Task MinimizeApp()
    {
        var lightingStateManager = await _lightingStateManager;
        lightingStateManager.PreviewProfileKey = string.Empty;

        Hide();
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        _lastActivated = DateTime.UtcNow;
    }

    private void TransitionToProfile(Application application, BitmapSource bitmap)
    {
        FocusedApplication = application;
        var color = ColorUtils.GetAverageColor(bitmap);

        _previousColor = _currentColor;
        _currentColor = ColorUtils.DrawingToSimpleColor(color) * 0.85;

        _transitionAmount = 0.0;
    }

    private static void FocusedProfileChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
        var th = (ConfigUi)source;
        var value = e.NewValue as Application;

        th.gridManagers.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;

        if (value == null)
            return;

        th.SelectedControl = value.Control;
    }

    private void UpdateManagerStackFocus(object? focusedElement, bool forced = false)
    {
        if (focusedElement is not FrameworkElement element || (element.Equals(_selectedManager) && !forced)) return;
        _selectedManager = element;
        if(gridManagers.ActualHeight != 0)
            stackPanelManagers.Height = gridManagers.ActualHeight;
        var totalHeight = stackPanelManagers.Height;

        foreach (FrameworkElement child in stackPanelManagers.Children)
        {
            if(child.Equals(element))
                child.Height = totalHeight - 28.0 * (stackPanelManagers.Children.Count - 1);
            else
                child.Height = 25.0;
        }
        _selectedManager.RaiseEvent(new RoutedEventArgs(GotFocusEvent));
    }

    private void ctrlLayerManager_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (_selectedManager != sender)
            SelectedControl = FocusedApplication.Profile.Layers.Count > 0 ? _layerPresenter : FocusedApplication.Control;
        UpdateManagerStackFocus(sender);
    }

    private void ctrlOverlayLayerManager_PreviewMouseDown(object? sender, MouseButtonEventArgs e) {
        if (_selectedManager != sender)
            SelectedControl = FocusedApplication.Profile.OverlayLayers.Count > 0 ? _layerPresenter : FocusedApplication.Control;
        UpdateManagerStackFocus(sender);
    }

    private void ctrlProfileManager_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (_selectedManager != sender)
            SelectedControl = _profilePresenter;
        UpdateManagerStackFocus(sender);
    }

    private void brdOverview_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        _selectedManager = SelectedControl = FocusedApplication.Control;

    }

    private void Window_SizeChanged(object? sender, SizeChangedEventArgs e) {
        UpdateManagerStackFocus(_selectedManager, true);
    }

    // This new code for the layer selection has been separated from the existing code so that one day we can sort all
    // the above out and make it more WPF with bindings and other dark magic like that.
    #region PropertyChangedEvent and Helpers
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Call the PropertyChangedEvent for a single property.
    /// </summary>
    private void NotifyChanged(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    /// <summary>
    /// Sets a field and calls <see cref="NotifyChanged(string)"/> with the calling member name and any additional properties.
    /// Designed for setting a field from a property.
    /// </summary>
    private void SetField<T>(ref T var, T value, string[]? additional = null, [CallerMemberName] string name = null) {
        var = value;
        NotifyChanged(name);
        if (additional == null) return;
        foreach (var prop in additional)
            NotifyChanged(prop);
    }
    #endregion

    #region Properties
    /// <summary>A reference to the currently selected layer in either the regular or overlay layer list. When set, will update the <see cref="SelectedControl"/> property.</summary>
    public Layer? SelectedLayer {
        get => _selectedLayer;
        set {
            SetField(ref _selectedLayer, value);
            if (value == null)
                SelectedControl = FocusedApplication?.Control;
            else {
                _layerPresenter.Layer = value;
                SelectedControl = _layerPresenter;
            }
        }
    }
    private Layer? _selectedLayer;

    /// <summary>The control that is currently displayed underneath they device preview panel. This could be an overview control or a layer presenter etc.</summary>
    public Control? SelectedControl { get => _selectedControl; set => SetField(ref _selectedControl, value); }
    private Control? _selectedControl;
    private readonly Control_ProfilesStack _profilesStack;
    private HwndSource? windowHwndSource;

    #endregion
}

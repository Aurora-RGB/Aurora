using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using AuroraRgb.Controls;
using AuroraRgb.Devices;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.Layouts;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common;
using Common.Devices;
using Common.Utils;
using PropertyChanged;
using Application = AuroraRgb.Profiles.Application;
using Brushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;

namespace AuroraRgb;

[DoNotNotify]
sealed partial class ConfigUi : INotifyPropertyChanged, IDisposable
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

    private bool _runKeyboardUpdate = true;
    private readonly SingleConcurrentThread _virtualKeyboardTimer;
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

    private readonly UpdateModule _updateModule;
    private static bool _changelogsShown;
    private bool _renderPauseNoticeVisible = true;
    
    public Application? FocusedApplication
    {
        get => GetValue(FocusedApplicationProperty) as Application;
        set
        {
            SetValue(FocusedApplicationProperty, value);
            _lightingStateManager.Result.ApplicationManager.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
        }
    }

    private CancellationTokenSource _keyboardUpdateCancel = new();

    public ConfigUi(Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener,
        Task<IpcListener?> ipcListener, Task<DeviceManager> deviceManager, Task<LightingStateManager> lightingStateManager,
        AuroraControlInterface controlInterface, UpdateModule updateModule)
    {
        BaseUri = BaseUriHelper.GetBaseUri(this);
        
        _layoutManager = layoutManager;
        _lightingStateManager = lightingStateManager;
        _controlInterface = controlInterface;
        _updateModule = updateModule;

        _settingsControl = new Control_Settings(pluginManager, httpListener, deviceManager, ipcListener, layoutManager);
        
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

            (await _layoutManager).SetKeyboardColors(_uiKeyColors, _keyboardUpdateCancel.Token);

            _keyboardUpdating = false;
        };
        
        InitializeComponent();
        
        // append version to title
        var auroraVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        WindowTitle.Text += $" - {auroraVersion}";

        _transparencyComponent = new TransparencyComponent(this, bg_grid);

        ctrlProfileManager.ProfileSelected += CtrlProfileManager_ProfileSelected;

        _settingsControl.DataContext = this;

        _keyboardTimerCallback = KeyboardTimerCallback;
        _virtualKeyboardTimer = new SingleConcurrentThread("ConfigUI render thread", virtual_keyboard_timer_Tick, ExceptionCallback);

        _profilesStack = new Control_ProfilesStack(httpListener, lightingStateManager);
        _profilesStack.FocusedAppChanged += ProfilesStackOnFocusedAppChanged;
        _profilesStack.SettingsSelected += ProfilesStackOnSettingsSelected;
        
        ProfileStackGrid.Children.Add(_profilesStack);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new FakeWindowsPeer(this);
    }

    private void ExceptionCallback(object? sender, SingleThreadExceptionEventArgs eventArgs)
    {
        Global.logger.Error(eventArgs.Exception, "Error updating virtual keyboard");
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
            if (!_renderPauseNoticeVisible)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    _renderPauseNoticeVisible = true;
                    RenderPauseNotice.Visibility = Visibility.Visible;
                });
            }
            return;
        }

        if (_renderPauseNoticeVisible)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _renderPauseNoticeVisible = false;
                RenderPauseNotice.Visibility = Visibility.Collapsed;
            });
        }

        _transitionAmount += _keyboardTimer.Elapsed.TotalSeconds;
        if (IsDragging)
        {
            _transitionAmount = 0.3;
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

        await CancelKeyboardUpdate();

        var dispatcherPriority = IsDragging ? DispatcherPriority.Background : DispatcherPriority.Render;
        var keyLights = Global.effengine.GetKeyboardLights();
        ConvertToMediaColors(keyLights);
        await Dispatcher.InvokeAsync(_updateKeyboardLayouts, dispatcherPriority, _keyboardUpdateCancel.Token);
    }

    private void ConvertToMediaColors(IDictionary<DeviceKeys, SimpleColor> keyColors)
    {
        foreach (var (key, color) in keyColors)
        {
            if (color.A == 0)
            {
                _uiKeyColors[key] = MediaColor.FromArgb(255, 0, 0, 0);
                continue;
            }
            _uiKeyColors[key] = ToMediaColor(color);
        }
    }

    private static MediaColor ToMediaColor(SimpleColor color)
    {
        var opaqueColor = ColorUtils.MultiplyColorByScalar(color, color.A / 255.0D);
        return MediaColor.FromArgb(opaqueColor.A, opaqueColor.R, opaqueColor.G, opaqueColor.B);
    }

    private async Task CancelKeyboardUpdate()
    {
        var keyboardUpdateCancel = _keyboardUpdateCancel;
        _keyboardUpdateCancel = new CancellationTokenSource();
        await keyboardUpdateCancel.CancelAsync();
        keyboardUpdateCancel.Dispose();
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

        if (_windowHwndSource != null)
        {
            _windowHwndSource.RemoveHook(WndProcDrag);
            _windowHwndSource.Dispose();
            _windowHwndSource = null;
        }
        var handle = new WindowInteropHelper(this).Handle;
        // Subclass the window to intercept messages
        _windowHwndSource = HwndSource.FromHwnd(handle);
        _windowHwndSource?.AddHook(WndProcDrag);

        _runKeyboardUpdate = true;
        _virtualKeyboardTimer.Trigger();

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

        if (_changelogsShown)
        {
            return;
        }
        await ShowChangelogs();
    }

    private async Task ShowChangelogs()
    {
        var changelogs = await _updateModule.Changelogs;
        if (changelogs.Length == 0) return;

        var changelogWindow = new Window_Changelogs(changelogs, _updateModule);
        changelogWindow.Show();
        _changelogsShown = true;
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

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        KeyboardGrid.Children.Clear();
        Task.Run(Window_UnloadedAsync).Wait();
    }

    private async Task Window_UnloadedAsync()
    {
        _runKeyboardUpdate = false;
        await CancelKeyboardUpdate();

        (await _layoutManager).KeyboardLayoutUpdated -= KbLayout_KeyboardLayoutUpdated;

        _windowHwndSource?.RemoveHook(WndProcDrag);
        _windowHwndSource?.Dispose();
        _windowHwndSource = null;
    }

    private readonly Stopwatch _keyboardTimer = Stopwatch.StartNew();
    private void virtual_keyboard_timer_Tick()
    {
        if (Visibility != Visibility.Visible || !_runKeyboardUpdate) return;
        _keyboardTimerCallback.Invoke();
        _keyboardTimer.Restart();
        Thread.Sleep(8);
        _virtualKeyboardTimer.Trigger();
    }

    ////Misc

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        var focusedApp = FocusedApplication;
        Hide();
        FocusedApplication = null;
        if (Global.Configuration.CloseMode == AppExitMode.Ask)
        {
            var result = MessageBox.Show("Would you like to Exit Aurora?", "Aurora", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                Hide();
                e.Cancel = true;
                return;
            }

            Task.Run(() => Window_ClosingAsync(focusedApp)).Wait();
        }

        Task.Run(() => Window_ClosingAsync(focusedApp)).Wait();
    }

    private async Task Window_ClosingAsync(Application? focusedApp)
    {
        _runKeyboardUpdate = false;
        await CancelKeyboardUpdate();

        if (focusedApp != null)
        { 
            await focusedApp.SaveAll();
        }

        var lightingStateManager = await _lightingStateManager;
        lightingStateManager.ApplicationManager.PreviewProfileKey = string.Empty;

        switch (Global.Configuration.CloseMode)
        {
            case AppExitMode.Ask:
            {
                // handled in the sync part
                return;
            }
            case AppExitMode.Minimize:
                // let the window dispose itself
                break;
            default:
                await _controlInterface.ShutdownDevices();
                _controlInterface.ExitApp();
                break;
        }
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
        if (_selectedManager != sender && FocusedApplication != null)
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

            if (FocusedApplication?.Profile != null)
            {
                FocusedApplication.SaveProfile(FocusedApplication.Profile);
            }
        }
    }
    private Layer? _selectedLayer;

    /// <summary>The control that is currently displayed underneath they device preview panel. This could be an overview control or a layer presenter etc.</summary>
    public Control? SelectedControl { get => _selectedControl; set => SetField(ref _selectedControl, value); }
    private Control? _selectedControl;
    private readonly Control_ProfilesStack _profilesStack;
    private HwndSource? _windowHwndSource;
    private readonly Dictionary<DeviceKeys, MediaColor> _uiKeyColors = new(Effects.MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

    #endregion

    public void Dispose()
    {
        _runKeyboardUpdate = false;
        _virtualKeyboardTimer.Dispose(50);
    }

    private void RenderPauseNotice_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _lastActivated = DateTime.UtcNow;
    }

    private void ConfigUi_OnClosed(object? sender, EventArgs e)
    {
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            PerformanceModeModule.UpdatePriority();
        });
    }

    private void KeyboardViewBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}

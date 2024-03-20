using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.Controls;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Aurora_Wrapper;
using AuroraRgb.Profiles.Generic_Application;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common;
using PropertyChanged;
using RazerSdkReader;
using Application = AuroraRgb.Profiles.Application;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
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
    private readonly Task<AuroraHttpListener?> _httpListener;
    private readonly Task<LightingStateManager> _lightingStateManager;

    private readonly TransparencyComponent _transparencyComponent;

    private bool _keyboardUpdating;
    private readonly Func<Task> _updateKeyboardLayouts;

    private static readonly bool DisposeWindow = false;

    private static bool IsDragging { get; set; }

    public Application? FocusedApplication
    {
        get => GetValue(FocusedApplicationProperty) as Application;
        set
        {
            SetValue(FocusedApplicationProperty, value);
            _lightingStateManager.Result.PreviewProfileKey = value != null ? value.Config.ID : string.Empty;
        }
    }

    private bool _showHidden;

    public bool ShowHidden
    {
        get => _showHidden;
        set
        {
            _showHidden = value;
            ShowHiddenChanged(value);
        }
    }

    private CancellationTokenSource _keyboardUpdateCancel = new();

    public ConfigUi(Task<ChromaReader?> rzSdkManager, Task<PluginManager> pluginManager,
        Task<KeyboardLayoutManager> layoutManager, Task<AuroraHttpListener?> httpListener,
        Task<IpcListener?> ipcListener, Task<DeviceManager> deviceManager, Task<LightingStateManager> lightingStateManager, AuroraControlInterface controlInterface)
    {
        _httpListener = httpListener;
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

        //Show hidden profiles button
        _profileHidden = new Image
        {
            Source = _notVisible,
            ToolTip = "Toggle Hidden profiles' visibility",
            Margin = new Thickness(0, 5, 0, 0)
        };
        _profileHidden.MouseDown += HiddenProfile_MouseDown;
    }

    private async Task KeyboardTimerCallback()
    {
        if (DateTime.UtcNow - _lastActivated > _renderTimeout)
        {
            return;
        }

        if (IsDragging)
        {
            _transitionAmount = 0.4;
            _previousColor = SimpleColor.Transparent;
        } else if (_transitionAmount <= 1.0f)
        {
            _transitionAmount += _keyboardTimer.Elapsed.TotalSeconds;
            var smooth = 1 - Math.Pow(1 - Math.Min(_transitionAmount, 1d), 3);
            var a = ColorUtils.BlendColors(_previousColor, _currentColor, smooth);
            _transparencyComponent.SetBackgroundColor(a);
        }
        
        if (_transitionAmount <= 1.0f)
        {
            _transitionAmount += _keyboardTimer.Elapsed.TotalSeconds;
            var smooth = 1 - Math.Pow(1 - Math.Min(_transitionAmount, 1d), 3);
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

    public async Task Initialize()
    {
        await GenerateProfileStack();
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

        if (_selectedManager.Equals(ctrlProfileManager))
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
        (await _layoutManager).KeyboardLayoutUpdated += KbLayout_KeyboardLayoutUpdated;
        
        var handle = new WindowInteropHelper(this).Handle;
        // Subclass the window to intercept messages
        var source = HwndSource.FromHwnd(handle);
        source?.AddHook(WndProcDrag);

        KeyboardRecordMessage.Visibility = Visibility.Hidden;

        _currentColor = SimpleColor.Transparent;

        var keyboardLayoutManager = await _layoutManager;
        var virtualKb = await keyboardLayoutManager.VirtualKeyboard;

        KeyboardGrid.Children.Add(virtualKb);
        KeyboardGrid.Children.Add(new LayerEditor());

        KeyboardGrid.Width = virtualKb.Width;
        KeyboardGrid.Height = virtualKb.Height;
        KeyboardGrid.UpdateLayout();

        KeyboardViewbox.MaxWidth = virtualKb.Width + 30;
        KeyboardViewbox.MaxHeight = virtualKb.Height + 30;
        KeyboardViewbox.UpdateLayout();

        UpdateManagerStackFocus(ctrlLayerManager);

        foreach (Image child in profiles_stack.Children)
        {
            if (child.Visibility != Visibility.Visible) continue;
            ProfileImage_MouseDown(child, null);
            break;
        }

        _virtualKeyboardTimer.Start();
        return;

        IntPtr WndProcDrag(IntPtr winHandle, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
    }

    private async void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        _virtualKeyboardTimer.Stop();

        (await _layoutManager).KeyboardLayoutUpdated -= KbLayout_KeyboardLayoutUpdated;

        KeyboardGrid.Children.Clear();
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
        FocusedApplication?.SaveAll();

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
                if (!DisposeWindow)
                {
                    e.Cancel = true;
                }
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

    private readonly Image _profileAdd = new()
    {
        Source = new BitmapImage(new Uri(@"Resources/addprofile_icon.png", UriKind.Relative)),
        ToolTip = "Add a new Lighting Profile",
        Margin = new Thickness(0, 5, 0, 0)
    };

    private readonly Image _profileHidden;

    private readonly BitmapImage _visible = new(new Uri(@"Resources/Visible.png", UriKind.Relative));
    private readonly BitmapImage _notVisible = new(new Uri(@"Resources/Not Visible.png", UriKind.Relative));

    private async Task GenerateProfileStack(string? focusedKey = null)
    {
        profiles_stack.Children.Clear();

        var lightingStateManager = await _lightingStateManager;
        foreach (var application in Global.Configuration.ProfileOrder
                     .Where(profileName => lightingStateManager.Events.ContainsKey(profileName))
                     .Select(profileName => (Application)lightingStateManager.Events[profileName])
                     .OrderBy(item => item.Settings.Hidden ? 1 : 0))
        {
            if (application is GenericApplication)
            {
                await Dispatcher.BeginInvoke(() => { CreateInsertGenericApplication(focusedKey, application); }, DispatcherPriority.Loaded);
            }
            else
            {
                await Dispatcher.BeginInvoke(() =>
                {
                    var profileImage = new Image
                    {
                        Tag = application,
                        Source = application.Icon,
                        ToolTip = application.Config.Name + " Settings",
                        Margin = new Thickness(0, 5, 0, 0),
                        Visibility = application.Settings.Hidden ? Visibility.Collapsed : Visibility.Visible
                    };
                    profileImage.MouseDown += ProfileImage_MouseDown;
                    profiles_stack.Children.Add(profileImage);

                    if (!application.Config.ID.Equals(focusedKey)) return;
                    FocusedApplication = application;
                    TransitionToProfile(profileImage);
                }, DispatcherPriority.Loaded);
            }
        }

        //Add new profiles button
        _profileAdd.MouseDown -= AddProfile_MouseDown;
        _profileAdd.MouseDown += AddProfile_MouseDown;
        profiles_stack.Children.Add(_profileAdd);
        profiles_stack.Children.Add(_profileHidden);
    }

    private void CreateInsertGenericApplication(string? focusedKey, Application application)
    {
        var settings = (GenericApplicationSettings)application.Settings;
        var profileImage = new Image
        {
            Tag = application,
            Source = application.Icon,
            ToolTip = settings.ApplicationName + " Settings",
            Margin = new Thickness(0, 0, 0, 5)
        };
        profileImage.MouseDown += ProfileImage_MouseDown;

        var profileRemove = new Image
        {
            Source = new BitmapImage(new Uri(@"Resources/removeprofile_icon.png", UriKind.Relative)),
            ToolTip = $"Remove {settings.ApplicationName} Profile",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 16,
            Width = 16,
            Visibility = Visibility.Hidden,
            Tag = application.Config.ID
        };
        profileRemove.MouseDown += RemoveProfile_MouseDown;

        var profileGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
            Margin = new Thickness(0, 5, 0, 0),
            Tag = profileRemove
        };

        profileGrid.MouseEnter += Profile_grid_MouseEnter;
        profileGrid.MouseLeave += Profile_grid_MouseLeave;

        profileGrid.Children.Add(profileImage);
        profileGrid.Children.Add(profileRemove);

        profiles_stack.Children.Add(profileGrid);

        if (!application.Config.ID.Equals(focusedKey)) return;
        FocusedApplication = application;
        TransitionToProfile(profileImage);
    }

    private void HiddenProfile_MouseDown(object? sender, EventArgs e)
    {
        ShowHidden = !ShowHidden;
    }

    private void ShowHiddenChanged(bool value)
    {
        _profileHidden.Source = value ? _visible : _notVisible;

        foreach (FrameworkElement ctrl in profiles_stack.Children)
        {
            var img = ctrl as Image ?? (ctrl is Grid grid ? grid.Children[0] as Image : null);
            if (img?.Tag is not Application profile) continue;
            img.Visibility = profile.Settings.Hidden && !value ? Visibility.Collapsed : Visibility.Visible;
            img.Opacity = profile.Settings.Hidden ? 0.5 : 1;
        }
    }

    private void mbtnHidden_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem btn)
        {
            return;
        }

        if (cmenuProfiles.PlacementTarget is not Image img) return;
        img.Opacity = btn.IsChecked ? 0.5 : 1;

        if (!ShowHidden && btn.IsChecked)
            img.Visibility = Visibility.Collapsed;

        (img.Tag as Application)?.SaveProfiles();
    }

    private void cmenuProfiles_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        if (((ContextMenu)e.Source).PlacementTarget is not Image)
            e.Handled = true;
    }

    private void ContextMenu_Opened(object? sender, RoutedEventArgs e)
    {
        var context = (ContextMenu)e.OriginalSource;

        if (context.PlacementTarget is not Image img)
            return;

        var profile = img.Tag as Application;
        context.DataContext = profile;
    }

    private void Profile_grid_MouseLeave(object? sender, MouseEventArgs e)
    {
        if ((sender as Grid)?.Tag is Image)
            ((Image)((Grid)sender).Tag).Visibility = Visibility.Hidden;
    }

    private void Profile_grid_MouseEnter(object? sender, MouseEventArgs e)
    {
        if ((sender as Grid)?.Tag is Image)
            ((Image)((Grid)sender).Tag).Visibility = Visibility.Visible;
    }

    private void TransitionToProfile(Image source)
    {
        FocusedApplication = source.Tag as Application;
        var bitmap = (BitmapSource)source.Source;
        var color = ColorUtils.GetAverageColor(bitmap);

        _previousColor = _currentColor;
        _currentColor = ColorUtils.DrawingToSimpleColor(color) * 0.85;

        _transitionAmount = 0.0;
    }

    private void ProfileImage_MouseDown(object? sender, MouseButtonEventArgs? e)
    {
        if (sender is not Image { Tag: Application } image) return;
        if (e == null || e.LeftButton == MouseButtonState.Pressed)
            TransitionToProfile(image);
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            cmenuProfiles.PlacementTarget = image;
            cmenuProfiles.IsOpen = true;
        }
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

    private async void RemoveProfile_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not Image { Tag: string } image)
        {
            return;
        }

        var name = (string)image.Tag;

        var lightingStateManager = await _lightingStateManager;
        if (!lightingStateManager.Events.TryGetValue(name, out var value)) return;
        var applicationName = (((Application)value).Settings as GenericApplicationSettings).ApplicationName;
        if (MessageBox.Show(
                "Are you sure you want to delete profile for " +
                applicationName + "?", "Remove Profile", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        var eventList = Global.Configuration.ProfileOrder
            .ToDictionary(x => x, x => lightingStateManager.Events[x])
            .Where(x => ShowHidden || !(x.Value as Application).Settings.Hidden)
            .ToList();
        var idx = Math.Max(eventList.FindIndex(x => x.Key == name), 0);
        lightingStateManager.RemoveGenericProfile(name);
        await GenerateProfileStack(eventList[idx].Key);
    }

    private async void AddProfile_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        var dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title ="Add Profile" };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath))
            return; // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

        var filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());

        var lightingStateManager = await _lightingStateManager;
        if (lightingStateManager.Events.ContainsKey(filename))
        {
            if (lightingStateManager.Events[filename] is GameEvent_Aurora_Wrapper)
                lightingStateManager.Events.Remove(filename);
            else
            {
                MessageBox.Show("Profile for this application already exists.");
                return;
            }
        }

        var genAppPm = new GenericApplication(filename);
        genAppPm.Initialize();
        ((GenericApplicationSettings)genAppPm.Settings).ApplicationName = Path.GetFileNameWithoutExtension(filename);

        var ico = System.Drawing.Icon.ExtractAssociatedIcon(dialog.ChosenExecutablePath.ToLowerInvariant());

        if (!Directory.Exists(genAppPm.GetProfileFolderPath()))
            Directory.CreateDirectory(genAppPm.GetProfileFolderPath());

        using (var iconAsbitmap = ico.ToBitmap())
        {
            iconAsbitmap.Save(Path.Combine(genAppPm.GetProfileFolderPath(), "icon.png"), ImageFormat.Png);
        }
        ico.Dispose();

        lightingStateManager.RegisterEvent(genAppPm);
        ConfigManager.Save(Global.Configuration);
        GenerateProfileStack(filename);
    }

    private void DesktopControl_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        FocusedApplication = null;
        SelectedControl = _settingsControl;

        _previousColor = _currentColor;
        _currentColor = _desktopColorScheme;
        _transitionAmount = 0.0;
    }
    private void cmbtnOpenBitmapWindow_Clicked(object? sender, RoutedEventArgs e) => Window_BitmapView.Open();
    private void cmbtnOpenHttpDebugWindow_Clicked(object? sender, RoutedEventArgs e) =>Window_GSIHttpDebug.Open(_httpListener);

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

    #endregion
}

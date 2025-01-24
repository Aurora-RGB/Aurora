using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Generic_Application;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using Application = AuroraRgb.Profiles.Application;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;

namespace AuroraRgb;

public class FocusedAppChangedEventArgs(Application application, BitmapSource imageBitmap) : EventArgs
{
    public Application Application => application;
    public BitmapSource ImageBitmap => imageBitmap;
}

public partial class Control_ProfilesStack
{
    public event EventHandler<FocusedAppChangedEventArgs>? FocusedAppChanged;
    public event EventHandler? SettingsSelected;

    private readonly Task<AuroraHttpListener?> _httpListener;
    private readonly Task<LightingStateManager> _lightingStateManager;

    private bool _showHidden;

    private readonly BitmapImage _visible = new(new Uri(@"Resources/Visible.png", UriKind.Relative));
    private readonly BitmapImage _notVisible = new(new Uri(@"Resources/Not Visible.png", UriKind.Relative));

    private CancellationTokenSource _generateStackCancelSource = new();

    public bool ShowHidden
    {
        get => _showHidden;
        set
        {
            _showHidden = value;
            ShowHiddenChanged(value);
        }
    }

    public Control_ProfilesStack(Task<AuroraHttpListener?> httpListener, Task<LightingStateManager> lightingStateManager)
    {
        _httpListener = httpListener;
        _lightingStateManager = lightingStateManager;
        InitializeComponent();
    }

    private void DesktopControl_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        SettingsSelected?.Invoke(this, EventArgs.Empty);
    }

    public async Task GenerateProfileStack()
    {
        var lightingStateManager = await _lightingStateManager;
        await GenerateProfileStack(lightingStateManager.CurrentEvent.Config.ID);
    }

    private async Task GenerateProfileStack(string focusedKey)
    {
        var oldCancelSource = _generateStackCancelSource;
        await oldCancelSource.CancelAsync();

        var newCancelSource = new CancellationTokenSource();
        var cancellationToken = newCancelSource.Token;
        _generateStackCancelSource = newCancelSource;
        
        ProfilesStack.Children.Clear();

        var focusedSetTaskCompletion = new TaskCompletionSource();

        var lightingStateManager = await _lightingStateManager;
        var profileLoadTasks = Global.Configuration.ProfileOrder
            .Where(profileName => lightingStateManager.Events.ContainsKey(profileName))
            .Select(profileName => lightingStateManager.Events[profileName])
            .OrderBy(item => item.Settings is { Hidden: false } ? 0 : 1)
            .Select(application => InsertApplicationImage(focusedKey, application, focusedSetTaskCompletion, cancellationToken))
            .Select(x => x.Task);

        var allCompleted = Task.WhenAll(profileLoadTasks);
        await Task.WhenAny(allCompleted, focusedSetTaskCompletion.Task);

        oldCancelSource.Dispose();
    }

    private DispatcherOperation InsertApplicationImage(string focusedKey, Application application,
        TaskCompletionSource focusedSetTaskCompletion, CancellationToken cancellationToken)
    {
        return Dispatcher.BeginInvoke(() =>
        {
            var profileImage = CreateApplication(focusedKey, application);
            var setHidden = application.Settings?.Hidden ?? false;
            profileImage.Visibility = setHidden && !ShowHidden ? Visibility.Collapsed : Visibility.Visible;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            ProfilesStack.Children.Add(profileImage);

            if (!application.Config.ID.Equals(focusedKey)) return;

            Dispatcher.BeginInvoke(() =>
            {
                FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)application.Icon));

                focusedSetTaskCompletion.TrySetResult();
            }, DispatcherPriority.Normal);
        }, DispatcherPriority.Loaded);
    }

    private FrameworkElement CreateApplication(string focusedKey, Application application)
    {
        var ctrl = new Control_ProfileImage(application);
        ctrl.ProfileRemoved += (_, e) =>
        {
            Task.Run(async () => await RemoveApplication(e.Application)).Wait();
        };
        ctrl.ProfileSelected += (_, e) => FocusApplication(e.Application);
        ctrl.MouseDown += ProfileImage_MouseDown;

        if (!application.Config.ID.Equals(focusedKey)) return ctrl;

        FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)application.Icon));
        return ctrl;
    }

    private async Task RemoveApplication(Application application)
    {
        var name = application.Config.ID;

        var lightingStateManager = await _lightingStateManager;
        if (!lightingStateManager.Events.TryGetValue(name, out var value)) return;
        var applicationName = value.Config.ProcessNames[0];

        var cancelled = MessageBox.Show(
            $"Are you sure you want to delete profile for {applicationName}?", "Remove Profile",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes;
        if (cancelled) return;

        lightingStateManager.RemoveGenericProfile(name);

        Dispatcher.InvokeAsync(async () => await GenerateProfileStack());
    }

    private async void AddProfile_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        var dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title = "Add Profile" };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath))
            return; // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

        var filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());

        var lightingStateManager = await _lightingStateManager;
        if (lightingStateManager.Events.ContainsKey(filename))
        {
            MessageBox.Show("Profile for this application already exists.");
            return;
        }

        var genAppPm = new GenericApplication(filename);
        await genAppPm.Initialize(CancellationToken.None);

        var ico = Icon.ExtractAssociatedIcon(dialog.ChosenExecutablePath.ToLowerInvariant());

        if (!Directory.Exists(genAppPm.GetProfileFolderPath()))
            Directory.CreateDirectory(genAppPm.GetProfileFolderPath());

        using (var iconAsbitmap = ico.ToBitmap())
        {
            iconAsbitmap.Save(Path.Combine(genAppPm.GetProfileFolderPath(), "icon.png"), ImageFormat.Png);
        }

        ico.Dispose();

        await lightingStateManager.RegisterEvent(genAppPm);
        await ConfigManager.SaveAsync(Global.Configuration);
        await GenerateProfileStack(filename);
    }

    private void HiddenProfile_MouseDown(object? sender, EventArgs e)
    {
        ShowHidden = !ShowHidden;
    }

    private void ShowHiddenChanged(bool showHidden)
    {
        ProfileHidden.Source = showHidden ? _visible : _notVisible;

        foreach (FrameworkElement ctrl in ProfilesStack.Children)
        {
            if (ctrl is not Control_ProfileImage img) continue;
            var setHidden = img.Application.Settings?.Hidden ?? false;
            img.Visibility = setHidden && !showHidden ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void mbtnHidden_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem btn)
        {
            return;
        }

        if (CmenuProfiles.PlacementTarget is not Image img) return;
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

        if (context.PlacementTarget is not Control_ProfileImage img)
            return;

        context.DataContext = img.Application;
    }

    private void cmbtnOpenBitmapWindow_Clicked(object? sender, RoutedEventArgs e) => Window_BitmapView.Open();
    private void cmbtnOpenHttpDebugWindow_Clicked(object? sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open(_httpListener);

    private async void Control_ProfilesStack_OnLoaded(object sender, RoutedEventArgs e)
    {
        (await _lightingStateManager).EventAdded += LightingStateManagerOnEventAdded;
    }

    private async void Control_ProfilesStack_OnUnloaded(object sender, RoutedEventArgs e)
    {
        (await _lightingStateManager).EventAdded -= LightingStateManagerOnEventAdded;
    }

    private void FocusApplication(Application application)
    {
        FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)application.Icon));
    }

    private void ProfileImage_MouseDown(object? sender, MouseButtonEventArgs? e)
    {
        if (sender is not Control_ProfileImage appControl) return;
        if (e.RightButton != MouseButtonState.Pressed) return;
        CmenuProfiles.PlacementTarget = appControl;
        CmenuProfiles.IsOpen = true;
    }

    private async void LightingStateManagerOnEventAdded(object? sender, EventArgs e)
    {
        await Dispatcher.BeginInvoke(() => GenerateProfileStack());
    }

    private async void CmenuProfiles_OnClosed(object sender, RoutedEventArgs e)
    {
        await Dispatcher.BeginInvoke(() => GenerateProfileStack());
    }
}
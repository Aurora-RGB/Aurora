using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Aurora_Wrapper;
using AuroraRgb.Profiles.Generic_Application;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using Application = AuroraRgb.Profiles.Application;
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

    private readonly BitmapImage _profileRemoveImage = new(new Uri(@"Resources/removeprofile_icon.png", UriKind.Relative));
    private readonly BitmapImage _disabledProfileImage = new(new Uri(@"Resources/disabled.png", UriKind.Relative));
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
            .Select(profileName => (Application)lightingStateManager.Events[profileName])
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
        return CreateGenericApplication(focusedKey, application, application is GenericApplication);
    }

    private FrameworkElement CreateGenericApplication(string? focusedKey, Application application, bool isGeneric)
    {
        var profileImage = GenerateProfileImage(application);

        var profileGrid = new Grid
        {
            Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
            Margin = new Thickness(0, 5, 0, 0),
        };

        profileGrid.Children.Add(profileImage);
        if (isGeneric)
        {
            AddProfileRemoveButton(application, profileGrid);
        }
        
        var profileDisabled = !application.Settings?.IsEnabled ?? false;
        if (profileDisabled)
        {
            AddProfileDisabledImage(application, profileGrid);
        }

        if (!application.Config.ID.Equals(focusedKey)) return profileGrid;

        FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)application.Icon));
        return profileGrid;
    }

    private void AddProfileRemoveButton(Application application, Grid profileGrid)
    {
        var profileRemove = new Image
        {
            Source = _profileRemoveImage,
            ToolTip = $"Remove {application.Config.ProcessNames[0]} Profile",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 16,
            Width = 16,
            Visibility = Visibility.Hidden,
            Tag = application.Config.ID
        };
        profileGrid.Tag = profileRemove;
            
        profileRemove.MouseDown += async (_, _) => { await RemoveApplication(application); };

        profileGrid.MouseEnter += (_, _) =>
        {
            profileRemove.Visibility = Visibility.Visible;
        };
        profileGrid.MouseLeave += (_, _) =>
        {
            profileRemove.Visibility = Visibility.Hidden;
        };

        profileGrid.Children.Add(profileRemove);
    }

    private void AddProfileDisabledImage(Application application, Grid profileGrid)
    {
        var disabledTooltip = application.Settings?.IsOverlayEnabled ?? true ?
            "Profile is disabled. Overlay Layers can still work " :  "Profile is completely disabled";
        var profileDisabledImage = new Image
        {
            Source = _disabledProfileImage,
            ToolTip = disabledTooltip,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 16,
            Width = 16,
        };

        profileGrid.Children.Add(profileDisabledImage);
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

        var eventList = Global.Configuration.ProfileOrder
            .ToDictionary(x => x, x => lightingStateManager.Events[x])
            .Where(x => ShowHidden || ShowProfile(x))
            .ToList();
        var idx = Math.Max(eventList.FindIndex(x => x.Key == name), 0);
        lightingStateManager.RemoveGenericProfile(name);
        await GenerateProfileStack(eventList[idx].Key);

        bool ShowProfile(KeyValuePair<string, ILightEvent> x)
        {
            var application1 = (Application)x.Value;
            if (application1.Settings == null)
            {
                return true;
            }
            return !application1.Settings.Hidden;
        }
    }

    private FrameworkElement GenerateProfileImage(Application application)
    {
        var profileImage = new Image
        {
            Tag = application,
            Source = application.Icon,
            ToolTip = application.Config.Name + " Settings",
            Margin = new Thickness(0, 5, 0, 0),
            Visibility = application.Settings is { Hidden: false } ? Visibility.Visible : Visibility.Collapsed,
        };
        
        profileImage.MouseDown += ProfileImage_MouseDown;
        
        return profileImage;
    }

    private async void AddProfile_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        var dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title = "Add Profile" };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath))
            return; // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

        var filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());

        var lightingStateManager = await _lightingStateManager;
        if (lightingStateManager.Events.TryGetValue(filename, out var lightEvent))
        {
            if (lightEvent is GameEvent_Aurora_Wrapper)
                lightingStateManager.Events.Remove(filename);
            else
            {
                MessageBox.Show("Profile for this application already exists.");
                return;
            }
        }

        var genAppPm = new GenericApplication(filename);
        await genAppPm.Initialize(CancellationToken.None);

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
            var img = ctrl as Image ?? (ctrl is Grid grid ? grid.Children[0] as Image : null);
            if (img?.Tag is not Application profile) continue;

            var setHidden = profile.Settings?.Hidden ?? false;
            img.Visibility = setHidden && !showHidden ? Visibility.Collapsed : Visibility.Visible;
            img.Opacity = setHidden ? 0.5 : 1;
        }
    }

    private async void mbtnHidden_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem btn)
        {
            return;
        }

        if (CmenuProfiles.PlacementTarget is not Image img) return;
        img.Opacity = btn.IsChecked ? 0.5 : 1;

        if (!ShowHidden && btn.IsChecked)
            img.Visibility = Visibility.Collapsed;

        await (img.Tag is Application application ? application.SaveProfiles() : Task.CompletedTask);
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

    private void cmbtnOpenBitmapWindow_Clicked(object? sender, RoutedEventArgs e) => Window_BitmapView.Open();
    private void cmbtnOpenHttpDebugWindow_Clicked(object? sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open(_httpListener);

    private async void Control_ProfilesStack_OnLoaded(object sender, RoutedEventArgs e)
    {
        (await _lightingStateManager).EventAdded += LightingStateManagerOnEventAdded;

        foreach (Image child in ProfilesStack.Children)
        {
            if (child.Visibility != Visibility.Visible) continue;
            ProfileImage_MouseDown(child, null);
            break;
        }
    }

    private async void Control_ProfilesStack_OnUnloaded(object sender, RoutedEventArgs e)
    {
        (await _lightingStateManager).EventAdded -= LightingStateManagerOnEventAdded;
    }

    private void ProfileImage_MouseDown(object? sender, MouseButtonEventArgs? e)
    {
        if (sender is not Image { Tag: Application } image) return;
        if (e == null || e.LeftButton == MouseButtonState.Pressed)
        {
            var application = (Application)image.Tag;
            FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)image.Source));
        }
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            CmenuProfiles.PlacementTarget = image;
            CmenuProfiles.IsOpen = true;
        }
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
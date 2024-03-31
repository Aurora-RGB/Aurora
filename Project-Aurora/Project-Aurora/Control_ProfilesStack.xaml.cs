using System;
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

    public async Task GenerateProfileStack(string? focusedKey = null)
    {
        var lightingStateManager = await _lightingStateManager;
        focusedKey ??= lightingStateManager.CurrentEvent.Config.ID;
        ProfilesStack.Children.Clear();

        var focusedSetTaskCompletion = new TaskCompletionSource();

        var profileLoadTasks = Global.Configuration.ProfileOrder
            .Where(profileName => lightingStateManager.Events.ContainsKey(profileName))
            .Select(profileName => (Application)lightingStateManager.Events[profileName])
            .OrderBy(item => item.Settings is { Hidden: false } ? 0 : 1)
            .Select(application =>
            {
                if (application is GenericApplication)
                {
                    return Dispatcher.BeginInvoke(() => CreateInsertGenericApplication(focusedKey, application), DispatcherPriority.Loaded);
                }

                return Dispatcher.BeginInvoke(() =>
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
                    ProfilesStack.Children.Add(profileImage);

                    if (!application.Config.ID.Equals(focusedKey)) return;

                    Dispatcher.BeginInvoke(() =>
                    {
                        FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)profileImage.Source));

                        focusedSetTaskCompletion.TrySetResult();
                    }, DispatcherPriority.Render);
                }, DispatcherPriority.Loaded);
            }).Select(x => x.Task);

        var allCompleted = Task.WhenAll(profileLoadTasks);
        await Task.WhenAny(allCompleted, focusedSetTaskCompletion.Task);
    }

    private void CreateInsertGenericApplication(string? focusedKey, Application application)
    {
        var settings = application.Settings as GenericApplicationSettings;
        var profileImage = new Image
        {
            Tag = application,
            Source = application.Icon,
            ToolTip = (settings?.ApplicationName ?? "") + " Settings",
            Margin = new Thickness(0, 0, 0, 5)
        };
        profileImage.MouseDown += ProfileImage_MouseDown;

        var profileRemove = new Image
        {
            Source = new BitmapImage(new Uri(@"Resources/removeprofile_icon.png", UriKind.Relative)),
            ToolTip = $"Remove {(settings?.ApplicationName ?? "")} Profile",
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

        ProfilesStack.Children.Add(profileGrid);

        if (!application.Config.ID.Equals(focusedKey)) return;

        FocusedAppChanged?.Invoke(this, new FocusedAppChangedEventArgs(application, (BitmapSource)profileImage.Source));
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
        await Dispatcher.BeginInvoke(GenerateProfileStack);
    }
}
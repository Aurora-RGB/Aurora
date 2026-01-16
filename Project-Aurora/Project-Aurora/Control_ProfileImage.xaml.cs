using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Dota_2;
using AuroraRgb.Profiles.Generic_Application;
using Application = AuroraRgb.Profiles.Application;
using EventArgs = System.EventArgs;

namespace AuroraRgb;

public class ProfileEventArgs(Application application) : EventArgs
{
    public Application Application { get; } = application;
}

public sealed partial class Control_ProfileImage : IDisposable, IAsyncDisposable
{
    private readonly Control_ProfilesStack _profilesStack;
    private readonly bool _isGenericApplication;
    private readonly LightingStateManager _lightingStateManager;
    public event EventHandler<ProfileEventArgs>? ProfileSelected; 
    public event EventHandler<ProfileEventArgs>? ProfileRemoved; 

    public Application Application { get; }

    public Control_ProfileImage()
    {
        DataContext = new Dota2();
        
        InitializeComponent();

        _isGenericApplication = false;
        var appHidden = false;
        var profileDisabled = true;
        var overlayEnabled = true;

        Image.Opacity = appHidden ? 0.5 : 1;

        if (profileDisabled)
        {
            var disabledTooltip = overlayEnabled ?
                "Profile is disabled. Global Layers can still work\nRight click to change" :
                "Profile is completely disabled\nRight click to change";
            IsDisabledButton.Visibility = Visibility.Visible;
            IsDisabledButton.ToolTip = disabledTooltip;
        }
        else
        {
            IsDisabledButton.Visibility = Visibility.Collapsed;
        }
        
        RemoveButton.Visibility = Visibility.Visible;
    }

    public Control_ProfileImage(Application application, Control_ProfilesStack profilesStack, LightingStateManager lightingStateManager)
    {
        _lightingStateManager = lightingStateManager;
        _profilesStack = profilesStack;
        Application = application;

        DataContext = this;
        
        InitializeComponent();

        _isGenericApplication = application is GenericApplication;
        var appHidden = application.Settings?.Hidden ?? false;
        var profileDisabled = !application.Settings?.IsEnabled ?? false;
        var overlayEnabled = application.Settings?.IsOverlayEnabled ?? true;

        Image.Opacity = appHidden ? 0.5 : 1;
        ToolTip = application.Config.Name + " Settings\n\nRight click to enable/disable the profile";

        if (profileDisabled)
        {
            var disabledTooltip = overlayEnabled ?
                "Profile is disabled. Global Layers can still work\nRight click to change" :
                "Profile is completely disabled\nRight click to change";
            IsDisabledButton.Visibility = Visibility.Visible;
            IsDisabledButton.ToolTip = disabledTooltip;
        }
        else
        {
            IsDisabledButton.Visibility = Visibility.Collapsed;
        }
    }

    private void ProfileImage_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            ProfileSelected?.Invoke(this, new ProfileEventArgs(Application));
        }
    }

    private void Control_ProfileImage_OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (_isGenericApplication)
        {
            RemoveButton.Visibility = Visibility.Visible;
        }
    }

    private void Control_ProfileImage_OnMouseLeave(object sender, MouseEventArgs e)
    {
        RemoveButton.Visibility = Visibility.Hidden;
    }

    private void RemoveButton_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        ProfileRemoved?.Invoke(this, new ProfileEventArgs(Application));
    }

    public void Dispose()
    {
        Image.Source = null;
        _profilesStack.FocusedAppChanged -= OnFocusedAppChanged;
    }

    public async ValueTask DisposeAsync()
    {
        await Application.DisposeAsync();
    }
    
    private bool AppIsFocused => _lightingStateManager.ApplicationManager.GetCurrentProfile() == Application;
    private bool OverlayIsEnabled => _lightingStateManager.ApplicationManager.OverlayActiveProfiles.Contains(Application.Config.ID);
    
    private Brush GetIndicatorBackground()
    {
        if (AppIsFocused)
            return Brushes.Green;
        if (OverlayIsEnabled)
            return Brushes.RoyalBlue;
        return Brushes.Transparent;
    }

    private void Control_ProfileImage_OnLoaded(object sender, RoutedEventArgs e)
    {
        _profilesStack.FocusedAppChanged += OnFocusedAppChanged;
        _lightingStateManager.ApplicationManager.OverlayProfilesChanged += ApplicationManagerOnOverlayProfilesChanged;
        UpdateIndicatorBackground();
    }

    private void Control_ProfileImage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _profilesStack.FocusedAppChanged -= OnFocusedAppChanged;
        _lightingStateManager.ApplicationManager.OverlayProfilesChanged -= ApplicationManagerOnOverlayProfilesChanged;
    }

    private void OnFocusedAppChanged(object? sender, FocusedAppChangedEventArgs e)
    {
        UpdateIndicatorBackground();
    }

    private void ApplicationManagerOnOverlayProfilesChanged(object? sender, EventArgs e)
    {
        UpdateIndicatorBackground();
    }

    private void UpdateIndicatorBackground()
    {
        Dispatcher.InvokeAsync(() =>
        {
            FocusIndicator.Background = GetIndicatorBackground();
        }, DispatcherPriority.DataBind);
    }
}
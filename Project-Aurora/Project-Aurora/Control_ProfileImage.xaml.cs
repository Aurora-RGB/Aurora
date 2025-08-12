﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
    private readonly bool _isGenericApplication;
    public event EventHandler<ProfileEventArgs>? ProfileSelected; 
    public event EventHandler<ProfileEventArgs>? ProfileRemoved; 

    public Application Application { get; }

    public Control_ProfileImage() : this(new Dota2())
    {
        DataContext = this;
        InitializeComponent();
        RemoveButton.Visibility = Visibility.Visible;
    }

    public Control_ProfileImage(Application application)
    {
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
    }

    public async ValueTask DisposeAsync()
    {
        await Application.DisposeAsync();
    }
}
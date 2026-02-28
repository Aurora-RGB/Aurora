using System;
using System.ComponentModel;
using System.Windows;
using AuroraRgb.Utils;

namespace AuroraRgb.Controls;

public sealed partial class WindowFirstTime : IDisposable
{
    private readonly TransparencyComponent _transparencyComponent;
    private readonly WindowFirstTimeViewModel _viewModel;

    private bool _isClosing;

    public WindowFirstTime()
    {
        InitializeComponent();
        _viewModel = (WindowFirstTimeViewModel)DataContext;

        _transparencyComponent = new TransparencyComponent(this, null);
    }

    public void Dispose()
    {
        _transparencyComponent.Dispose();
    }

    private void WindowFirstTime_OnLoaded(object sender, RoutedEventArgs e)
    {
        CheckFirstTime();
        CheckUnsafeDrivers();
        CheckPawnIo();
        CheckGsiSetting();
    }

    private void CheckFirstTime()
    {
        var isFirstTime = Global.Configuration.AutoInstallGsi == null;
        if (!isFirstTime)
        {
            FirstTimePanel.Visibility = Visibility.Collapsed;
        }
    }

    private void CheckUnsafeDrivers()
    {
        var hasUnsecureDriver = UnsecureDrivers.InpOutDriverExists() || UnsecureDrivers.WinRing0DriverExists();
        if (!hasUnsecureDriver)
        {
            UnsafeDriversPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void CheckPawnIo()
    {
        var hasPawnIo = ControlPawnIo.PawnIoDriverExists();
        if (hasPawnIo)
        {
            PawnIoPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void CheckGsiSetting()
    {
        var gsiUnset = Global.Configuration.AutoInstallGsi == null;
        if (!gsiUnset)
        {
            IntegrationsPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void WindowFirstTime_OnClosing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.CanContinue || _isClosing)
        {
            return;
        }

        const string exitText = "You haven't completed the setup yet. Are you sure you want to cancel?\nAurora-RGB will close if you click Yes.";
        var result = MessageBox.Show(this, exitText, "Exit Setup", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            _isClosing = true;
            Application.Current.Shutdown();
        }

        e.Cancel = true;
    }
}
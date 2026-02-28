using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using AuroraRgb.Utils;
using Microsoft.Win32;

namespace AuroraRgb.Controls;

public partial class ControlPawnIo
{
    private const string PawnIoReg = @"SYSTEM\CurrentControlSet\Services\PawnIO";
    private readonly RegistryWatcher _registryWatcher = new(RegistryHiveOpt.LocalMachine, PawnIoReg, "ImagePath");
    
    public ControlPawnIo()
    {
        InitializeComponent();
    }

    private void ControlPawnIo_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        _registryWatcher.RegistryChanged += RegistryWatcherOnRegistryChanged;
        _registryWatcher.StartWatching();
        UpdateInpOutStatus();
    }

    private void ControlPawnIo_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _registryWatcher.RegistryChanged -= RegistryWatcherOnRegistryChanged;
        _registryWatcher.StopWatching();
    }

    private void RegistryWatcherOnRegistryChanged(object? sender, RegistryChangedEventArgs e)
    {
        UpdateInpOutStatus();
    }

    private void PawnIoLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start("explorer", e.Uri.AbsoluteUri);
        e.Handled = true;
    }
    

    private void UpdateInpOutStatus()
    {
        var pawnIoExists = PawnIoDriverExists();
        if (pawnIoExists)
        {
            PawnIoStatus.Foreground = Brushes.Green;
            PawnIoStatus.Text = "Installed";
        }
        else
        {
            PawnIoStatus.Foreground = Brushes.Red;
            PawnIoStatus.Text = "Not installed";
        }
    }
    
    public static bool PawnIoDriverExists()
    {
        using var r = Registry.LocalMachine.OpenSubKey(PawnIoReg);
        return r != null;
    }
}
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuroraRgb.Controls;

public partial class WindowFirstTimeViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AutoGsiYes))]
    [NotifyPropertyChangedFor(nameof(AutoGsiNo))]
    [NotifyPropertyChangedFor(nameof(CanContinue))]
    [NotifyPropertyChangedFor(nameof(CanContinueVisibility))]
    private bool? _autoGsiEnabled = Global.Configuration.AutoInstallGsi;

    public bool AutoGsiYes => AutoGsiEnabled == true;
    public bool AutoGsiNo => AutoGsiEnabled == false;

    public bool CanContinue => AutoGsiEnabled != null;
    public Visibility CanContinueVisibility => CanContinue ? Visibility.Visible : Visibility.Collapsed;

    [RelayCommand]
    private void SetAutoGsiYes(string? value)
    {
        SetAutoGsiEnabled(true);
    }

    [RelayCommand]
    private void SetAutoGsiNo(string? value)
    {
        SetAutoGsiEnabled(false);
    }

    private void SetAutoGsiEnabled(bool value)
    {
        AutoGsiEnabled = value;

        // Save to settings
        Global.Configuration.AutoInstallGsi = value;
    }
}
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AuroraRgb.Settings;

public class ApplicationSettings : INotifyPropertyChanged
{
    public bool IsEnabled { get; set; } = true;
    public bool IsOverlayEnabled { get; set; } = true;
    public bool Hidden { get; set; } = false;
    public string SelectedProfile { get; set; } = "default";

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual bool InstallationCompleted => true;

    public virtual void CompleteInstallation()
    {
    }
}

[method: JsonConstructor]
public class FirstTimeApplicationSettings(bool isFirstTimeInstalled) : ApplicationSettings
{
    public bool IsFirstTimeInstalled { get; private set; } = isFirstTimeInstalled;

    public override bool InstallationCompleted => IsFirstTimeInstalled;
    public override void CompleteInstallation()
    {
        IsFirstTimeInstalled = true;
    }
}

[method: JsonConstructor]
public class NewJsonApplicationSettings(bool isFirstTimeInstalled) : ApplicationSettings
{
    public bool IsNewJsonInstalled { get; private set; } = isFirstTimeInstalled;

    public override bool InstallationCompleted => IsNewJsonInstalled;
    public override void CompleteInstallation()
    {
        IsNewJsonInstalled = true;
    }
}
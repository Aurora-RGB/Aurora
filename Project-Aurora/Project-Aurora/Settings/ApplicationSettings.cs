using System.ComponentModel;
using System.Text.Json.Serialization;
using AuroraRgb.Profiles.Generic_Application;

namespace AuroraRgb.Settings;

[JsonSerializable(typeof(ApplicationSettings))]
[JsonSerializable(typeof(FirstTimeApplicationSettings))]
[JsonSerializable(typeof(NewJsonApplicationSettings))]
[JsonSerializable(typeof(GenericApplicationSettings))]
[JsonSerializable(typeof(PluginManagerSettings))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class SettingsJsonContext : JsonSerializerContext;

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

public class FirstTimeApplicationSettings : ApplicationSettings
{
    public bool IsFirstTimeInstalled { get; private set; }

    public override bool InstallationCompleted => IsFirstTimeInstalled;

    public FirstTimeApplicationSettings()
    {
    }

    [method: JsonConstructor]
    public FirstTimeApplicationSettings(bool isFirstTimeInstalled)
    {
        IsFirstTimeInstalled = isFirstTimeInstalled;
    }

    public override void CompleteInstallation()
    {
        IsFirstTimeInstalled = true;
    }
}

public class NewJsonApplicationSettings : ApplicationSettings
{
    public bool IsNewJsonInstalled { get; private set; }

    public override bool InstallationCompleted => IsNewJsonInstalled;

    public NewJsonApplicationSettings()
    {
    }

    [method: JsonConstructor]
    public NewJsonApplicationSettings(bool isNewJsonInstalled)
    {
        IsNewJsonInstalled = isNewJsonInstalled;
    }
    
    public override void CompleteInstallation()
    {
        IsNewJsonInstalled = true;
    }
}
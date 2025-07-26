using System.ComponentModel;
using System.IO;

namespace AuroraRgb.Settings;

public class SensitiveData : INotifyPropertyChanged
{
    public static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, "Sensitive.json");

    public event PropertyChangedEventHandler? PropertyChanged;

    public double Lat { get; set; }
    public double Lon { get; set; }
    public string ObsWebSocketPassword { get; set; } = "";
}
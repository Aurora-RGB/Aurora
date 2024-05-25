using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers;
using Debounce.Core;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PropertyChanged;

namespace AuroraRgb.Settings;

public class ScriptSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    [OnChangedMethod(nameof(OnEnabledChanged))] public bool Enabled { get; set; }
    [JsonIgnore] public bool ExceptionHit { get; set; }
    [JsonIgnore] public Exception Exception { get; set; }

    private void OnEnabledChanged()
    {
        if (!Enabled) return;
        ExceptionHit = false;
        Exception = null;
    }
}

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ApplicationProfile : INotifyPropertyChanged, IDisposable
{
    private static int _backupFileNumber;

    public string ProfileName { get; set; }
    public Keybind TriggerKeybind { get; set; }
    [JsonIgnore] public string ProfileFilepath { get; set; }
    public Dictionary<string, ScriptSettings> ScriptSettings { get; set; }
    public ObservableCollection<Layer> Layers { get; set; }
    public ObservableCollection<Layer> OverlayLayers { get; set; }

    private string? _savePath;
    private Debouncer _saveDebouncer;


    protected ApplicationProfile()
    {
        _saveDebouncer = new Debouncer(() =>
        {
            SaveProfile().Wait();
        }, 850);
        
        Reset();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void Reset()
    {
        Layers = [];
        OverlayLayers = [];
        ScriptSettings = new Dictionary<string, ScriptSettings>();
        TriggerKeybind = new Keybind();
    }

    public void SetApplication(Application app)
    {
        foreach (var l in Layers)
            l.SetProfile(app);

        foreach (var l in OverlayLayers)
            l.SetProfile(app);
    }

    public void SaveProfile(string path)
    {
        _savePath = path;
        _saveDebouncer.Debounce();
    }

    private async Task SaveProfile()
    {
        try
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var content = JsonConvert.SerializeObject(this, Formatting.Indented, settings);

            Directory.CreateDirectory(Path.GetDirectoryName(_savePath!));
            if (File.Exists(_savePath!))
            {
                var backupFile = _savePath! + ".bk" + _backupFileNumber++;
                // move current save to preserve it
                File.Move(_savePath!, backupFile);
                // write new save to supposed location
                await File.WriteAllTextAsync(_savePath!, content, Encoding.UTF8);
                // write successful, delete backup
                File.Delete(backupFile);
            }
            else
            {
                await File.WriteAllTextAsync(_savePath!, content, Encoding.UTF8);
            }
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception Saving Profile: {Path}", _savePath!);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        foreach (var l in Layers)
            l.Dispose();

        foreach (var l in OverlayLayers)
            l.Dispose();
    }
}
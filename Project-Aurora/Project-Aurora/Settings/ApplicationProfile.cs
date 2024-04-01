using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers;
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
    public string ProfileName { get; set; }
    public Keybind TriggerKeybind { get; set; }
    [JsonIgnore] public string ProfileFilepath { get; set; }
    public Dictionary<string, ScriptSettings> ScriptSettings { get; set; }
    public ObservableCollection<Layer> Layers { get; set; }
    public ObservableCollection<Layer> OverlayLayers { get; set; }

    protected ApplicationProfile()
    {
        Reset();
    }

    public event PropertyChangedEventHandler PropertyChanged;

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
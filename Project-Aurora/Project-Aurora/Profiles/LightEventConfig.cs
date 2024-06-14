using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using AuroraRgb.Profiles.Generic;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles;

public class LightEventConfig : INotifyPropertyChanged
{
    private readonly Temporary<LightEvent> _lightEvent;
    public string[] ProcessNames
    {
        get => _processNames;
        set
        {
            _processNames = value.Select(s => s.ToLower()).ToArray();
            ProcessNamesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler<EventArgs>? ProcessNamesChanged;

    /// <summary>One or more REGULAR EXPRESSIONS that can be used to match the title of an application</summary>
    public Regex[]? ProcessTitles { get; init; }

    public string Name { get; init; } = "Unset";

    public string ID { get; init; } = DateTime.Now.ToString(CultureInfo.InvariantCulture);

    public string AppID { get; init; } = string.Empty;

    public Type SettingsType { get; init; } = typeof(ApplicationSettings);

    public Type ProfileType { get; init; } = typeof(ApplicationProfile);

    public Type OverviewControlType { get; init; } = typeof(UserControl);

    public Type? GameStateType { get; init; }

    public Application? Application { get; set; }

    private string[] _processNames = [];
    public LightEvent Event => _lightEvent.Value;

    public string IconURI { get; init; } = "UNSET ICON";

    public HashSet<Type> ExtraAvailableLayers { get; } = [];

    public bool EnableByDefault { get; init; } = true;
    public bool EnableOverlaysByDefault { get; set; } = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LightEventConfig() : this(() => new GameEvent_Generic())
    {
    }

    public LightEventConfig(Func<LightEvent> lightEvent)
    {
        _lightEvent = new (() =>
        {
            var evLightEvent = lightEvent.Invoke();
            evLightEvent.Application = Application;
            evLightEvent.ResetGameState(GameStateType);
            return evLightEvent;
        });
    }

    public LightEventConfig WithLayer<T>() where T : ILayerHandler {
        ExtraAvailableLayers.Add(typeof(T));
        return this;
    }
}
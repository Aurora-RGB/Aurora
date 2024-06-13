using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using AuroraRgb.Vorons;
using Common.Utils;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using JetBrains.Annotations;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EnumConverter = AuroraRgb.Utils.EnumConverter;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace AuroraRgb.Profiles;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public class Application : ObjectSettings<ApplicationSettings>, ILightEvent, INotifyPropertyChanged
{
    #region Public Properties
    public bool Initialized { get; private set; }
    public bool Disposed { get; private set; }
    public ApplicationProfile? Profile { get; private set; }
    public ObservableCollection<ApplicationProfile> Profiles { get; set; }
    public GameStateParameterLookup? ParameterLookup { get; }
    public event EventHandler? ProfileChanged;
    public LightEventConfig Config { get; }
    public bool IsEnabled => Initialized && Settings.IsEnabled && !Disposed;
    public bool IsOverlayEnabled => Initialized && Settings.IsOverlayEnabled && !Disposed;

    #endregion

    #region Internal Properties
    protected ImageSource? icon;
    public virtual ImageSource Icon => icon ??= new BitmapImage(new Uri(GetBaseUri(), @"/AuroraRgb;component/" + Config.IconURI));

    private UserControl? _control;
    public UserControl Control => _control ??= (UserControl)Activator.CreateInstance(Config.OverviewControlType, this);

    internal Dictionary<string, IEffectScript> EffectScripts { get; } = new();
    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    private static Uri GetBaseUri()
    {
        if (System.Windows.Application.Current.MainWindow is ConfigUi mainWindow)
        {
            return mainWindow.BaseUri;
        }

        throw new ThreadStateException("Application.Current.MainWindow is null");
    }

    protected Application(LightEventConfig config)
    {
        Config = config;
        SettingsSavePath = Path.Combine(GetProfileFolderPath(), "settings.json");
        config.Application = this;
        config.Event.ResetGameState(config.GameStateType);
        Profiles = new ObservableCollection<ApplicationProfile>();
        Profiles.CollectionChanged += (_, e) =>
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (ApplicationProfile prof in e.NewItems)
            {
                prof.SetApplication(this);
            }
        };
        if (config.GameStateType != null)
            ParameterLookup = new GameStateParameterLookup(config.GameStateType);

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            TypeNameHandling = TypeNameHandling.Auto,
            FloatParseHandling = FloatParseHandling.Double,
            SerializationBinder = _binder,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Error = LoadProfilesError,
            Converters = [
                new ObservableCollectionJsonConverter(),
                new EnumConverter(),
                new SingleToDoubleConverter(),
                new OverrideTypeConverter(),
                new TypeAnnotatedObjectConverter(),
                new ObjectDictionaryJsonConverterAdapter(),
                new StringDictionaryJsonConverterAdapter(),
                new SingleDictionaryJsonConverterAdapter(),
                new DoubleDictionaryJsonConverterAdapter<dynamic>(),
                new SortedDictionaryAdapter(),
                new VariableRegistryDictionaryConverter(),
                new UltimateListJsonConverter(),
            ],
        };

        _serializer = JsonSerializer.Create(jsonSerializerSettings);
    }

    public virtual async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        if (Initialized)
            return Initialized;

        await LoadSettings(Config.SettingsType);
        await LoadProfiles(cancellationToken);
        Initialized = true;
        return Initialized;
    }

    protected override void SettingsCreateHook() {
        Settings.IsEnabled = Config.EnableByDefault;
        Settings.IsOverlayEnabled = Config.EnableOverlaysByDefault;
    }

    /// <summary>Enables the use of a non-default layer for this application.</summary>
    protected void AllowLayer<T>() where T : ILayerHandler => Config.WithLayer<T>();

    /// <summary>Determines if the given layer handler type can be used by this application.
    /// This is the case either if it is a default handler or has explicitly been allowed for this application.</summary>
    public bool IsAllowedLayer(Type type) => Global.LightingStateManager.LayerHandlers.TryGetValue(type, out var def) &&
                                             (def.IsDefault || Config.ExtraAvailableLayers.Contains(type));

    /// <summary>Gets a list of layers that are allowed to be used by this application.</summary>
    public IEnumerable<LayerHandlerMeta> AllowedLayers
        => Global.LightingStateManager.LayerHandlers.Values.Where(val => val.IsDefault || Config.ExtraAvailableLayers.Contains(val.Type));

    public void SwitchToProfile(ApplicationProfile? newProfileSettings)
    {
        if (Disposed)
            return;

        if (newProfileSettings == null || Profile == newProfileSettings) return;
        if (Profile != null)
        {
            SaveProfile();
            Profile.PropertyChanged -= Profile_PropertyChanged;
        }

        Profile = newProfileSettings;
        Settings.SelectedProfile = Path.GetFileNameWithoutExtension(Profile.ProfileFilepath);
        Profile.PropertyChanged += Profile_PropertyChanged;

        App.Current.Dispatcher.BeginInvoke(() => ProfileChanged?.Invoke(this, EventArgs.Empty));
    }

    protected virtual ApplicationProfile CreateNewProfile(string profileName)
    {
        var profile = (ApplicationProfile)Activator.CreateInstance(Config.ProfileType);
        profile.ProfileName = profileName;
        profile.ProfileFilepath = Path.Combine(GetProfileFolderPath(), GetUnusedFilename(GetProfileFolderPath(), profile.ProfileName) + ".json");
        return profile;
    }

    private void AddDefaultProfile()
    {
        AddNewProfile("default");
    }

    public void AddNewProfile()
    {
        AddNewProfile($"Profile {Profiles.Count + 1}");
    }

    public ApplicationProfile? AddNewProfile(string profileName)
    {
        if (Disposed)
            return null;

        var newProfile = CreateNewProfile(profileName);

        Profiles.Add(newProfile);

        SaveProfiles();

        SwitchToProfile(newProfile);

        return newProfile;
    }

    public void DeleteProfile(ApplicationProfile? profile)
    {
        if (Disposed)
            return;

        if (Profiles.Count == 1)
            return;

        if (profile == null || string.IsNullOrWhiteSpace(profile.ProfileFilepath)) return;
        var profileIndex = Profiles.IndexOf(profile);

        if (Profiles.Contains(profile))
            Profiles.Remove(profile);

        if (profile.Equals(Profile))
            SwitchToProfile(Profiles[Math.Min(profileIndex, Profiles.Count - 1)]);

        if (!File.Exists(profile.ProfileFilepath)) return;
        try
        {
            File.Delete(profile.ProfileFilepath);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not delete profile with path \"{ProfileFilepath}\"", profile.ProfileFilepath);
        }
    }

    private string GetValidFilename(string filename)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            filename = filename.Replace(c, '_');

        return filename;
    }

    protected string GetUnusedFilename(string dir, string filename) {
        var safeName = GetValidFilename(filename);
        if (!File.Exists(Path.Combine(dir, safeName + ".json"))) return safeName;
        var i = 0;
        while (File.Exists(Path.Combine(dir, safeName + "-" + ++i + ".json")));
        return safeName + "-" + i;
    }

    public virtual string GetProfileFolderPath()
    {
        return Path.Combine(Global.AppDataDirectory, "Profiles", Config.ID);
    }

    public void ResetProfile()
    {
        if (Disposed)
            return;

        try
        {
            Profile?.Reset();

            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception Resetting Profile");
        }
    }

    //hacky fix to sort out MoD profile type change
    private readonly ISerializationBinder _binder = new AuroraSerializationBinder();
    private readonly JsonSerializer _serializer;

    private async Task<ApplicationProfile?> LoadProfile(string path)
    {
        if (Disposed)
            return null;

        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var profileFile = await File.ReadAllTextAsync(path);
            await using var jsonTextReader = new JsonTextReader(new StringReader(profileFile));

            if (_serializer.Deserialize(jsonTextReader, Config.ProfileType) is not ApplicationProfile prof)
            {
                return null;
            }

            prof.ProfileFilepath = path;

            if (string.IsNullOrWhiteSpace(prof.ProfileName))
                prof.ProfileName = Path.GetFileNameWithoutExtension(path);

            // Call the above setup method on the regular layers and the overlay layers.
            InitialiseLayerCollection(prof.Layers);
            InitialiseLayerCollection(prof.OverlayLayers);

            prof.PropertyChanged += Profile_PropertyChanged;
            return prof;

            // Initializes a collection, setting the layers' profile/application property and adding events to them and the collections to save to disk.
            void InitialiseLayerCollection(ObservableCollection<Layer> collection)
            {
                foreach (var lyr in collection.ToList())
                {
                    //Remove any Layers that have non-functional handlers
                    if (!Global.LightingStateManager.LayerHandlers.ContainsKey(lyr.Handler.GetType()))
                    {
                        prof.Layers.Remove(lyr);
                        continue;
                    }

                    WeakEventManager<Layer, PropertyChangedEventArgs>.AddHandler(lyr, nameof(lyr.PropertyChanged), async (_, e) =>
                    {
                        if (e.PropertyName == nameof(Layer.Error))
                        {
                            return;
                        }
                        SaveProfile(prof, path);
                    });
                }

                collection.CollectionChanged += async (_, e) =>
                {
                    SaveProfile(prof, path);
                    if (e.NewItems == null)
                    {
                        return;
                    }

                    foreach (Layer lyr in e.NewItems)
                        if (lyr != null)
                            WeakEventManager<Layer, PropertyChangedEventArgs>.AddHandler(lyr, nameof(lyr.PropertyChanged),
                                (_, _) =>
                                {
                                    SaveProfile(prof, path);
                                });
                };
            }
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception Loading Profile: {Path}", path);
            if (Path.GetFileNameWithoutExtension(path).Equals("default"))
            {
                var newPath = path + ".corrupted";

                var copy = 1;
                while (File.Exists(newPath))
                {
                    newPath = path + $"({copy++}).corrupted";
                }

                MessageBox.Show($"Default profile for {Config.Name} could not be loaded.\nMoved to {newPath}, reset to default settings.\nException={exc.Message}",
                    "Error loading default profile", MessageBoxButton.OK, MessageBoxImage.Error);
                File.Move(path, newPath);
            }
        }

        return null;
    }

    private void LoadProfilesError(object? sender, ErrorEventArgs e)
    {
        if (e.CurrentObject != null)
        {
            if (e.CurrentObject.GetType() == typeof(Layer) && e.ErrorContext.Member.Equals("Handler"))
            {
                ((Layer)e.ErrorContext.OriginalObject).Handler = new DefaultLayerHandler();
                e.ErrorContext.Handled = true;
            }
        } else if (e.ErrorContext.Path.Equals("$type") && e.ErrorContext.Member == null)
        {
            MessageBox.Show($"The profile type for {Config.Name} has been changed, your profile will be reset and your old one moved to have the extension '.corrupted', ignore the following error",
                "Profile type changed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Profile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ApplicationProfile profile)
            SaveProfile(profile);
    }

    private bool RegisterEffect(string key, IEffectScript obj)
    {
        if (Disposed)
            return false;

        if (EffectScripts.TryAdd(key, obj)) return true;
        Global.logger.Warning("Effect script with key {Key} already exists!", key);
        return false;

    }

    public virtual void UpdateLights(EffectFrame frame)
    {
        if (Disposed)
            return;

        Config.Event.UpdateLights(frame);
    }

    public virtual void UpdateOverlayLights(EffectFrame frame) {
        if (Disposed) return;
        Config.Event.UpdateOverlayLights(frame);
    }

    public virtual void SetGameState(IGameState state)
    {
        if (Disposed)
            return;

        Config.Event.SetGameState(state);
    }

    public virtual void ResetGameState(Type? gameStateType = null)
    {
        if (Disposed)
            return;

        Config.Event.ResetGameState();
    }
        
    public virtual void OnStart()
    {
        if (Disposed)
            return;

        Config.Event.OnStart();
    }

    public virtual void OnStop()
    {
        if (Disposed)
            return;

        Config.Event.OnStop();
    }

    private void LoadScripts(string profilesPath, bool force = false)
    {
        if (!force && EffectScripts.Count != 0)
            return;

        EffectScripts.Clear();
        var voronsScriptPerf = new PerformanceEffect();
        var voronsScriptPing = new PingEffect();
        RegisterEffect(voronsScriptPerf.ID, voronsScriptPerf);
        RegisterEffect(voronsScriptPing.ID, voronsScriptPing);

        var scriptsPath = Path.Combine(profilesPath, Global.ScriptDirectory);
        if (!Directory.Exists(scriptsPath))
            Directory.CreateDirectory(scriptsPath);

        foreach (var script in Directory.EnumerateFiles(scriptsPath, "*.*"))
        {
            var pythonEngine = new Lazy<ScriptEngine>(Python.CreateEngine);
            try
            {
                var ext = Path.GetExtension(script);
                var anyLoaded = false;
                switch (ext)
                {
                    case ".py":
                        var scope = pythonEngine.Value.ExecuteFile(script);
                        foreach (var v in scope.GetItems())
                        {
                            if (v.Value is not PythonType type) continue;
                            var typ = type.__clrtype__();
                            if (typ.IsInterface || !typeof(IEffectScript).IsAssignableFrom(typ)) continue;
                            if (pythonEngine.Value.Operations.CreateInstance(v.Value) is IEffectScript obj)
                            {
                                if (!(obj.ID != null && RegisterEffect(obj.ID, obj)))
                                    Global.logger.Warning("Script \"{Script}\" must have a unique string ID variable for the effect {VKey}", script, v.Key);
                                else
                                    anyLoaded = true;
                            }
                            else
                                Global.logger.Error("Could not create instance of Effect Script: {VKey} in script: \"{Script}\"", v.Key, script);
                        }
                        break;
                    case ".cs":
                        new PluginCompiler(Global.logger, Global.ExecutingDirectory)
                            .Compile(script)
                            .Wait();

                        var scriptAssembly = Assembly.LoadFrom(script + ".dll");
                        var effectType = typeof(IEffectScript);
                        foreach (var typ in scriptAssembly.ExportedTypes)
                        {
                            if (!effectType.IsAssignableFrom(typ)) continue;
                            var obj = (IEffectScript)Activator.CreateInstance(typ);
                            if (!(obj.ID != null && RegisterEffect(obj.ID, obj)))
                                Global.logger.Warning("Script {Script} must have a unique string ID variable for the effect {FullName}",
                                    script, typ.FullName);
                            else
                                anyLoaded = true;
                        }

                        break;
                    case ".dll":
                        break;
                    default:
                        Global.logger.Warning("Script with path {Script} has an unsupported type/ext! ({Ext})", script, ext);
                        continue;
                }

                if (!anyLoaded)
                    Global.logger.Warning("Script \"{Script}\": No compatible effects found. Does this script need to be updated?", script);
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "An error occured while trying to load script {Script}", script);
                //Maybe MessageBox info dialog could be included.
            }
            if (pythonEngine.IsValueCreated)
            {
                pythonEngine.Value.Runtime.Shutdown();
            }
        }
    }

    public void ForceScriptReload() {
        LoadScripts(GetProfileFolderPath(), true);
    }

    private void InitializeScriptSettings(ApplicationProfile profileSettings, bool ignoreRemoval = false)
    {
        foreach (var id in EffectScripts.Keys.Where(id => !profileSettings.ScriptSettings.ContainsKey(id)))
        {
            profileSettings.ScriptSettings.Add(id, new ScriptSettings());
        }

        if (ignoreRemoval) return;
        foreach (var key in profileSettings.ScriptSettings.Keys.Where(s => !EffectScripts.ContainsKey(s)).ToList())
        {
            profileSettings.ScriptSettings.Remove(key);
        }
    }

    private async Task LoadProfiles(CancellationToken cancellationToken)
    {
        var profilesPath = GetProfileFolderPath();

        if (Directory.Exists(profilesPath))
        {
            LoadScripts(profilesPath);

            var profileFiles = Directory.EnumerateFiles(profilesPath, "*.json", SearchOption.TopDirectoryOnly)
                .OrderBy(filePath =>
                {
                    var profileFilename = Path.GetFileNameWithoutExtension(filePath);
                    return !profileFilename.Equals(Settings?.SelectedProfile);
                });
            foreach (var profile in profileFiles)
            {
                var profileFilename = Path.GetFileNameWithoutExtension(profile);
                if (profileFilename.Equals(Path.GetFileNameWithoutExtension(SettingsSavePath)))
                    continue;

                var selectedProfile = profileFilename.Equals(Settings?.SelectedProfile);
                if (selectedProfile)
                {
                    await LoadProfileFile(profile, selectedProfile);
                }
                else
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(async () =>
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    {
                        await Task.Delay(4000, cancellationToken);
                        await LoadProfileFile(profile, selectedProfile);
                    }, cancellationToken);
                }
            }
        }
        else
        {
            Global.logger.Information("Profiles directory for {ConfigName} does not exist", Config.Name);
        }

        if (Profile == null)
            SwitchToProfile(Profiles.FirstOrDefault());
        else
            Settings.SelectedProfile = Path.GetFileNameWithoutExtension(Profile.ProfileFilepath);

        if (Profile == null)
            AddDefaultProfile();
    }

    private async Task LoadProfileFile(string profilePath, bool selectedProfile)
    {
        var profileDir = Path.GetDirectoryName(profilePath);
        var profileFileName = Path.GetFileName(profilePath);
        // find auto-backup save files also ending with .json.bkX
        var saveProfileFile = Directory.EnumerateFiles(profileDir, profileFileName + "*")
            .Where(path => path.EndsWith(".json") || path.Contains(".json.bk"))
            .Where(path => !path.EndsWith(".corrupted"))
            .Order()
            .Last();
        var profileSettings = await LoadProfile(saveProfileFile);
        
        // means save with .bkX extension is found. Load that file
        if (profilePath != saveProfileFile)
        {
            File.Delete(profilePath);
            File.Move(saveProfileFile, profilePath);
        }

        // find extra auto-backup save files ending with .bkX and delete them
        foreach (var extraFailedSaves in Directory.EnumerateFiles(profileDir, profileFileName + ".json.bk*")
                     .Where(path => !path.EndsWith(".corrupted")))
        {
            File.Delete(extraFailedSaves);
        }

        if (profileSettings == null) return;
        InitializeScriptSettings(profileSettings);

        if (selectedProfile)
            Profile = profileSettings;

        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Profiles.Add(profileSettings);
        }, DispatcherPriority.Input);
    }

    private void SaveProfile()
    {
        SaveProfile(Profile);
    }

    public void SaveProfile(ApplicationProfile profile, string? path = null)
    {
        if (Disposed)
            return;

        path ??= Path.Combine(GetProfileFolderPath(), profile.ProfileFilepath);
        profile.SaveProfile(path);
    }

    public void SaveProfiles()
    {
        if (Disposed)
            return;

        try
        {
            var profilesPath = GetProfileFolderPath();

            if (!Directory.Exists(profilesPath))
                Directory.CreateDirectory(profilesPath);

            foreach (var profile in Profiles)
            {
                SaveProfile(profile, Path.Combine(profilesPath, profile.ProfileFilepath));
            }
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception during SaveProfiles");
        }
    }

    public async Task SaveAll()
    {
        if (Disposed || Config == null)
            return;

        await SaveSettings(Config.SettingsType);
        SaveProfiles();
    }

    protected override async Task LoadSettings(Type settingsType)
    {
        await base.LoadSettings(settingsType);

        Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    private async void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await SaveSettings(Config.SettingsType);
    }

    public virtual void Dispose()
    {
        if (Disposed)
            return;
        Disposed = true;

        SaveProfile();
        Profile = null;

        if (Settings != null)
        {
            Settings.PropertyChanged -= OnSettingsPropertyChanged;
        }

        foreach (var profile in Profiles)
            profile.Dispose();
        Profiles = null;
        _control = null;
        EffectScripts.Clear();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (Disposed)
            return;
        Disposed = true;

        SaveProfile();
        Profile = null;

        if (Settings != null)
        {
            Settings.PropertyChanged -= OnSettingsPropertyChanged;
        }

        foreach (var profile in Profiles)
            profile.Dispose();
        Profiles = null;
        _control = null;
        EffectScripts.Clear();
    }
}
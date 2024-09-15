using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using AuroraRgb.Modules.AudioCapture;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Utils;
using Common.Devices;
using Newtonsoft.Json;
using Serilog.Events;

namespace AuroraRgb.Settings;

public enum AuroraProcessPriority
{
    EfficiencyMode,
    Normal,
    High,
}

[JsonObject]
public class Configuration : INotifyPropertyChanged, IAuroraConfig
{
    public static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, "Config.json");

    [JsonIgnore]
    public string ConfigPath => ConfigFile;

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonProperty("close_on_exception")]
    public bool CloseProgramOnException { get; set; } = true;

    [JsonProperty("allow_wrappers_in_background")]
    public bool AllowWrappersInBackground { get; set; } = true;

    public bool AllowTransparency { get; set; } = true;
    public bool ChromaDisableDeviceControl { get; set; }
    public bool EnableLightsyncTakeover { get; set; } = true;

    [JsonProperty("global_brightness")]
    public float GlobalBrightness { get; set; } = 1.0f;

    [JsonProperty("keyboard_brightness_modifier")]
    public float KeyboardBrightness { get; set; } = 1.0f;

    [JsonProperty("peripheral_brightness_modifier")]
    public float PeripheralBrightness { get; set; } = 1.0f;

    public bool GetDevReleases { get; set; }
    public bool GetPointerUpdates { get; set; } = true;
    public AuroraProcessPriority ProcessPriority { get; set; } = AuroraProcessPriority.EfficiencyMode;
    public BitmapAccuracy BitmapAccuracy { get; set; } = BitmapAccuracy.Good;
    public bool EnableAudioCapture2 { get; set; } = true;
    public bool EnableAudioEnumeration { get; set; } = true;
    public bool EnableMediaInfo { get; set; } = true;
    public bool EnableInputCapture { get; set; } = true;
    public bool EnableHttpListener { get; set; } = true;
    public bool EnableIpcListener { get; set; } = true;
    public bool EnableHardwareInfo { get; set; } = true;
    public bool EnableAmdCpuMonitor { get; set; }
    public bool EnableShutdownOnConflict { get; set; } = true;

    public int UpdateDelay { get; set; } = 30;

    public double Fps => 1000d / UpdateDelay;

    [JsonProperty("updates_check_on_start_up")]
    public bool UpdatesCheckOnStartUp { get; set; } = true;

    public bool UpdateBackground { get; set; } = true;

    [JsonProperty("start_silently")]
    public bool StartSilently { get; set; }

    [JsonProperty("close_mode")]
    public AppExitMode CloseMode { get; set; } = AppExitMode.Minimize;

    [JsonProperty("mouse_orientation")]
    public MouseOrientationType MouseOrientation { get; set; } = MouseOrientationType.RightHanded;

    [JsonProperty("keyboard_brand")]
    public PreferredKeyboard KeyboardBrand { get; set; } = PreferredKeyboard.None;

    [JsonProperty("keyboard_localization")]
    public PreferredKeyboardLocalization KeyboardLocalization { get; set; } = PreferredKeyboardLocalization.None;

    [JsonProperty("mouse_preference")]
    public PreferredMouse MousePreference { get; set; } = PreferredMouse.Generic_Peripheral;

    [JsonProperty("mousepad_preference")]
    public PreferredMousepad MousepadPreference { get; set; } = PreferredMousepad.None;

    public PreferredHeadset HeadsetPreference { get; set; } = PreferredHeadset.None;
    public PreferredChromaLeds ChromaLedsPreference { get; set; } = PreferredChromaLeds.Automatic;

    [JsonProperty("virtualkeyboard_keycap_type")]
    public KeycapType VirtualkeyboardKeycapType { get; set; } = KeycapType.Default;

    [JsonProperty("detection_mode")]
    public ApplicationDetectionMode DetectionMode { get; set; } = ApplicationDetectionMode.WindowsEvents;

    public bool OverlaysInPreview { get; set; } = true;

    public ObservableCollection<string> ExcludedPrograms { get; set; } = [];

    public List<BitmapAccuracy> BitmapAccuracies { get; } =
    [
        BitmapAccuracy.Best,
        BitmapAccuracy.Good,
        BitmapAccuracy.Lowest
    ];

    //Blackout and Night theme
    [JsonProperty("time_based_dimming_enabled")]
    public bool TimeBasedDimmingEnabled { get; set; }

    [JsonProperty("time_based_dimming_affect_games")]
    public bool TimeBasedDimmingAffectGames { get; set; }

    [JsonProperty("time_based_dimming_start_hour")]
    public int TimeBasedDimmingStartHour { get; set; } = 21;

    [JsonProperty("time_based_dimming_start_minute")]
    public int TimeBasedDimmingStartMinute { get; set; }

    [JsonProperty("time_based_dimming_end_hour")]
    public int TimeBasedDimmingEndHour { get; set; } = 8;

    [JsonProperty("time_based_dimming_end_minute")]
    public int TimeBasedDimmingEndMinute { get; set; }

    [JsonProperty("nighttime_enabled")]
    public bool NighttimeEnabled { get; set; }

    [JsonProperty("nighttime_start_hour")]
    public int NighttimeStartHour { get; set; } = 20;

    [JsonProperty("nighttime_start_minute")]
    public int NighttimeStartMinute { get; set; }

    [JsonProperty("nighttime_end_hour")]
    public int NighttimeEndHour { get; set; } = 7;

    [JsonProperty("nighttime_end_minute")]
    public int NighttimeEndMinute { get; set; }

    // Idle Effects
    [JsonProperty("idle_type")]
    public IdleEffects IdleType { get; set; } = IdleEffects.None;

    [JsonProperty("idle_delay")]
    public int IdleDelay { get; set; } = 5;

    [JsonProperty("idle_speed")]
    public float IdleSpeed { get; set; } = 1.0f;

    [JsonProperty("idle_effect_primary_color")]
    public Color IdleEffectPrimaryColor { get; set; } = Color.Lime;

    [JsonProperty("idle_effect_secondary_color")]
    public Color IdleEffectSecondaryColor { get; set; } = Color.Black;

    [JsonProperty("idle_amount")]
    public int IdleAmount { get; set; } = 5;

    [JsonProperty("idle_frequency")]
    public float IdleFrequency { get; set; } = 2.5f;

    //Hardware Monitor
    public int HardwareMonitorUpdateRate { get; set; } = 500;
    public int HardwareMonitorMaxQueue { get; set; } = 8;
    public bool HardwareMonitorUseAverageValues { get; set; } = true;

    public double Width { get; set; } = 1200;
    public double Height { get; set; } = 800;
    public double Top { get; set; } = 50;
    public double Left { get; set; } = 50;
    public WindowState WindowState { get; set; } = WindowState.Normal;

    //BitmapDebug Data
    public bool BitmapDebugTopMost { get; set; }
    public bool BitmapWindowOnStartUp { get; set; }

    //httpDebug Data
    public bool HttpDebugTopMost { get; set; }
    public bool HttpWindowOnStartUp { get; set; }

    public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;

    public ObservableConcurrentDictionary<string, IEvaluatable> EvaluatableTemplates { get; set; } = new();

    public List<string> ProfileOrder { get; set; } = [
        "discord", "chroma", "logitech"
    ];

    [JsonProperty("GSIAudioRenderDevice", NullValueHandling = NullValueHandling.Ignore)]
    public string GsiAudioRenderDevice { get; set; } = AudioDevices.DefaultDeviceId;

    [JsonProperty("GSIAudioCaptureDevice", NullValueHandling = NullValueHandling.Ignore)]
    public string GsiAudioCaptureDevice { get; set; } = AudioDevices.DefaultDeviceId;

    public string GsiNetworkDevice { get; set; } = "Local Area Connection";

    public IList<string> Migrations { get; set; } = [];

    public bool? AutoInstallGsi { get; set; }

    /// <summary>
    /// Called after the configuration file has been deserialized or created for the first time.
    /// </summary>
    public void OnPostLoad()
    {
        // Setup events that will trigger PropertyChanged when child collections change (to trigger a save)
        ExcludedPrograms.CollectionChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExcludedPrograms)));
        EvaluatableTemplates.CollectionChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EvaluatableTemplates)));
    }
}

public static class ExtensionHelpers
{
    public static bool IsAutomaticGeneration(this PreferredKeyboardLocalization self)
    {
        return self is PreferredKeyboardLocalization.ansi or PreferredKeyboardLocalization.iso;
    }
}
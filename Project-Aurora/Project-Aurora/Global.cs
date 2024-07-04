using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.AudioCapture;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Modules.Layouts;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace AuroraRgb;

/// <summary>
/// Globally accessible classes and variables
/// </summary>
public static class Global
{
    public const string AuroraExe = "AuroraRgb.exe";
    public static readonly string ScriptDirectory = "Scripts";

    /// <summary>
    /// A boolean indicating if Aurora was started with Debug parameter
    /// </summary>
    public static bool isDebug;

    /// <summary>
    /// The path to the application executing directory
    /// </summary>
    public static string ExecutingDirectory { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";

    public static string AppDataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora");

    public static string LogsDirectory { get; } = Path.Combine(AppDataDirectory, "Logs");

    public static SynchronizationContext WpfSyncContext { get; private set; }

    /// <summary>
    /// Output logger for errors, warnings, and information
    /// </summary>
    public static ILogger logger;

    public static LightingStateManager? LightingStateManager { get; set; } //TODO module access

    public static Configuration Configuration
    {
        get => _configuration!;
        set
        {
            _configuration = value;

            Configuration.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(Configuration.LogLevel))
                {
                    return;
                }

                logger.Information("Setting logger level: {Level}", Configuration.LogLevel);
                LoggingLevelSwitch.MinimumLevel = Configuration.LogLevel;
            };
            LoggingLevelSwitch.MinimumLevel = Configuration.LogLevel;
        }
    }

    public static SensitiveData SensitiveData { get; set; } = new();

    public static KeyboardLayoutManager? kbLayout { get; set; }                //TODO module access
    public static Effects effengine { get; set; }
    public static KeyRecorder? key_recorder { get; set; }
    public static AudioDeviceProxy? CaptureProxy { get; set; }
    public static AudioDeviceProxy? RenderProxy { get; set; }

    internal static readonly LoggingLevelSwitch LoggingLevelSwitch = new(LogEventLevel.Verbose);
    private static Configuration? _configuration;

    public static void Initialize()
    {
        Directory.SetCurrentDirectory(ExecutingDirectory);
        
        WpfSyncContext = AsyncOperationManager.SynchronizationContext;
#if DEBUG
        isDebug = true;
#endif
        var logFile = $"{DateTime.UtcNow:yyyy-MM-dd HH.mm.ss}.log";
        var logPath = Path.Combine(AppDataDirectory, "Logs", logFile);
        var timeSpan = isDebug ? TimeSpan.FromSeconds(2) : TimeSpan.FromSeconds(30);
        logger = new LoggerConfiguration()
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .Filter.UniqueOverSpan("true", timeSpan)
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Infinite,
                fileSizeLimitBytes: 25 * 1000000,  //25 MB
                outputTemplate: "Aurora-{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
#if DEBUG
            .WriteTo.Console(
                applyThemeToRedirectedOutput: true
            )
            .WriteTo.Debug()
#endif
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .CreateLogger();
    }
}
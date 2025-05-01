using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.Gamebar;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Utils;
using AuroraRgb.Utils.Json;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AuroraRgb.Settings;

public static class ConfigManager
{
    private static readonly Dictionary<string, DateTime> LastSaveTimes = new();
    private static readonly TimeSpan SaveInterval = TimeSpan.FromMilliseconds(300);

    public static async Task<Configuration> Load()
    {
        Global.logger.Information("Loading Configuration");
        var config = await TryLoad();

        config.OnPostLoad();
        config.PropertyChanged += (_, _) =>
        {
            Save(config);
        };

        return config;
    }

    private static async Task<Configuration> TryLoad()
    {
        try
        {
            return await Parse();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception during ConfigManager.Load(). Error: ");
            var result = MessageBox.Show(
                $"Exception loading configuration. Error: {exc.Message}\r\n\r\n" +
                $" Do you want to reset settings? (this won't reset profiles).",
                "Aurora - Error",
                MessageBoxButton.YesNo
            );

            if (result == MessageBoxResult.Yes)
            {
                return await CreateDefaultConfigurationFile();
            }

            App.ForceShutdownApp(-1);
            throw new UnreachableException();
        }
    }

    private static async Task<Configuration> Parse()
    {
        if (!File.Exists(Configuration.ConfigFile))
            return await CreateDefaultConfigurationFile();
        
        var content = await File.ReadAllTextAsync(Configuration.ConfigFile, Encoding.UTF8);
        return JsonConvert.DeserializeObject<Configuration>(content,
                new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.All,
                }) ?? await CreateDefaultConfigurationFile();
    }

    public static async Task<DeviceConfig> LoadDeviceConfig()
    {
        DeviceConfig config;
        try
        {
            config = await TryLoadDevice();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception loading DeviceConfig. Error: ");
            config = new DeviceConfig();
        }

        config.OnPostLoad();
        config.PropertyChanged += (_, _) => { Save(config); };

        return config;
    }

    private static async Task<DeviceConfig> TryLoadDevice()
    {
        if (!File.Exists(DeviceConfig.ConfigFile))
        {
            if (File.Exists(Configuration.ConfigFile))
                // v194 Migration
                return await MigrateDeviceConfig();

            // first time start
            var deviceConfig = new DeviceConfig();
            await SaveAsync(deviceConfig);
            return deviceConfig;

        }

        var content = await File.ReadAllTextAsync(DeviceConfig.ConfigFile, Encoding.UTF8);
        return JsonSerializer.Deserialize(content, CommonSourceGenerationContext.Default.DeviceConfig) ?? await MigrateDeviceConfig();
    }
    
    public static async Task<GamebarConfig> LoadGamebarConfig()
    {
        GamebarConfig config;
        try
        {
            config = await TryLoadGamebar();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception loading GamebarConfig. Error: ");
            config = new GamebarConfig();
        }

        return config;
    }
    
    private static async Task<GamebarConfig> TryLoadGamebar()
    {
        if (!File.Exists(GamebarConfig.ConfigFile))
        {
            // first time start
            var gamebarConfig = new GamebarConfig();
            await SaveAsync(gamebarConfig);
            return gamebarConfig;
        }

        var content = await File.ReadAllTextAsync(GamebarConfig.ConfigFile, Encoding.UTF8);
        return JsonSerializer.Deserialize(content, GamebarSourceGenerationContext.Default.GamebarConfig) ?? new GamebarConfig();
    }

    public static async Task<AuroraChromaSettings> LoadChromaConfig()
    {
        AuroraChromaSettings config;
        try
        {
            config = await TryLoadChroma();
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception loading AuroraChromaSettings. Error: ");
            config = new AuroraChromaSettings();
        }

        return config;
    }

    private static async Task<AuroraChromaSettings> TryLoadChroma()
    {
        if (!File.Exists(AuroraChromaSettings.ConfigFile))
        {
            // first time start
            var chromaSettings = new AuroraChromaSettings();
            await SaveAsync(chromaSettings);
            return chromaSettings;
        }

        var content = await File.ReadAllTextAsync(AuroraChromaSettings.ConfigFile, Encoding.UTF8);
        return JsonSerializer.Deserialize(content, ChromaSourceGenerationContext.Default.AuroraChromaSettings) ?? new AuroraChromaSettings();
    }

    public static void Save(IAuroraConfig configuration)
    {
        var path = configuration.ConfigPath;
        var currentTime = DateTime.UtcNow;

        if (LastSaveTimes.TryGetValue(path, out var lastSaveTime) && lastSaveTime + SaveInterval > currentTime) return;

        LastSaveTimes[path] = currentTime;

        var content = JsonConvert.SerializeObject(configuration, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto, SerializationBinder = new AuroraSerializationBinder()
        });

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content, Encoding.UTF8);
    }

    public static async Task SaveAsync(IAuroraConfig configuration)
    {
        var path = configuration.ConfigPath;
        var currentTime = DateTime.UtcNow;

        if (LastSaveTimes.TryGetValue(path, out var lastSaveTime) && lastSaveTime + SaveInterval > currentTime) return;

        LastSaveTimes[path] = currentTime;

        var content = JsonConvert.SerializeObject(configuration, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto, SerializationBinder = new AuroraSerializationBinder()
        });

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        await File.WriteAllTextAsync(path, content, Encoding.UTF8);
    }

    private static async Task<Configuration> CreateDefaultConfigurationFile()
    {
        Global.logger.Information("Creating default configuration");
        var config = new Configuration();
        await SaveAsync(config);
        return config;
    }

    private static async Task<DeviceConfig> MigrateDeviceConfig()
    {
        Global.logger.Information("Migrating default device configuration");
        var content = await File.ReadAllTextAsync(Configuration.ConfigFile, Encoding.UTF8);
        var config = JsonConvert.DeserializeObject<DeviceConfig>(content,
            new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                SerializationBinder = new AuroraSerializationBinder(),
            }) ?? new DeviceConfig();
        File.Copy(Configuration.ConfigFile, Configuration.ConfigFile + ".v194", true);
        return config;
    }

    public static async Task<SensitiveData> LoadSensitiveData()
    {
        var exists = File.Exists(SensitiveData.ConfigFile);
        if (!exists)
        {
            return new SensitiveData();
        }

        try
        {
            var encryptedContent = await File.ReadAllBytesAsync(SensitiveData.ConfigFile);
            var content = Encryption.Decrypt(encryptedContent);
            return JsonSerializer.Deserialize<SensitiveData>(content) ?? new SensitiveData();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Failed parsing Sensitive.json");
        }
        return new SensitiveData();
    }

    public static void SaveSensitiveData()
    {
        var content = JsonSerializer.Serialize(Global.SensitiveData);
        var encryptedContent = Encryption.Encrypt(content);
        File.WriteAllBytes(SensitiveData.ConfigFile, encryptedContent);
    }
}
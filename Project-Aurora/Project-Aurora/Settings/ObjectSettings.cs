using System;
using System.IO;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AuroraRgb.Settings;

public class ObjectSettings<T>
{
    protected string? SettingsSavePath { get; set; }
    public T? Settings { get; protected set; }

    public async Task SaveSettings()
    {
        await SaveSettings(typeof(T));
    }

    protected async Task SaveSettings(Type settingsType)
    {
        if (SettingsSavePath == null)
        {
            return;
        }

        if (Settings == null) {
            Settings = (T)Activator.CreateInstance(settingsType);
            SettingsCreateHook();
        }

        var dir = Path.GetDirectoryName(SettingsSavePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var retries = 5;
        while (retries-- > 0)
        {
            try
            {
                var json = JsonSerializer.Serialize(Settings, Settings.GetType(), SettingsJsonContext.Default);
                await File.WriteAllTextAsync(SettingsSavePath, json);
                return;
            }
            catch (IOException ioException)
            {
                Global.logger.Error(ioException, "Unable to save settings to {SettingsSavePath}", SettingsSavePath);
                await Task.Delay(500);
            }
        }
    }

    /// <summary>A method that is called immediately after the settings being created. Can be overriden to provide specalized handling.</summary>
    protected virtual void SettingsCreateHook() { }

    protected async Task LoadSettings()
    {
        await LoadSettings(typeof(T));
    }

    protected virtual async Task LoadSettings(Type settingsType)
    {
        if (SettingsSavePath == null)
        {
            Global.logger.Warning("Type {Type} does not have a setting save path!", GetType());
            return;
        }

        if (File.Exists(SettingsSavePath))
        {
            try
            {
                string json;
                using (var streamReader = new StreamReader(SettingsSavePath))
                {
                    json = await streamReader.ReadToEndAsync();
                }
                Settings = (T)JsonSerializer.Deserialize(json, settingsType, SettingsJsonContext.Default);
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "Exception occured while loading \\\"{Name}\\\" Settings", GetType().Name);
            }
        }
        if (Equals(Settings, default(T)))
        {
            Settings = (T)Activator.CreateInstance(settingsType);
            SettingsCreateHook();
        }
    }
}
using System;
using System.IO;
using Newtonsoft.Json;

namespace AuroraRgb.Settings;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
public class ObjectSettings<T>(string settingsSavePath)
    where T : new()
{
    protected string SettingsSavePath { get; init; } = settingsSavePath;
    public T? Settings { get; protected set; }

    public void SaveSettings()
    {
        if (Settings == null) {
            Settings = new T();
            SettingsCreateHook();
        }

        var dir = Path.GetDirectoryName(SettingsSavePath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(SettingsSavePath, JsonConvert.SerializeObject(Settings, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented }));
    }

    /// <summary>A method that is called immediately after the settings being created. Can be overriden to provide specialized handling.</summary>
    protected virtual void SettingsCreateHook() { }

    protected virtual void LoadSettings()
    {
        if (File.Exists(SettingsSavePath))
        {
            try
            {
                Settings = JsonConvert.DeserializeObject<T>(File.ReadAllText(SettingsSavePath), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
                if (Settings == null)
                {
                    SaveSettings();
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, """Exception occured while loading \"{Name}\" Settings""", GetType().Name);
                SaveSettings();
            }
        }
        else
            SaveSettings();
    }
}
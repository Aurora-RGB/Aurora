using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using JetBrains.Annotations;

namespace AuroraRgb.Settings;

[PublicAPI]
public interface IShortcut
{
    string Title { get; set; }
}

[PublicAPI]
public class ShortcutNode(string title) : IShortcut
{
    public string Title { get; set; } = title;

    public List<IShortcut>? Children { get; set; }

    public Keybind[] GetShortcuts()
    {
        if (Children == null)
            return [];

        var binds = new List<Keybind>();

        foreach (var shortcut in Children)
        {
            if (shortcut is ShortcutGroup group)
                binds.AddRange(group.Shortcuts);
            else if (shortcut is ShortcutNode shortcutNode)
                binds.AddRange(shortcutNode.GetShortcuts());
        }

        return binds.ToArray();
    }
}

[PublicAPI]
public class ShortcutGroup(string title) : IShortcut
{
    public string Title { get; set; } = title;

    public Keybind[] Shortcuts { get; set; } = [];
}

[PublicAPI]
public interface IPluginHost
{
    Dictionary<string, IPlugin> Plugins { get; }
    void SetPluginEnabled(string id, bool enabled);
}

[PublicAPI]
public interface IPlugin
{
    string ID { get; }
    string Title { get; }
    string Author { get; }
    Version Version { get; }
    IPluginHost? PluginHost { get; set; }
    void ProcessManager(object manager);
}

public static class PluginUtils
{
    public static bool Enabled(this IPlugin self)
    {
        return self.PluginHost != null;
    }
}

public class PluginEnabledConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var plugin = value as IPlugin;
        return plugin != null && plugin.Enabled();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PluginManagerSettings
{
    public Dictionary<string, bool> PluginManagement { get; } = new();
}

public sealed class PluginManager() : ObjectSettings<PluginManagerSettings>(Path.Combine(Global.AppDataDirectory, "PluginSettings.json")),
    IInitializableProfile, IPluginHost
{
    public const string PluginDirectory = "Plugins";

    public Dictionary<string, IPlugin> Plugins { get; set; } = new();

    public bool Initialized { get; private set; }

    public bool Initialize()
    {
        if (Initialized)
            return true;

        LoadSettings();
        LoadPlugins();

        return Initialized = true;
    }

    public void ProcessManager(object manager)
    {
        foreach (var plugin in Plugins)
        {
            try
            {
                plugin.Value.ProcessManager(manager);
            }
            catch(Exception e)
            {
                Global.logger.Error(e, "Failed to load plugin {PluginKey}", plugin.Key);
            }
        }
    }

    private void LoadPlugins()
    {
        var installationDir = Path.Combine(Global.ExecutingDirectory, PluginDirectory);
        LoadPlugins(installationDir);
        var userDir = Path.Combine(Global.AppDataDirectory, PluginDirectory);
        LoadPlugins(userDir);
    }

    private void LoadPlugins(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);

            //No need to search the directory if we just created it
            return;
        }

        foreach (var pathPlugin in Directory.EnumerateFiles(dir, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                LoadPlugin(pathPlugin);
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "Failed loading plugin {PluginPath}", pathPlugin);
                if (Global.isDebug)
                    throw;
            }
        }
    }

    private void LoadPlugin(string pathPlugin)
    {
        Global.logger.Information("Loading plugin: {PathPlugin}", pathPlugin);
        var dllPlugin = Assembly.LoadFrom(pathPlugin);

        foreach (var name in dllPlugin.GetReferencedAssemblies())
            AppDomain.CurrentDomain.Load(name);

        foreach (var typ in dllPlugin.GetExportedTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(typ)) continue;
            //Create an instance of the plugin type
            var objPlugin = (IPlugin)Activator.CreateInstance(typ);

            //Get the ID of the plugin
            var id = objPlugin.ID;

            if (!Settings.PluginManagement.ContainsKey(id) || Settings.PluginManagement[id])
                objPlugin.PluginHost = this;

            Plugins.Add(id, objPlugin);
        }
    }

    public void SetPluginEnabled(string id, bool enabled)
    {
        Settings.PluginManagement[id] = enabled; 
        SaveSettings();
    }

    public void Dispose()
    {

    }
}
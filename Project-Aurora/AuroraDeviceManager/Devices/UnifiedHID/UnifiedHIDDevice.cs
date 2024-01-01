﻿using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Common.Devices;
using Common.Utils;

namespace AuroraDeviceManager.Devices.UnifiedHID;

public class UnifiedHIDDevice : DefaultDevice
{
    private Stopwatch sleepWatch = new();
    private long lastUpdateTime = 0;
    private long lastSleepUpdateTime;

    List<UnifiedBase> allDevices = new();
    List<UnifiedBase> connectedDevices = new();

    public override string DeviceName => "UnifiedHID";
    protected override string DeviceInfo => string.Join(", ", connectedDevices.Select(hd => hd.PrettyName));

    public override bool IsInitialized => connectedDevices.Count != 0;

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        variableRegistry = new VariableRegistry();

        variableRegistry.Register($"{DeviceName}_update_interval", 0, "Update interval", null, 0);
        variableRegistry.Register($"{DeviceName}_enable_shutdown_color", false, "Enable shutdown color");
        variableRegistry.Register($"{DeviceName}_shutdown_color", Color.FromArgb(255, 255, 255, 255), "Shutdown color");

        foreach (var device in allDevices)
            variableRegistry.Register($"UnifiedHID_{device.GetType().Name}_enable", false, 
                $"Enable {(string.IsNullOrEmpty(device.PrettyName) ? device.GetType().Name : device.PrettyName)} in {DeviceName}");
    }

    public UnifiedHIDDevice()
    {
        try
        {
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetLoadableTypes())
                .Where(type => type.IsSubclassOf(typeof(UnifiedBase))).ToList()
                .ForEach(class_ => allDevices.Add((UnifiedBase)Activator.CreateInstance(class_)));
        }
        catch (Exception exc)
        {
            Global.Logger.Error(exc, "[UnifiedHID] class could not be constructed:");
        }
    }

    protected override Task<bool> DoInitialize()
    {
        if (IsInitialized) return Task.FromResult(IsInitialized);
        // Clear list from old data
        connectedDevices.Clear();

        try
        {
            foreach (var device in allDevices)
            {
                // Force disconnection and try a new connection
                if (device.Disconnect() && device.Connect())
                {
                    connectedDevices.Add(device);
                }
            }
        }
        catch (Exception e)
        {
            Global.Logger.Error($"[UnifiedHID] device could not be initialized:", e);
        }

        return Task.FromResult(IsInitialized);
    }

    protected override Task Shutdown()
    {
        try
        {
            if (IsInitialized)
            {
                var enableShutdownColor = Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_enable_shutdown_color");
                var shutdownColor = Global.DeviceConfig.VarRegistry.GetVariable<Color>($"{DeviceName}_shutdown_color");

                foreach (UnifiedBase dev in connectedDevices)
                {
                    foreach (var map in dev.DeviceFuncMap)
                    {
                        if (enableShutdownColor)
                            map.Value.Invoke(shutdownColor.R, shutdownColor.G, shutdownColor.B);
                    }

                    dev.Disconnect();
                }

                connectedDevices.Clear();
            }
        }
        catch (Exception ex)
        {
            Global.Logger.Error("[UnifiedHID] there was an error shutting down devices", ex);
        }

        return Task.CompletedTask;
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        var sleep = Global.DeviceConfig.VarRegistry.GetVariable<int>($"{DeviceName}_update_interval");
        sleepWatch.Stop();
        lastSleepUpdateTime = sleepWatch.ElapsedMilliseconds;
        if (lastSleepUpdateTime > sleep)
        {
            sleepWatch.Restart();
        }
        else
        {
            // Resume stopWatch
            sleepWatch.Start();
            return Task.FromResult(false);
        }

        try
        {
            var results = new Dictionary<UnifiedBase, bool>(connectedDevices.Count);

            foreach (var key in keyColors)
            {
                foreach (var device in connectedDevices)
                {
                    if (!device.DeviceColorMap.TryGetValue(key.Key, out Color currentColor) ||
                        currentColor == key.Value) continue;
                    // Apply and strip Alpha
                    var color = Color.FromArgb(255, CommonColorUtils.MultiplyColorByScalar(key.Value, key.Value.A / 255.0D));

                    // Update current color
                    device.DeviceColorMap[key.Key] = color;

                    // Set color
                    results[device] = device.SetColor(key.Key, color.R, color.G, color.B);
                }
            }

            // Check results of connected devices
            foreach (var result in results)
            {
                if (result.Value) continue;
                Global.Logger.Error("[UnifiedHID] error when updating device {KeyName}. Restarting...", result.Key.PrettyName);

                // Try to restart device
                if (!result.Key.Disconnect() || result.Key.Connect()) continue;
                Global.Logger.Error("[UnifiedHID] unable to restart device {KeyName}. Removed from connected device!", result.Key.PrettyName);
                // Remove device from connected list
                connectedDevices.Remove(result.Key);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Global.Logger.Error("[UnifiedHID] error when updating device:", ex);
            return Task.FromResult(false);
        }
    }
}
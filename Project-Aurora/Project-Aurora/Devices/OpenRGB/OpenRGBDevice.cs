using Aurora.Modules.ProcessMonitor;
using Aurora.Settings;
using CSScripting;
using Microsoft.Scripting.Utils;
using OpenRGB.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using OpenRGBClient = OpenRGB.NET.OpenRgbClient;
using OpenRGBColor = OpenRGB.NET.Color;
using OpenRGBDeviceType = OpenRGB.NET.DeviceType;
using OpenRGBZoneType = OpenRGB.NET.ZoneType;

namespace Aurora.Devices.OpenRGB
{
    public class OpenRGBDevice : DefaultDevice
    {
        public override string DeviceName => "OpenRGB";
        protected override string DeviceInfo => string.Join(", ", _helpers.Select(h => h.Device.Name));

        public readonly Queue<DeviceKeys> MouseLights = new Queue<DeviceKeys>(OpenRgbKeyNames.MouseLights);

        private OpenRGBClient _client;
        private List<OpenRGBDeviceHelper> _helpers;

        private SemaphoreSlim _updateLock = new(1);
        private SemaphoreSlim _initializeLock = new(1);

        protected override async Task<bool> DoInitialize()
        {
            if (IsInitialized)
                return true;

            await _initializeLock.WaitAsync().ConfigureAwait(false);

            try
            {
                // Get connection settings
                var ip = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_ip");
                var port = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_port");
                var remainingMillis = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_connect_sleep_time") * 1000;

                var openrgbRunning = () => Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("openrgb.exe");

                // Search if process is running
                while (!openrgbRunning())
                {
                    Thread.Sleep(100);
                    remainingMillis -= 100;
                    if (remainingMillis <= 0)
                    {
                        throw new Exception("OpenRGB is not running...");
                    }
                }

                // Create OpenRGBDevice list
                _helpers = new List<OpenRGBDeviceHelper>();
                // Initialize OpenRGBClient
                _client = new OpenRGBClient(ip, port, "Aurora");
                // Add callback to device list update
                _client.DeviceListUpdated += (s, e) => UpdateDeviceList();

                UpdateDeviceList();
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                _client = null;
                IsInitialized = false;
                LogError("Unable to Initialize OpenRGB", ex);
            }
            finally
            {
                _initializeLock.Release();
            }

            return IsInitialized;
        }

        private void UpdateDeviceList()
        {
            _updateLock.Wait();

            var fallbackKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_fallback_key");

            // Clear old device list
            _helpers.Clear();

            try
            {
                foreach (var device in _client.GetAllControllerData())
                {
                    // Device MUST have Direct mode to work with Aurora
                    var directModeIndex = device.Modes.FindIndex(m => m.Name.Equals("Direct"));

                    if (directModeIndex != -1)
                    {
                        // Initialize new device
                        _client.SetMode(device.Index, directModeIndex);
                        var helper = new OpenRGBDeviceHelper(device);
                        helper.ProcessMappings(fallbackKey);
                        _helpers.Add(helper);
                    }
                }
            }
            finally
            {
                _updateLock.Release();
            }

            Thread.Sleep(500);
        }

        public override async Task Shutdown()
        {
            if (!IsInitialized)
                return;

            await _initializeLock.WaitAsync().ConfigureAwait(false);

            foreach (var helper in _helpers)
            {
                try
                {
                    // Restore initial colors
                    _client?.UpdateLeds(helper.Device.Index, helper.InitialColors);
                }
                catch
                {
                    //we tried.
                }
            }

            _client?.Dispose();
            _client = null;

            _initializeLock.Release();

            IsInitialized = false;
            return;
        }

        protected override async Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            await _updateLock.WaitAsync().ConfigureAwait(false);

            var needRestart = false;

            foreach (var helper in _helpers)
            {
                try
                {
                    UpdateLeds(helper, keyColors, forced);
                }
                catch (Exception ex)
                {
                    needRestart = true;
                    LogError($"Failed to update OpenRGB device {helper.Device.Name}", ex);
                }
            }

            _updateLock.Release();

            if (needRestart)
            {
                Reset();
                return false;
            }

            if (Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_sleep") is int sleep && sleep > 0)
                Thread.Sleep(sleep);

            return true;
        }

        private void UpdateLeds(OpenRGBDeviceHelper helper, Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
        {
            var ledIndex = 0;

            foreach (var zone in helper.Device.Zones)
            {
                for (var zoneLed = 0; zoneLed < zone.LedCount; ledIndex++, zoneLed++)
                {
                    var currentColor = helper.Device.Colors[ledIndex];

                    if (!keyColors.TryGetValue(helper.Mapping[ledIndex], out var keyColor)) continue;

                    if (Global.Configuration.DeviceCalibrations.TryGetValue(CalibrationName(helper, zone), out var calibration))
                        helper.Device.Colors[ledIndex] = new OpenRGBColor(
                            (byte)(keyColor.R * calibration.R / 255),
                            (byte)(keyColor.G * calibration.G / 255),
                            (byte)(keyColor.B * calibration.B / 255)
                        );
                    else
                        helper.Device.Colors[ledIndex] = new OpenRGBColor(keyColor.R, keyColor.G, keyColor.B);

                    if (helper.Device.Colors[ledIndex] != currentColor)
                        helper.NeedUpdate = true;
                }
            }

            if (helper.NeedUpdate)
            {
                helper.NeedUpdate = false;
                _client.UpdateLeds(helper.Device.Index, helper.Device.Colors);
            }
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 0, "Sleep for", 1000, 0);
            variableRegistry.Register($"{DeviceName}_ip", "127.0.0.1", "IP Address");
            variableRegistry.Register($"{DeviceName}_port", 6742, "Port", 1024, 65535);
            variableRegistry.Register($"{DeviceName}_fallback_key", DeviceKeys.Peripheral_Logo, "Key to use for unknown leds. Select NONE to disable");
            variableRegistry.Register($"{DeviceName}_connect_sleep_time", 5, "Connection timeout seconds");
        }

        public override IEnumerable<string> GetDevices()
        {
            _updateLock.Wait();

            var names = from helper in _helpers
                   from zone in helper.Device.Zones
                   select CalibrationName(helper, zone);

            _updateLock.Release();

            return names;
        }

        private string CalibrationName(OpenRGBDeviceHelper helper, Zone zone)
        {
            return helper.ZoneCalibrationNames[zone.Index];
        }
    }

    public class OpenRGBDeviceHelper
    {
        private Device _device;
        public Device Device { get => _device; }

        public bool NeedUpdate { get; set; }
        public OpenRGBColor[] InitialColors { get; }
        public DeviceKeys[] Mapping { get; private set; }
        public Dictionary<int, string> ZoneCalibrationNames { get; }

        public OpenRGBDeviceHelper(Device device)
        {
            _device = device;
            NeedUpdate = true;

            // Save current device color
            InitialColors = device.Colors.ToArray();

            // Reset current device color
            for (var i = 0; i < InitialColors.Length; i++)
                device.Colors[i] = new OpenRGBColor();

            // Add calibration zones
            ZoneCalibrationNames = new Dictionary<int, string>();
            foreach (var zone in device.Zones)
                ZoneCalibrationNames.Add(zone.Index, $"OpenRGB_{device.Name.Trim()}_{zone.Name}");
        }

        public void ProcessMappings(DeviceKeys fallbackKey)
        {
            Mapping = new DeviceKeys[Device.Zones.Sum(z => z.LedCount)];

            for (var ledIndex = 0; ledIndex < Device.Leds.Length; ledIndex++)
            {
                DeviceKeys devKey = fallbackKey;
                var orgbKeyName = Device.Leds[ledIndex].Name;

                var resultKey =
                    OpenRgbKeyNames.KeyNames.TryGetValue(orgbKeyName, out devKey) ||
                    OpenRgbKeyNames.KeyNames.TryGetValue("Key: " + orgbKeyName, out devKey) ||
                    OpenRgbKeyNames.KeyNames.TryGetValue(orgbKeyName.Replace(" LED", ""), out devKey) ||
                    OpenRgbKeyNames.KeyNames.TryGetValue("Key: " + orgbKeyName.Replace(" LED", ""), out devKey);

                if (Device.Type == OpenRGBDeviceType.Mouse)
                {
                    // Remove LED postfix
                    orgbKeyName = orgbKeyName.Replace(" LED", "");

                    if (orgbKeyName.Equals("Logo"))
                    {
                        resultKey = true;
                        devKey = DeviceKeys.Peripheral_Logo;
                    }
                    else if (orgbKeyName.Equals("Primary"))
                    {
                        // Tested with Logitech G502
                        resultKey = true;
                        devKey = DeviceKeys.PERIPHERAL_DPI;
                    }
                    else if (orgbKeyName.Equals("Underglow"))
                    {
                        // Tested with Asus PUGIO
                        resultKey = true;
                        devKey = DeviceKeys.Peripheral_FrontLight;
                    }
                }

                Mapping[ledIndex] = resultKey ? devKey : fallbackKey;
            }

            //if we have the option enabled,
            //we'll skip these as users may not want 
            //linear zones to depend on additionalllight

            uint ledOffset = 0;
            var _mouseLights = new Queue<DeviceKeys>(OpenRgbKeyNames.MouseLights);

            for (int zoneIndex = 0; zoneIndex < Device.Zones.Length; zoneIndex++)
            {
                if (Device.Zones[zoneIndex].Type == OpenRGBZoneType.Linear)
                {
                    for (int zoneLedIndex = 0; zoneLedIndex < Device.Zones[zoneIndex].LedCount; zoneLedIndex++)
                    {
                        switch (Device.Type)
                        {
                            case OpenRGBDeviceType.Mousemat:
                                if (zoneLedIndex < OpenRgbKeyNames.MousepadLights.Length)
                                    Mapping[(int)(ledOffset + zoneLedIndex)] = OpenRgbKeyNames.MousepadLights[zoneLedIndex];
                                break;
                            case OpenRGBDeviceType.Mouse:
                                if (zoneLedIndex < OpenRgbKeyNames.MouseLights.Length && _mouseLights.TryDequeue(out var result))
                                    Mapping[(int)(ledOffset + zoneLedIndex)] = result;
                                break;
                            default:
                                if (zoneLedIndex < OpenRgbKeyNames.AdditionalLights.Length)
                                    Mapping[(int)(ledOffset + zoneLedIndex)] = OpenRgbKeyNames.AdditionalLights[zoneLedIndex];
                                break;
                        }
                    }
                }

                ledOffset += Device.Zones[zoneIndex].LedCount;
            }
        }
    }
}

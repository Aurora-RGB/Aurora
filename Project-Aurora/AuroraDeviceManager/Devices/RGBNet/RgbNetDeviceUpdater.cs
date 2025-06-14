﻿using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;
using RGB.NET.Core;

namespace AuroraDeviceManager.Devices.RGBNet;

public class RgbNetDeviceUpdater(ConcurrentDictionary<IRGBDevice, Dictionary<LedId, DeviceKeys>> deviceKeyRemap, bool needsLayout)
{
    private bool _flush;
    
    internal void Flush()
    {
        _flush = true;
    }
    
    internal void UpdateDevice(Dictionary<DeviceKeys, SimpleColor> keyColors, IRGBDevice device)
    {
        if (needsLayout)
        {
            UpdateReverse(keyColors, device);
        }
        else
        {
            UpdateStraight(keyColors, device);
        }

        device.Update(_flush);
        _flush = false;
    }

    private static void UpdateReverse(Dictionary<DeviceKeys, SimpleColor> keyColors, IRGBDevice device)
    {
        var calibrationName = CalibrationName(device);
        var calibrated = Global.DeviceConfig.DeviceCalibrations.TryGetValue(calibrationName, out var calibration);
        foreach (var key in keyColors.Keys)
        {
            ref var color = ref CollectionsMarshal.GetValueRefOrNullRef(keyColors, key);

            if (!RgbNetKeyMappings.AuroraToRgbNet.TryGetValue(key, out var rgbNetLedId))
                continue;

            var led = device[rgbNetLedId];
            if (led == null && LedInDeviceGroup(rgbNetLedId, device.DeviceInfo.DeviceType) && color.A != 0)
            {
                if (device.Size == Size.Invalid)
                {
                    device.Size = new Size(0, 0);
                }
                led = device.AddLed(rgbNetLedId, new Point(device.Size.Width, 0), new Size(10, 10));
                device.Size = new Size(device.Size.Width + 10, 10);
            }

            if (led == null)
                continue;

            if (calibrated)
            {
                UpdateLedCalibrated(led, in color, calibration);
            }
            else
            {
                UpdateLed(led, in color);
            }
        }
    }

    private static bool LedInDeviceGroup(LedId rgbNetLedId, RGBDeviceType deviceInfoDeviceType)
    {
        return deviceInfoDeviceType switch
        {
            RGBDeviceType.Keyboard => LedGroups.KeyboardLeds.Contains(rgbNetLedId),
            RGBDeviceType.Mouse => LedGroups.MouseLeds.Contains(rgbNetLedId),
            RGBDeviceType.Mousepad => LedGroups.MousepadLeds.Contains(rgbNetLedId),
            RGBDeviceType.Headset => LedGroups.HeadsetLeds.Contains(rgbNetLedId),
            _ => true
        };
    }

    private void UpdateStraight(Dictionary<DeviceKeys, SimpleColor> keyColors, IRGBDevice device)
    {
        var calibrationName = CalibrationName(device);
        deviceKeyRemap.TryGetValue(device, out var keyRemap);
        var calibrated = Global.DeviceConfig.DeviceCalibrations.TryGetValue(calibrationName, out var calibration);
        foreach (var led in device)
        {
            if (!(keyRemap != null &&
                  keyRemap.TryGetValue(led.Id, out var dk)) && //get remapped key if device if remapped
                !RgbNetKeyMappings.KeyNames.TryGetValue(led.Id, out dk)) continue;
            if (!keyColors.TryGetValue(dk, out var color)) continue;

            if (calibrated)
            {
                UpdateLedCalibrated(led, in color, calibration);
            }
            else
            {
                UpdateLed(led, in color);
            }
        }
    }

    private static void UpdateLed(Led led, in SimpleColor color)
    {
        led.Color = new Color(
            color.A,
            color.R,
            color.G,
            color.B
        );
    }

    private static void UpdateLedCalibrated(Led led, in SimpleColor color, SimpleColor calibration)
    {
        led.Color = new Color(
            (byte)(color.A * calibration.A / 255),
            (byte)(color.R * calibration.R / 255),
            (byte)(color.G * calibration.G / 255),
            (byte)(color.B * calibration.B / 255)
        );
    }

    private static string CalibrationName(IRGBDevice device)
    {
        return device.DeviceInfo.DeviceName;
    }

}
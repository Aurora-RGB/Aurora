﻿using RGB.NET.Core;
using RGB.NET.Devices.SteelSeries;

namespace AuroraDeviceManager.Devices.RGBNet;

public class SteelSeriesRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => SteelSeriesDeviceProvider.Instance;

    public override string DeviceName => "SteelSeries (RGB.NET)";

    protected override bool IsReversed() => true;
}
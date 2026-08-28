using System.ComponentModel;
using System.Diagnostics;
using Common;
using Common.Devices;
using Common.Utils;
using HidSharp;

namespace AuroraDeviceManager.Devices.SteelSeries;

// Direct HID driver for the Apex Pro Gen 3 family: no GameSense/GG needed, Aurora owns the board.
// Protocol (validated against SignalRGB 2.5.72 driving PID 0x1640 on 2026-07-05):
//   vendor interface (mi_01, max feature report 643) takes FEATURE reports with report id 0x00;
//   [0x00, 0x4B] enters software mode, [0x00, <modelByte>, <ledCount>, (<hidCode>, R, G, B)*] sets colours.
// LED ids are USB HID keyboard usage codes, the same table GameSense used (USBHIDCodes.cs).
public class SteelSeriesApexGen3Device : DefaultDevice
{
    private const int VendorId = 0x1038;
    private const int ReportLength = 642;
    private const byte SoftwareModeCommand = 0x4B;

    // Second report byte differs per model; the device ignores the frame when it is wrong.
    private static readonly (int Pid, byte ModelByte, string Model)[] SupportedDevices =
    [
        (0x1640, 0x3A, "Apex Pro Gen 3"),
        (0x1642, 0x40, "Apex Pro TKL Gen 3"),
        (0x1644, 0x61, "Apex Pro TKL Gen 3 Wireless"),
        (0x1646, 0x21, "Apex Pro TKL Gen 3 Wireless (wired)"),
        (0x1648, 0x40, "Apex Pro Mini Gen 3"),
    ];

    private HidStream? _stream;
    private byte _modelByte;
    private byte[] _report = new byte[ReportLength];
    private byte[] _prevReport = new byte[ReportLength];

    private long _framesIn;
    private long _framesSkipped;
    private long _framesWritten;
    private readonly Stopwatch _statsWatch = Stopwatch.StartNew();
    private readonly Dictionary<byte, int> _hidSlot = new();

    private readonly ApexGen3Oled _oled = new();

    public override string DeviceName => "SteelSeries Apex Gen3";

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);
        _oled.RegisterVariables(variableRegistry, DeviceName);
    }

    protected override string DeviceInfo => _deviceInfo;
    private string _deviceInfo = "";

    protected override Task<bool> DoInitialize(CancellationToken cancellationToken)
    {
        foreach (var (pid, modelByte, model) in SupportedDevices)
        {
            // The vendor interface is the only one with feature reports large enough for a full colour frame.
            var hidDevice = DeviceList.Local.GetHidDevices(VendorId, pid)
                .FirstOrDefault(d => d.GetMaxFeatureReportLength() >= ReportLength);
            if (hidDevice == null)
                continue;

            try
            {
                if (!hidDevice.TryOpen(out _stream))
                {
                    LogError($"{model} found but the HID interface could not be opened (in use by GG/SignalRGB?)");
                    continue;
                }

                var reportLength = hidDevice.GetMaxFeatureReportLength();
                _report = new byte[reportLength];
                _prevReport = new byte[reportLength];
                _modelByte = modelByte;

                var softwareMode = new byte[reportLength];
                softwareMode[1] = SoftwareModeCommand;
                _stream.SetFeature(softwareMode);

                _deviceInfo = $"{model} (PID 0x{pid:X4})";
                LogInfo($"connected to {_deviceInfo}, report length {reportLength}");
                _oled.Initialize(DeviceName);
                IsInitialized = true;
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                LogError($"failed to initialize {model}", e);
                _stream?.Dispose();
                _stream = null;
            }
        }

        return Task.FromResult(false);
    }

    protected override Task Shutdown()
    {
        try
        {
            _stream?.Dispose();
        }
        catch { /* device may already be gone */ }

        _stream = null;
        _deviceInfo = "";
        IsInitialized = false;
        return Task.CompletedTask;
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, SimpleColor> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!IsInitialized || _stream == null)
            return Task.FromResult(false);

        _oled.Tick(_stream, _report.Length, LogError);

        _framesIn++;
        if (_statsWatch.ElapsedMilliseconds >= 10000)
        {
            LogInfo($"frames in={_framesIn} skipped={_framesSkipped} written={_framesWritten} (10s window)");
            _framesIn = _framesSkipped = _framesWritten = 0;
            _statsWatch.Restart();
        }

        Array.Clear(_report, 0, _report.Length);
        _hidSlot.Clear();
        _report[1] = _modelByte;

        var ledCount = 0;
        var maxLeds = (_report.Length - 3) / 4;
        foreach (var (key, color) in keyColors)
        {
            // The board follows the standard USB HID usage table (probe-verified: 0xE5=RShift,
            // 0xE7=RWin); only the apex/context key is vendor-specific at 0xF0.
            var hid = key == DeviceKeys.APPLICATION_SELECT ? (byte)0xF0 : SteelSeriesDevice.GetHIDCode(key);
            // 0x00 (legacy LOGO) floods the whole board; 0xE8-0xEF are GameSense G-key/logo
            // aliases that do not exist as LEDs on Gen 3 hardware.
            if (hid == 0x00 || hid == (byte)USBHIDCodes.ERROR || hid is >= 0xE8 and <= 0xEF)
                continue;

            var corrected = CommonColorUtils.CorrectWithAlpha(color);
            // Several DeviceKeys alias to one hid code (TILDE/OEM variants, slash variants);
            // unrendered aliases are black and must not overwrite a lit key.
            if (_hidSlot.TryGetValue(hid, out var slot))
            {
                if (corrected is { R: 0, G: 0, B: 0 })
                    continue;
                _report[slot + 1] = corrected.R;
                _report[slot + 2] = corrected.G;
                _report[slot + 3] = corrected.B;
                continue;
            }

            if (ledCount >= maxLeds)
                continue;

            var offset = 3 + ledCount * 4;
            _hidSlot[hid] = offset;
            _report[offset] = hid;
            _report[offset + 1] = corrected.R;
            _report[offset + 2] = corrected.G;
            _report[offset + 3] = corrected.B;
            ledCount++;
        }

        // The media/mute button next to the roller (led 251, "LCD Button") has no Aurora
        // DeviceKeys the layouts render; it sits directly above Num -, so follow that key.
        if (ledCount < maxLeds && keyColors.TryGetValue(DeviceKeys.NUM_MINUS, out var mediaColor))
        {
            var corrected = CommonColorUtils.CorrectWithAlpha(mediaColor);
            var offset = 3 + ledCount * 4;
            _report[offset] = 251;
            _report[offset + 1] = corrected.R;
            _report[offset + 2] = corrected.G;
            _report[offset + 3] = corrected.B;
            ledCount++;
        }

        _report[2] = (byte)ledCount;

        if (!forced && _report.AsSpan().SequenceEqual(_prevReport))
        {
            _framesSkipped++;
            return Task.FromResult(true);
        }

        try
        {
            _stream!.SetFeature(_report);
            _framesWritten++;
            (_prevReport, _report) = (_report, _prevReport);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            LogError("failed to write colour report", ex);
            return Task.FromResult(false);
        }
    }
}

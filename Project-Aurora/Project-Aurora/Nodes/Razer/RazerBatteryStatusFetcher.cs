using System;
using System.Linq;
using System.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.OnlineConfigs.Model;

namespace AuroraRgb.Nodes.Razer;

sealed class RazerBatteryStatusFetcher : RazerFetcher
{
    public bool MouseBatteryCharging { get; private set; }
    private readonly Timer _timer;

    public RazerBatteryStatusFetcher()
    {
        _timer = new Timer(_ => TimerUpdate(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20));
    }

    private byte[] BatteryStatusMessage(RazerMouseHidInfo mouseHidInfo)
    {
        var tid = byte.Parse(mouseHidInfo.TransactionId.Split('x')[1], System.Globalization.NumberStyles.HexNumber);
        var header = new byte[] { 0x00, tid, 0x00, 0x00, 0x00, 0x02, 0x07, 0x84 };

        var crc = 0;
        for (var i = 2; i < header.Length; i++)
        {
            crc ^= header[i];
        }

        var data = new byte[80];
        var crcData = new byte[] { (byte)crc, 0 };

        return header.Concat(data).Concat(crcData).ToArray();
    }

    private void TimerUpdate()
    {
        try
        {
            UpdateBatteryStatus();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "RazerBatteryStatusFetcher update error");
        }
    }

    private void UpdateBatteryStatus()
    {
        var usbDevice = GetUsbDevice();
        if (usbDevice == null)
        {
            return;
        }

        try
        {
            if (!Mutex.WaitOne(TimeSpan.FromMilliseconds(2000), true))
            {
                return;
            }
        }
        catch (AbandonedMutexException)
        {
            //continue
        }

        var mouseDictionary = OnlineSettings.RazerDeviceInfo.MouseHidInfos;
        var mouseHidInfo = mouseDictionary[GetDeviceProductKeyString(usbDevice)];
        usbDevice.Open();
        var batteryStatus = GetReport(usbDevice, BatteryStatusMessage(mouseHidInfo));
        Mutex.ReleaseMutex();
        usbDevice.Close();
        usbDevice.Dispose();

        if (batteryStatus != null)
        {
            MouseBatteryCharging = batteryStatus[9] == 1;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _timer.Dispose();
    }
}
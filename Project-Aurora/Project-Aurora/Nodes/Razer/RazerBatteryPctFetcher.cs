using System;
using System.Linq;
using System.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.OnlineConfigs.Model;

namespace AuroraRgb.Nodes.Razer;

sealed class RazerBatteryPctFetcher : RazerFetcher
{
    public double MouseBatteryPercentage { get; private set; }

    private readonly Timer _timer;

    public RazerBatteryPctFetcher()
    {
        _timer = new Timer(_ => TimerUpdate(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30));
    }

    private byte[] BatteryLevelMessage(RazerMouseHidInfo mouseHidInfo)
    {
        var tid = byte.Parse(mouseHidInfo.TransactionId.Split('x')[1], System.Globalization.NumberStyles.HexNumber);
        var header = new byte[] { 0x00, tid, 0x00, 0x00, 0x00, 0x02, 0x07, 0x80 };

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
            UpdateBatteryPct();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "RazerBatteryPctFetcher update error");
        }
    }

    private void UpdateBatteryPct()
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
        var batteryLevel = GetReport(usbDevice, BatteryLevelMessage(mouseHidInfo));
        Mutex.ReleaseMutex();
        usbDevice.Close();
        usbDevice.Dispose();

        if (batteryLevel != null)
        {
            MouseBatteryPercentage = batteryLevel[9] / 255d;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _timer.Dispose();
    }
}
using System;
using System.Linq;
using System.Threading;
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

    protected override byte[] GetMessage(RazerMouseHidInfo mouseHidInfo)
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
        var batteryStatusReport = Update();

        if (batteryStatusReport != null)
        {
            MouseBatteryCharging = batteryStatusReport[9] == 1;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _timer.Dispose();
    }
}
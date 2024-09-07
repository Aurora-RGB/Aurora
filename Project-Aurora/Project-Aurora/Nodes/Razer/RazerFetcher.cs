using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.OnlineConfigs.Model;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace AuroraRgb.Nodes.Razer;

internal abstract class RazerFetcher : IDisposable
{
    private const int HidReqSetReport = 0x09;
    private const int HidReqGetReport = 0x01; // Add GET_REPORT request
    private const int UsbTypeClass = 0x20;
    private const int UsbRecipInterface = 0x01;
    private const int UsbDirOut = 0x00;
    private const int UsbDirIn = 0x80; // Direction IN for reading
    private const int UsbTypeRequestOut = UsbTypeClass | UsbRecipInterface | UsbDirOut;
    private const int UsbTypeRequestIn = UsbTypeClass | UsbRecipInterface | UsbDirIn;
    private const int RazerUsbReportLen = 90; // Example length, set this according to actual length

    private readonly Mutex _mutex = new(false, "Global\\RazerLinkReadWriteGuardMutex");

    static RazerFetcher()
    {
        UsbDevice.ForceLibUsbWinBack = true;
    }

    protected RazerFetcher()
    {
        var rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
            MutexRights.Synchronize | MutexRights.Modify | MutexRights.FullControl | MutexRights.ReadPermissions | MutexRights.TakeOwnership, 
            AccessControlType.Allow);
        
        var mutexSecurity = new MutexSecurity();
        mutexSecurity.AddAccessRule(rule);
        
        _mutex.SetAccessControl(mutexSecurity);
    }

    protected byte[]? Update()
    {
        try
        {
            if (!_mutex.WaitOne(TimeSpan.FromMilliseconds(2000), true))
            {
                return null;
            }
        }
        catch (AbandonedMutexException)
        {
            //continue
        }

        var usbDevice = GetUsbDevice();
        if (usbDevice == null)
        {
            _mutex.ReleaseMutex();
            return null;
        }
   
        var productKeyString = GetDeviceProductKeyString(usbDevice);
        var mouseHidInfo = OnlineSettings.RazerDeviceInfo.MouseHidInfos[productKeyString];
        var message = GetMessage(mouseHidInfo);

        usbDevice.Open();
        var report = GetReport(usbDevice, message);
        usbDevice.Close();
        UsbDevice.Exit();
        _mutex.ReleaseMutex();

        return report;
    }

    protected abstract byte[] GetMessage(RazerMouseHidInfo mouseHidInfo);

    private static UsbDevice? GetUsbDevice()
    {
        const int vendorId = 0x1532;
        var mouseDictionary = OnlineSettings.RazerDeviceInfo.MouseHidInfos;

        var usbDevice = UsbDevice.OpenUsbDevice(d =>
            d.Vid == vendorId &&
            mouseDictionary.ContainsKey(GetDeviceProductKeyString(d)));
        if (usbDevice == null)
        {
            return null;
        }
        if (!usbDevice.IsOpen)
        {
            usbDevice.Open();
        }
        return usbDevice;
    }

    private static byte[]? GetReport(UsbDevice usbDevice, byte[] msg)
    {
        RazerSendControlMsg(usbDevice, msg, 0x09);
        Thread.Sleep(50);
        var res = RazerReadResponseMsg(usbDevice, 0x01);
        return res;
    }

    private static void RazerSendControlMsg(UsbDevice usbDev, byte[] data, uint reportIndex)
    {
        const ushort value = 0x300;

        var setupPacket = new UsbSetupPacket(UsbTypeRequestOut, HidReqSetReport, value, (ushort)reportIndex, (ushort)data.Length);

        // Send USB control message
        usbDev.ControlTransfer(ref setupPacket, data, RazerUsbReportLen, out _);
    }

    private static byte[]? RazerReadResponseMsg(UsbDevice usbDev, uint reportIndex)
    {
        const ushort value = 0x300;
        var responseBuffer = new byte[RazerUsbReportLen];

        var setupPacket = new UsbSetupPacket(UsbTypeRequestIn, HidReqGetReport, value, (ushort)reportIndex, (ushort)responseBuffer.Length);

        // Receive USB control message
        var transferredLength = responseBuffer.Length;
        usbDev.ControlTransfer(ref setupPacket, responseBuffer, RazerUsbReportLen, out _);
        
        return transferredLength != responseBuffer.Length ? null : responseBuffer;
    }

    private static string GetDeviceProductKeyString(UsbDevice device)
    {
        return "0x"+device.Info.Descriptor.ProductID.ToString("X4");
    }

    private static string GetDeviceProductKeyString(UsbRegistry device)
    {
        return "0x"+device.Pid.ToString("X4");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        _mutex.Close();
        _mutex.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
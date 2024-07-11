using System;
using System.Threading;
using AuroraRgb.Modules;
using LibUsbDotNet.LibUsb;
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

    protected readonly Mutex Mutex = new(false, "Global\\RazerLinkReadWriteGuardMutex");

    protected static IUsbDevice? GetUsbDevice()
    {
        const int vendorId = 0x1532;
        var mouseDictionary = OnlineSettings.RazerDeviceInfo.MouseHidInfos;
   
        using var context = new UsbContext();
        var usbDevice = context.Find(d =>
            d.VendorId == vendorId &&
            mouseDictionary.ContainsKey(GetDeviceProductKeyString(d)));
        return usbDevice;
    }

    protected static byte[]? GetReport(IUsbDevice usbDevice, byte[] msg)
    {
        RazerSendControlMsg(usbDevice, msg, 0x09);
        Thread.Sleep(50);
        var res = RazerReadResponseMsg(usbDevice, 0x01);
        return res;
    }

    private static void RazerSendControlMsg(IUsbDevice usbDev, byte[] data, uint reportIndex)
    {
        const ushort value = 0x300;

        var setupPacket = new UsbSetupPacket(UsbTypeRequestOut, HidReqSetReport, value, (ushort)reportIndex, (ushort)data.Length);

        // Send USB control message
        var transferredLength = data.Length;
        usbDev.ControlTransfer(setupPacket, data, 0, transferredLength);
    }

    private static byte[]? RazerReadResponseMsg(IUsbDevice usbDev, uint reportIndex)
    {
        const ushort value = 0x300;
        var responseBuffer = new byte[RazerUsbReportLen];

        var setupPacket = new UsbSetupPacket(UsbTypeRequestIn, HidReqGetReport, value, (ushort)reportIndex, (ushort)responseBuffer.Length);

        // Receive USB control message
        var transferredLength = responseBuffer.Length;
        var ec = usbDev.ControlTransfer(setupPacket, responseBuffer, 0, transferredLength);
        if (ec == 0)
        {
            return null;
        }
        
        return transferredLength != responseBuffer.Length ? null : responseBuffer;
    }

    protected static string GetDeviceProductKeyString(IUsbDevice device)
    {
        return "0x"+device.ProductId.ToString("X4");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        Mutex.Close();
        Mutex.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
// Based on https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11.1/ScreenCapture/Program.cs

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using Common.Utils;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace AuroraRgb.Settings.Layers.Ambilight;

public sealed class DesktopDuplicator : IDisposable
{
    public bool IsDisposed { get; private set; }

    private readonly Device _device;
    private readonly Texture2D _desktopImageTexture;
    private readonly Lazy<OutputDuplication> _deskDupl;

    public DesktopDuplicator(Output5 output)
    {
        WeakEventManager<Output5, EventArgs>.AddHandler(output, nameof(output.Disposing), (_, _) =>
        {
            Dispose();
        });
        var deviceFlags = DeviceCreationFlags.SingleThreaded;
        if (Global.isDebug)
        {
            deviceFlags &= DeviceCreationFlags.Debug;
        }
        _device = new Device(DriverType.Hardware, deviceFlags);
        
        Global.logger.Information("Starting desktop duplicator");
        if (Global.isDebug)
        {
            _device.ExceptionMode = 1;
        }
        var textureDesc = new Texture2DDescription
        {
            CpuAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = output.Description.DesktopBounds.Right - output.Description.DesktopBounds.Left,
            Height = output.Description.DesktopBounds.Bottom - output.Description.DesktopBounds.Top,
            OptionFlags = ResourceOptionFlags.Guarded,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = ResourceUsage.Staging,
        };

        _deskDupl = new Lazy<OutputDuplication>( () =>
        {
            try
            {
                return output.DuplicateOutput1(_device, 0, 1, [Format.B8G8R8A8_UNorm]);
            }
            catch
            {
                DesktopUtils.ResetDpiAwareness();
                throw;
            }
        });
        _desktopImageTexture = new Texture2D(_device, textureDesc);
    }

    public Bitmap? Capture(Rectangle desktopRegion, Bitmap screenBitmap, int timeout)
    {
        if (IsDisposed || _device.IsDisposed) 
            return null;

        try
        {
            var deskDupl = _deskDupl.Value;
            if (deskDupl is { IsDisposed: true })
            {
                return null;
            }
            var tryAcquireNextFrame = deskDupl.TryAcquireNextFrame(timeout, out var frameInformation, out var desktopResource);
            deskDupl.ReleaseFrame();
            if (tryAcquireNextFrame.Failure || frameInformation.LastPresentTime == 0)
            {
                return null;
            }

            tryAcquireNextFrame.CheckError();
            using var tempTexture = desktopResource.QueryInterface<Texture2D>();
            _device.ImmediateContext.CopyResource(tempTexture, _desktopImageTexture);
            desktopResource.Dispose();

            var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read, MapFlags.None);
            return mapSource.IsEmpty ? null : ProcessFrame(mapSource, desktopRegion, screenBitmap);
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.WaitTimeout)
        {
            Global.logger.Information("Timeout of {Timeout}ms exceeded while acquiring next frame", timeout);
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessLost)
        {
            // Can happen when going fullscreen / exiting fullscreen
            Global.logger.Information(e, "DesktopDuplicator access lost");
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessDenied)
        {
            // Happens when locking PC
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.InvalidCall)
        {
            Global.logger.Information(e, "DesktopDuplicator InvalidCall");
            return null;
        }
        catch (SharpDXException e) when (e.ResultCode.Failure)
        {
            Global.logger.Warning(e, "SharpDX exception in DesktopDuplicator");
            return null;
        }
        finally
        {
            if (_device is { IsDisposed: false, ImmediateContext.IsDisposed: false } && !_desktopImageTexture.IsDisposed)
            {
                _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
            }
            else
            {
                Global.logger.Information("Not unmapping desktop texture");
            }
        }
    }

    private static Bitmap ProcessFrame(DataBox mapSource, Rectangle rect, Bitmap frame)
    {
        var sourcePtr = mapSource.DataPointer;
        var sourceRowPitch = mapSource.RowPitch;

        // Copy pixels from screen capture Texture to GDI bitmap
        var mapDest = frame.LockBits(rect with {X = 0, Y = 0}, ImageLockMode.WriteOnly, frame.PixelFormat);

        var destPtr = mapDest.Scan0;
        var stride = mapDest.Stride;

        sourcePtr = IntPtr.Add(sourcePtr, rect.Y * sourceRowPitch);
        sourcePtr = IntPtr.Add(sourcePtr, rect.X * 4);
        for (var y = 0; y < rect.Height; y++)
        {
            // Copy a single line 
            Utilities.CopyMemory(destPtr, sourcePtr, stride);

            // Advance pointers
            sourcePtr = IntPtr.Add(sourcePtr, sourceRowPitch);
            destPtr = IntPtr.Add(destPtr, stride);
        }
        // Release source and dest locks
        frame.UnlockBits(mapDest);

        return frame;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;

        if (_deskDupl.IsValueCreated)
        {
            var outputDuplication = _deskDupl.Value;
            if (!outputDuplication.IsDisposed)
            {
                outputDuplication.Dispose();
            }
        }
        if (!_desktopImageTexture.IsDisposed)
        {
            _desktopImageTexture.Dispose();
        }
    }
}
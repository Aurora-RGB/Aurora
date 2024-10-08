using System;
using System.Drawing;
using AuroraRgb.EffectsEngine;
using SkiaSharp;

namespace AuroraRgb.Bitmaps.Skia;

public class AuroraVulkanSkiaBitmap : AuroraSkiaBitmap
{
    //TODO move these to non-static to properly dispose
    private static readonly Win32VkContext VulkanContext = new();
    private static readonly GRVkBackendContext Ctx = new()
    {
        VkInstance = (IntPtr) VulkanContext.Instance.RawHandle.ToUInt64(),
        VkPhysicalDevice = (IntPtr) VulkanContext.PhysicalDevice.RawHandle.ToUInt64(),
        VkDevice = (IntPtr) VulkanContext.Device.RawHandle.ToUInt64(),
        VkQueue = (IntPtr) VulkanContext.GraphicsQueue.RawHandle.ToUInt64(),
        GraphicsQueueIndex = VulkanContext.GraphicsFamily,
        GetProcedureAddress = VulkanContext.GetProc
    };
    private static readonly GRContext GrContext = GRContext.CreateVulkan(Ctx);
    
    public static readonly AuroraVulkanSkiaBitmap EmptyBitmap = new(1, 1, true);

    private readonly SKSurface _surface;
    private readonly SKBitmap _localBitmap;
    private readonly SKImageInfo _skImageInfo;
    private bool _invalidated;

    public AuroraVulkanSkiaBitmap(int canvasWidth, int canvasHeight, bool readable) : base(canvasWidth, canvasHeight)
    {
        _skImageInfo = new SKImageInfo(canvasWidth, canvasHeight);
        _localBitmap = readable ? new SKBitmap(_skImageInfo) : EmptyBitmap._localBitmap;
        _surface = SKSurface.Create(GrContext, false, _skImageInfo);
        Canvas = _surface.Canvas;
    }

    protected override void Invalidate()
    {
        base.Invalidate();

        _invalidated = true;
    }

    public override Color GetRegionColor(Rectangle keyRectangleRectangle)
    {
        if (_invalidated)
        {
            LoadBitmap();
        }
        var skiaRectangle = SkiaRectangle(keyRectangleRectangle);
        return GetAverageColorInRectangle(_localBitmap, skiaRectangle);
    }

    private void LoadBitmap()
    {
        _surface.Flush();
        // Read the pixel data from the surface into the bitmap
        if (!_surface.ReadPixels(_skImageInfo, _localBitmap.GetPixels(), _skImageInfo.RowBytes, 0, 0))
            throw new InvalidOperationException("Failed to read pixels from surface.");

        _invalidated = false;
    }

    public override void DrawRectangle(EffectLayer brush)
    {
        var auroraSkiaBitmap = (AuroraVulkanSkiaBitmap)GetSkiaBitmap(brush.GetBitmap());
        SkPaint.Color = new SKColor(255, 255, 255, (byte)(auroraSkiaBitmap.Opacity * 255));
        var skiaBitmap = auroraSkiaBitmap._surface;
        Canvas.DrawSurface(skiaBitmap, 0, 0, SkPaint);
    }

    public override void DrawRectangle(EffectLayer brush, Rectangle dimension)
    {
        var auroraSkiaBitmap = (AuroraVulkanSkiaBitmap)GetSkiaBitmap(brush.GetBitmap());
        SkPaint.Color = new SKColor(255, 255, 255, (byte)(auroraSkiaBitmap.Opacity * 255));
        var skiaBitmap = auroraSkiaBitmap._surface;
        var rectangle = SkiaRectangle(dimension);
        Canvas.DrawSurface(skiaBitmap, rectangle.Left, rectangle.Top, SkPaint);
    }

    private static AuroraSkiaBitmap GetSkiaBitmap(IAuroraBitmap bitmap)
    {
        return bitmap switch
        {
            AuroraSkiaBitmap skBitmap => skBitmap,
            RuntimeChangingBitmap runtimeChangingBitmap => runtimeChangingBitmap.GetSkiaVulkanBitmap(),
            _ => throw new NotSupportedException("Only AuroraVulkanSkiaBitmaps are supported.")
        };
    }
}
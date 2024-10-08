using System;
using SharpVk;
using SharpVk.Khronos;
using SkiaSharp;

namespace AuroraRgb.Bitmaps.Skia;

public abstract class VkContext : IDisposable
{
    public Instance Instance { get; protected set; }

    public PhysicalDevice PhysicalDevice { get; protected set; }

    public Surface Surface { get; protected set; }

    public Device Device { get; protected set; }

    public Queue GraphicsQueue { get; protected set; }

    public uint GraphicsFamily { get; protected set; }

    public uint PresentFamily { get; protected set; }

    public GRVkGetProcedureAddressDelegate GetProc { get; protected set; }

    public virtual void Dispose()
    {
        Instance?.Dispose();
    }
}
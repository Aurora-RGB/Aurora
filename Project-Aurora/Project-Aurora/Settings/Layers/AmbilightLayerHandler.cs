using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Amib.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Ambilight;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;
using PropertyChanged;
using Point = System.Drawing.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace AuroraRgb.Settings.Layers;

#region Enums
public enum AmbilightType
{
    [Description("Default")]
    Default = 0,

    [Description("Average color")]
    AverageColor = 1
}

public enum AmbilightCaptureType
{
    [Description("Coordinates")]
    Coordinates = 0,

    [Description("Entire Monitor")]
    EntireMonitor = 1,

    [Description("Foreground Application")]
    ForegroundApp = 2,

    [Description("Specific Process")]
    SpecificProcess = 3
}

public enum AmbilightFpsChoice
{
    [Description("Lowest")]
    VeryLow = 0,

    [Description("Low")]
    Low,

    [Description("Medium")]
    Medium,

    [Description("High")]
    High,

    [Description("Highest")]
    Highest,
}

#endregion

public partial class AmbilightLayerHandlerProperties : LayerHandlerProperties2Color
{
    private AmbilightType? _ambilightType;
    [JsonProperty("_AmbilightType")]
    public AmbilightType AmbilightType
    {
        get => Logic?._ambilightType ?? _ambilightType ?? AmbilightType.Default;
        set => SetFieldAndRaisePropertyChanged(out _ambilightType, value);
    }

    private AmbilightCaptureType? _ambilightCaptureType;
    [JsonProperty("_AmbilightCaptureType")]
    public AmbilightCaptureType AmbilightCaptureType
    {
        get => Logic?._ambilightCaptureType ?? _ambilightCaptureType ?? AmbilightCaptureType.EntireMonitor;
        set => SetFieldAndRaisePropertyChanged(out _ambilightCaptureType, value);
    }

    private int? _ambilightOutputId;
    [JsonProperty("_AmbilightOutputId")]
    public int AmbilightOutputId
    {
        get => Logic?._ambilightOutputId ?? _ambilightOutputId ?? 0;
        set => _ambilightOutputId = value;
    }

    private AmbilightFpsChoice? _ambiLightUpdatesPerSecond;
    [JsonProperty("_AmbiLightUpdatesPerSecond")]
    public AmbilightFpsChoice AmbiLightUpdatesPerSecond
    {
        get => Logic?._ambiLightUpdatesPerSecond ?? _ambiLightUpdatesPerSecond ?? AmbilightFpsChoice.Medium;
        set => SetFieldAndRaisePropertyChanged(out _ambiLightUpdatesPerSecond, value);
    }

    private string? _specificProcess;
    [JsonProperty("_SpecificProcess")]
    public string SpecificProcess
    {
        get => Logic?._specificProcess ?? _specificProcess ?? string.Empty;
        set => SetFieldAndRaisePropertyChanged(out _specificProcess, value);
    }

    private Rectangle? _coordinates;
    [JsonProperty("_Coordinates")]
    [LogicOverridable("Coordinates")] 
    public Rectangle Coordinates
    {
        get => Logic?._coordinates ?? _coordinates ?? Rectangle.Empty;
        set => SetFieldAndRaisePropertyChanged(out _coordinates, value);
    }

    private bool? _brightenImage;
    [JsonProperty("_BrightenImage")]
    public bool BrightenImage
    {
        get => Logic?._brightenImage ?? _brightenImage ?? false;
        set => SetFieldAndRaisePropertyChanged(out _brightenImage, value);
    }

    private float? _brightnessChange;
    [JsonProperty("_BrightnessChange")]
    public float BrightnessChange
    {
        get => Logic?._brightnessChange ?? _brightnessChange ?? 0.0f;
        set => SetFieldAndRaisePropertyChanged(out _brightnessChange, value);
    }

    private bool? _saturateImage;
    [JsonProperty("_SaturateImage")]
    public bool SaturateImage
    {
        get => Logic?._saturateImage ?? _saturateImage ?? false;
        set => SetFieldAndRaisePropertyChanged(out _saturateImage, value);
    }

    private float? _saturationChange;
    [JsonProperty("_SaturationChange")]
    public float SaturationChange
    {
        get => Logic?._saturationChange ?? _saturationChange ?? 0.0f;
        set => SetFieldAndRaisePropertyChanged(out _saturationChange, value);
    }

    private bool? _flipVertically;
    [JsonProperty("_FlipVertically")]
    public bool FlipVertically
    {
        get => Logic?._flipVertically ?? _flipVertically ?? false;
        set => SetFieldAndRaisePropertyChanged(out _flipVertically, value);
    }

    private bool? _experimentalMode;
    [JsonProperty("_ExperimentalMode")]
    public bool ExperimentalMode
    {
        get => Logic?._experimentalMode ?? _experimentalMode ?? false;
        set => SetFieldAndRaisePropertyChanged(out _experimentalMode, value);
    }

    private bool? _hueShiftImage;
    [JsonProperty("_HueShiftImage")]
    public bool HueShiftImage
    {
        get => Logic?._hueShiftImage ?? _hueShiftImage ?? false;
        set => SetFieldAndRaisePropertyChanged(out _hueShiftImage, value);
    }

    private float? _hueShiftAngle;
    [JsonProperty("_HueShiftAngle")]
    public float HueShiftAngle
    {
        get => Logic?._hueShiftAngle ?? _hueShiftAngle ?? 0.0f;
        set => SetFieldAndRaisePropertyChanged(out _hueShiftAngle, value);
    }

    public override void Default()
    {
        base.Default();
        _ambilightOutputId = 0;
        _ambiLightUpdatesPerSecond = AmbilightFpsChoice.Medium;
        _ambilightType = AmbilightType.Default;
        _ambilightCaptureType = AmbilightCaptureType.EntireMonitor;
        _specificProcess = "";
        _coordinates = new Rectangle(0, 0, 0, 0);
        _brightenImage = false;
        _brightnessChange = 1.0f;
        _saturateImage = false;
        _saturationChange = 1.0f;
        _flipVertically = false;
        _experimentalMode = false;
        _hueShiftImage = false;
        _hueShiftAngle = 0.0f;
        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("SecondaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[DoNotNotify]
public sealed class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>
{
    private readonly Temporary<IScreenCapture> _screenCapture;

    private readonly SmartThreadPool _captureWorker;
    private readonly WorkItemCallback _screenshotWork;

    private readonly object _brushLock = new();
    private Brush _screenBrush = Brushes.Transparent;
    private IntPtr _specificProcessHandle = IntPtr.Zero;
    private Rectangle _cropRegion = Rectangle.Empty;
    private Bitmap _screenBitmap = new(8, 8);
    private ImageAttributes _imageAttributes = new();

    private bool _brushChanged = true;

    private readonly Stopwatch _captureStopwatch = new();
    private DateTime _lastProcessDetectTry = DateTime.UtcNow;

    public IEnumerable<string> Displays => _screenCapture.Value.GetDisplays();

    public AmbilightLayerHandler() : base("Ambilight Layer")
    {
        var stpStartInfo = new STPStartInfo
        {
            ApartmentState = ApartmentState.STA,
            IdleTimeout = 1000
        };

        _captureWorker = new SmartThreadPool(stpStartInfo)
        {
            MaxThreads = 1,
            Name = "Ambilight Screenshot"
        };
        _screenshotWork = TakeScreenshot;

        _screenCapture = new Temporary<IScreenCapture>(() =>
        {
            IScreenCapture screenCapture = Properties.ExperimentalMode ? new DxScreenCapture() : new GdiScreenCapture();
            screenCapture.ScreenshotTaken += ScreenshotAction;
            return screenCapture;
        });
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (Properties.Sequence.GetAffectedRegion().IsEmpty)
            return EffectLayer.EmptyLayer;

        if (_captureWorker.WaitingCallbacks < 1)
        {
            _captureWorker.QueueWorkItem(_screenshotWork);
        }

        //This is needed to prevent the layer from disappearing
        //for a frame when the user alt-tabs with the foregroundapp option selected
        if (!TryGetCropRegion(out _))
        {
            if (DateTime.UtcNow - _lastProcessDetectTry > TimeSpan.FromSeconds(2))
            {
                UpdateSpecificProcessHandle(Properties.SpecificProcess);
                _lastProcessDetectTry = DateTime.UtcNow;
            }
        }

        //and because of that, this should never happen 
        var effectLayer = EffectLayer;
        if (_cropRegion.IsEmpty)
            return effectLayer;

        if (!_brushChanged)
        {
            return effectLayer;
        }

        if (Invalidated)
        {
            effectLayer.Clear();
            Invalidated = false;
        }

        lock (_brushLock)
        {
            effectLayer.DrawTransformed(Properties.Sequence,
                m =>
                {
                    if (!Properties.FlipVertically) return;
                    m.Scale(1, -1, MatrixOrder.Prepend);
                    m.Translate(0, -_cropRegion.Height, MatrixOrder.Prepend);
                },
                g =>
                {
                    try
                    {
                        effectLayer.Clear();
                        g.DrawRectangle(_screenBrush, _cropRegion with{ X = 0, Y = 0});
                    }
                    catch
                    {
                        //rarely matrix
                    }
                },
                _cropRegion with {X = 0, Y = 0}
            );
        }
        _brushChanged = false;

        return effectLayer;
    }

    protected override UserControl CreateControl()
    {
        return new Control_AmbilightLayer(this);
    }

    private object? TakeScreenshot(object? sender)
    {
        try
        {
            TryTakeScreenshot();
        }
        catch(Exception e) {
            Global.logger.Error(e, "Ambilight Screenshot Error");
            Thread.Sleep(2000);
        }
        return null;
    }

    private void TryTakeScreenshot()
    {
        if (TryGetCropRegion(out var newCropRegion))
        {
            if (_cropRegion != newCropRegion)
            {
                var screenBitmap = _screenBitmap;
                _screenBitmap = new Bitmap(newCropRegion.Width, newCropRegion.Height);
                screenBitmap.Dispose();
            }
            _cropRegion = newCropRegion;
        }
        
        _screenCapture.Value.Capture(_cropRegion, _screenBitmap);
        WaitTimer(_captureStopwatch.Elapsed);
        _captureStopwatch.Restart();
    }

    private void ScreenshotAction(object? sender, Bitmap bitmap)
    {
        CreateScreenBrush(bitmap, _cropRegion);
    }

    private void WaitTimer(TimeSpan elapsed)
    {
        var screenshotInterval = GetIntervalFromFps(Properties.AmbiLightUpdatesPerSecond);
        if (screenshotInterval > elapsed)
            Thread.Sleep(screenshotInterval - elapsed);
        else
            Thread.Sleep(screenshotInterval);
    }

    //B, G, R, A
    private static readonly long[] ColorData = [0L, 0L, 0L, 0L];
    private void CreateScreenBrush(Bitmap screenCapture, Rectangle cropRegion)
    {
        switch (Properties.AmbilightType)
        {
            case AmbilightType.Default:
                lock (_brushLock)
                {
                    var previousBrush = _screenBrush;
                    _screenBrush = new TextureBrush(screenCapture, new Rectangle(0, 0, screenCapture.Width, screenCapture.Height), _imageAttributes);
                    previousBrush.Dispose();
                }
                _brushChanged = true;
                break;
            case AmbilightType.AverageColor:
                var average = BitmapUtils.GetRegionColor(screenCapture, new Rectangle(Point.Empty, cropRegion.Size), ColorData);

                if (Properties.BrightenImage)
                    average = CommonColorUtils.ChangeBrightness(average, Properties.BrightnessChange);
                if (Properties.SaturateImage)
                    average = CommonColorUtils.ChangeSaturation(average, Properties.SaturationChange);
                if (Properties.HueShiftImage)
                    average = CommonColorUtils.ChangeHue(average, Properties.HueShiftAngle);

                if (average is { R: 0, G: 0, B: 0 })
                {
                    return;
                }

                lock (_brushLock)
                {
                    _screenBrush.Dispose();
                    _screenBrush = new SolidBrush(average);
                }
                _brushChanged = true;
                break;
        }
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        var mtx = BitmapUtils.GetEmptyColorMatrix();
        if (Properties.BrightenImage)
            mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetBrightnessColorMatrix(Properties.BrightnessChange));
        if (Properties.SaturateImage)
            mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetSaturationColorMatrix(Properties.SaturationChange));
        if (Properties.HueShiftImage)
            mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetHueShiftColorMatrix(Properties.HueShiftAngle));
        _imageAttributes = new ImageAttributes();
        _imageAttributes.SetColorMatrix(new ColorMatrix(mtx));
        _imageAttributes.SetWrapMode(WrapMode.Clamp);

        switch (args.PropertyName)
        {
            case nameof(Properties.AmbilightCaptureType):
            {
                var activeProcessMonitor = ProcessesModule.ActiveProcessMonitor.Result;
                activeProcessMonitor.ActiveProcessChanged -= ProcessChanged;
                _screenCapture.Value.WindowListener.WindowCreated -= WindowsChanged;
                _screenCapture.Value.WindowListener.WindowDestroyed -= WindowsChanged;
                switch (Properties.AmbilightCaptureType)
                {
                    case AmbilightCaptureType.SpecificProcess when !string.IsNullOrWhiteSpace(Properties.SpecificProcess):
                        UpdateSpecificProcessHandle(Properties.SpecificProcess);
                
                        _screenCapture.Value.WindowListener.WindowCreated += WindowsChanged;
                        _screenCapture.Value.WindowListener.WindowDestroyed += WindowsChanged;
                        break;
                    case AmbilightCaptureType.ForegroundApp:
                        _specificProcessHandle = User32.GetForegroundWindow();
                        activeProcessMonitor.ActiveProcessChanged += ProcessChanged;
                        break;
                    case AmbilightCaptureType.Coordinates:
                    case AmbilightCaptureType.EntireMonitor:
                    default:
                        break;
                }
            
                // the instance will be recreated
                _screenCapture.Dispose();
                break;
            }
            case nameof(Properties.ExperimentalMode):
                // the instance will be recreated
                _screenCapture.Dispose();
                break;
        }
    }

    private void ProcessChanged(object? sender, EventArgs e)
    {
        _specificProcessHandle = User32.GetForegroundWindow();
    }

    private void WindowsChanged(object? sender, WindowEventArgs e)
    {
        if (e.Opened && (IsSelectedProcess() || CaptureForeground()))
        {
            _specificProcessHandle = new IntPtr(e.WindowHandle);
        }

        if (!_screenCapture.Value.WindowListener.ProcessWindowsMap.TryGetValue(Properties.SpecificProcess, out var windows)) return;

        var targetWindow = windows.First();
        _specificProcessHandle = new IntPtr(targetWindow.WindowHandle);

        bool IsSelectedProcess()
        {
            return e.ProcessName == Properties.SpecificProcess;
        }

        bool CaptureForeground()
        {
            return Properties.AmbilightCaptureType == AmbilightCaptureType.ForegroundApp;
        }
    }

    #region Helper Methods
    /// <summary>
    /// Gets the region to crop based on user preference and current display.
    /// Switches display if the desired coordinates are offscreen.
    /// </summary>
    /// <returns></returns>
    private bool TryGetCropRegion(out Rectangle crop)
    {
        crop = new Rectangle();

        switch (Properties.AmbilightCaptureType)
        {
            case AmbilightCaptureType.EntireMonitor:
                //we're using the whole screen, so we don't crop at all
                crop = Screen.AllScreens[Properties.AmbilightOutputId].Bounds;
                break;
            case AmbilightCaptureType.Coordinates:
                var screenBounds = Screen.AllScreens[Properties.AmbilightOutputId].Bounds;
                crop = new Rectangle(
                    Properties.Coordinates.Left - screenBounds.Left,
                    Properties.Coordinates.Top - screenBounds.Top,
                    Math.Abs(Properties.Coordinates.Width),
                    Math.Abs(Properties.Coordinates.Height));
                break;
            case AmbilightCaptureType.SpecificProcess:
            case AmbilightCaptureType.ForegroundApp:
                var handle = _specificProcessHandle;
                if (handle == IntPtr.Zero)
                    return false;//happens when alt tabbing

                var appRect = DwmApi.GetWindowRectangle(handle);

                crop = new Rectangle(
                    appRect.Left,
                    appRect.Top,
                    Math.Abs(appRect.Right - appRect.Left),
                    Math.Abs(appRect.Bottom - appRect.Top));

                break;
        }

        return crop is { Width: > 4, Height: > 4 };
    }

    /// <summary>
    /// Returns an interval in ms usign the AmbilightFpsChoice enum.
    /// Higher values result in lower intervals
    /// </summary>
    /// <param name="fps"></param>
    /// <returns></returns>
    private static TimeSpan GetIntervalFromFps(AmbilightFpsChoice fps) => new(0, 0,0,  0, 1000 / (10 + 5 * (int)fps));

    /// <summary>
    /// Updates the handle of the window used to crop the screenshot
    /// </summary>
    /// <param name="process"></param>
    public void UpdateSpecificProcessHandle(string process)
    {
        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process));
        try
        {
            if (processes.Length == 0)
            {
                return;
            }
            var targetProcess = Array.Find(
                processes,
                p => p.MainWindowHandle != IntPtr.Zero
            );
            if (targetProcess != null)  //target process is there but doesn't have window yet
            {
                _specificProcessHandle = targetProcess.MainWindowHandle;
            }
        }
        finally
        {
            foreach (var p in processes)
            {
                p.Close();
            }
        }
    }

    #endregion

    public override void Dispose()
    {
        if (!_screenCapture.HasValue) _screenCapture.Value.ScreenshotTaken -= ScreenshotAction;
        _screenCapture.Dispose();

        base.Dispose();
    }
}
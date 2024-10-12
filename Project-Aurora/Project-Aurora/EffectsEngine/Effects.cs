using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AuroraRgb.Bitmaps;
using AuroraRgb.BrushAdapters;
using AuroraRgb.Devices;
using AuroraRgb.Utils;
using Common;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public delegate void NewLayerRendered(IAuroraBitmap bitmap);

internal class EnumHashGetter: IEqualityComparer<Enum>
{
    public static readonly EnumHashGetter Instance = new();

    private EnumHashGetter()
    {
    }

    public bool Equals(Enum? x, Enum? y)
    {
        return object.Equals(x, y);
    }

    public int GetHashCode(Enum obj)
    {
        return Convert.ToInt32(obj);
    }
}

public class Effects(Task<DeviceManager> deviceManager)
{
    //Optimization: used to mitigate dictionary resizing
    public static readonly int MaxDeviceId = Enum.GetValues(typeof(DeviceKeys)).Cast<int>().Max() + 1;

    private static readonly DeviceKeys[] PossiblePeripheralKeys =
    [
        DeviceKeys.Peripheral,
        DeviceKeys.Peripheral_FrontLight,
        DeviceKeys.Peripheral_ScrollWheel,
        DeviceKeys.Peripheral_Logo,
        DeviceKeys.MOUSEPADLIGHT1,
        DeviceKeys.MOUSEPADLIGHT2,
        DeviceKeys.MOUSEPADLIGHT3,
        DeviceKeys.MOUSEPADLIGHT4,
        DeviceKeys.MOUSEPADLIGHT5,
        DeviceKeys.MOUSEPADLIGHT6,
        DeviceKeys.MOUSEPADLIGHT7,
        DeviceKeys.MOUSEPADLIGHT8,
        DeviceKeys.MOUSEPADLIGHT9,
        DeviceKeys.MOUSEPADLIGHT1,
        DeviceKeys.MOUSEPADLIGHT2,
        DeviceKeys.MOUSEPADLIGHT3,
        DeviceKeys.MOUSEPADLIGHT4,
        DeviceKeys.MOUSEPADLIGHT5,
        DeviceKeys.MOUSEPADLIGHT6,
        DeviceKeys.MOUSEPADLIGHT7,
        DeviceKeys.MOUSEPADLIGHT8,
        DeviceKeys.MOUSEPADLIGHT9,
        DeviceKeys.MOUSEPADLIGHT10,
        DeviceKeys.MOUSEPADLIGHT11,
        DeviceKeys.MOUSEPADLIGHT12,
        DeviceKeys.MOUSEPADLIGHT13,
        DeviceKeys.MOUSEPADLIGHT14,
        DeviceKeys.MOUSEPADLIGHT15,
        DeviceKeys.MOUSEPADLIGHT16,
        DeviceKeys.MOUSEPADLIGHT17,
        DeviceKeys.MOUSEPADLIGHT18,
        DeviceKeys.MOUSEPADLIGHT19,
        DeviceKeys.MOUSEPADLIGHT20,
        DeviceKeys.PERIPHERAL_LIGHT1,
        DeviceKeys.PERIPHERAL_LIGHT2,
        DeviceKeys.PERIPHERAL_LIGHT3,
        DeviceKeys.PERIPHERAL_LIGHT4,
        DeviceKeys.PERIPHERAL_LIGHT5,
        DeviceKeys.PERIPHERAL_LIGHT6,
        DeviceKeys.PERIPHERAL_LIGHT7,
        DeviceKeys.PERIPHERAL_LIGHT8,
        DeviceKeys.PERIPHERAL_LIGHT9,
        DeviceKeys.PERIPHERAL_LIGHT1,
        DeviceKeys.PERIPHERAL_LIGHT2,
        DeviceKeys.PERIPHERAL_LIGHT3,
        DeviceKeys.PERIPHERAL_LIGHT4,
        DeviceKeys.PERIPHERAL_LIGHT5,
        DeviceKeys.PERIPHERAL_LIGHT6,
        DeviceKeys.PERIPHERAL_LIGHT7,
        DeviceKeys.PERIPHERAL_LIGHT8,
        DeviceKeys.PERIPHERAL_LIGHT9,
        DeviceKeys.PERIPHERAL_LIGHT10,
        DeviceKeys.PERIPHERAL_LIGHT11,
        DeviceKeys.PERIPHERAL_LIGHT12,
        DeviceKeys.PERIPHERAL_LIGHT13,
        DeviceKeys.PERIPHERAL_LIGHT14,
        DeviceKeys.PERIPHERAL_LIGHT15,
        DeviceKeys.PERIPHERAL_LIGHT16,
        DeviceKeys.PERIPHERAL_LIGHT17,
        DeviceKeys.PERIPHERAL_LIGHT18,
        DeviceKeys.PERIPHERAL_LIGHT19,
        DeviceKeys.PERIPHERAL_LIGHT20
    ];

    public event NewLayerRendered? NewLayerRender = delegate { };

    private Bitmap? _forcedFrame;

    public static event EventHandler? CanvasChanged;
    private static readonly object CanvasChangedLock = new();

    private static EffectCanvas _canvas = new(8, 8,
        new Dictionary<DeviceKeys, BitmapRectangle>
        {
            { DeviceKeys.SPACE , new BitmapRectangle(0, 0, 8, 8)}
        }
    );
    public static EffectCanvas Canvas
    {
        get => _canvas;
        set
        {
            if (Equals(_canvas, value))
            {
                return;
            }
            lock (CanvasChangedLock)
            {
                _canvas = value;
                CanvasChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    private readonly Dictionary<DeviceKeys, SimpleColor> _keyColors = new(MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

    private ReadableEffectLayer Background { get; } = new("Global Background", Color.Black);

    private readonly SingleColorBrush _keyboardDarknessBrush = new();
    private readonly SingleColorBrush _blackBrush = new(SimpleColor.Black);

    public void ForceImageRender(Bitmap? forcedFrame)
    {
        _forcedFrame?.Dispose();
        _forcedFrame = forcedFrame?.Clone() as Bitmap;
    }

    public void PushFrame(EffectFrame frame)
    {
        lock (CanvasChangedLock)
        {
            PushFrameLocked(frame);
        }

        frame.Reset();
    }

    private void PushFrameLocked(EffectFrame frame)
    {
        Background.Fill(_blackBrush);

        var overLayersArray = frame.GetOverlayLayers();
        var layersArray = frame.GetLayers();

        foreach (var layer in layersArray)
            Background.Add(layer);
        foreach (var layer in overLayersArray)
            Background.Add(layer);

        var keyboardDarkness = 1.0f - Global.Configuration.KeyboardBrightness * Global.Configuration.GlobalBrightness;
        _keyboardDarknessBrush.Color = SimpleColor.FromRgba( 0, 0, 0, (byte) (255.0f * keyboardDarkness));
        Background.FillOver(_keyboardDarknessBrush);

        var renderCanvas = Canvas; // save locally in case it changes between ref calls
        if (_forcedFrame != null)
        {
            var g = Background.GetGraphics();
            g.Fill(Brushes.Black);
            g.DrawImage(_forcedFrame, 0, 0, renderCanvas.Width, renderCanvas.Height);
        }

        foreach (var key in renderCanvas.BitmapMap.Keys)
            _keyColors[key] = (SimpleColor)Background.Get(key);
        Background.Close();

        var peripheralDarkness = 1.0f - Global.Configuration.PeripheralBrightness * Global.Configuration.GlobalBrightness;
        foreach (var key in PossiblePeripheralKeys)
        {
            if (_keyColors.TryGetValue(key, out var color))
            {
                _keyColors[key] = ColorUtils.BlendColors(color, SimpleColor.Black, peripheralDarkness);
            }
        }

        deviceManager.Result.UpdateDevices(_keyColors);

        NewLayerRender?.Invoke(Background.GetBitmap());

        frame.Dispose();
    }

    public Dictionary<DeviceKeys, SimpleColor> GetKeyboardLights()
    {
        return _keyColors;
    }
}
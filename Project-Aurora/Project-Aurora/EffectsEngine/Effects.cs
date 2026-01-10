using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using AuroraRgb.Bitmaps;
using AuroraRgb.Devices;
using AuroraRgb.Utils;
using Common;
using Common.Devices;
using Common.Utils;

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
    //TODO https://github.com/skarllot/EnumUtilities/issues/469
    public const int MaxDeviceId = 2000;

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

    private event NewLayerRendered? _newLayerRender = delegate { };
    
    public event NewLayerRendered? NewLayerRender
    {
        add
        {
            if (_newLayerRender?.GetInvocationList().Length < 2)
            {
                Background.ChangeToBitmapEffectLayer();
            }
            _newLayerRender += value; // Update stored event listeners
        }
        remove
        {
            _newLayerRender -= value; // Update stored event listeners
            if (_newLayerRender?.GetInvocationList().Length < 2)
            {
                Background.ChangeToNoRenderLayer();
            }
        }
    }

    public static event EventHandler? CanvasChanged;
    public static readonly object CanvasChangedLock = new();

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

    private readonly DeviceKeyStore _keyColors = new();

    private RuntimeChangingLayer Background { get; } = new("Background Layer");
    private readonly Color _backgroundColor = Color.Black;

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
        Background.Fill(in _backgroundColor);

        var overLayersArray = frame.GetOverlayLayers();
        var layersArray = frame.GetLayers();

        foreach (var layer in layersArray)
            Background.Add(layer);
        foreach (var layer in overLayersArray)
            Background.Add(layer);

        var keyboardDarknessA = 1.0f - Global.Configuration.KeyboardBrightness * Global.Configuration.GlobalBrightness;
        var keyboardDarkness = CommonColorUtils.FastColor(0, 0, 0, (byte) (255.0f * keyboardDarknessA));
        Background.FillOver(in keyboardDarkness);

        var renderCanvas = Canvas; // save locally in case it changes between ref calls

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

        _newLayerRender?.Invoke(Background.GetBitmap());

        frame.Dispose();
    }

    public DeviceKeyStore GetKeyboardLights()
    {
        return _keyColors;
    }
}
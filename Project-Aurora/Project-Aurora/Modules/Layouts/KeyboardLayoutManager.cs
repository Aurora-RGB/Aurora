using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.Logitech;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls.Keycaps;
using AuroraRgb.Settings.Layouts;
using AuroraRgb.Utils;
using Common;
using Common.Devices;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;

namespace AuroraRgb.Modules.Layouts;

public sealed class KeyboardLayoutManager : IDisposable
{
    private const string CulturesFolder = "kb_layouts";

    public Dictionary<DeviceKeys, DeviceKeys> LayoutKeyConversion { get; private set; } = new();

    private VirtualGroup _virtualKeyboardGroup = new();

    private readonly Dictionary<DeviceKeys, Keycap> _virtualKeyboardMap = new();

    public Task<Grid> VirtualKeyboard { get; }

    public Task<Panel> AbstractVirtualKeyboard => CreateUserControlAbstract(_virtualKeyboardGroup, CancellationToken.None);

    public delegate void LayoutUpdatedEventHandler(object? sender);

    public event LayoutUpdatedEventHandler? KeyboardLayoutUpdated;

    public PreferredKeyboardLocalization LoadedLocalization { get; private set; } = PreferredKeyboardLocalization.None;

    private readonly string _layoutsPath;

    private readonly Task<ChromaSdkManager> _rzSdk;

    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _loadLinkLeds;
    private ChromaSdkManager? _chromaSdkManager;

    public KeyboardLayoutManager(Task<ChromaSdkManager> rzSdk)
    {
        _rzSdk = rzSdk;
        _layoutsPath = Path.Combine(Global.ExecutingDirectory, CulturesFolder);
        var vkTcs = new TaskCompletionSource<Grid>(TaskCreationOptions.RunContinuationsAsynchronously);
        VirtualKeyboard = vkTcs.Task;
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var grid = new Grid { Width = 8, Height = 8, MaxWidth = double.PositiveInfinity, MaxHeight = double.PositiveInfinity };
            vkTcs.SetResult(grid);
        }, DispatcherPriority.Loaded);
    }

    public async Task Initialize()
    {
        _chromaSdkManager = await _rzSdk;
        _loadLinkLeds = _chromaSdkManager.ChromaReader is not null || LogitechSdkModule.LogitechSdkListener.State != LightsyncSdkState.Disabled;

        _chromaSdkManager.StateChanged += ChromaSdkManagerOnStateChanged;
        LogitechSdkModule.LogitechSdkListener.StateChanged += LogitechSdkListenerOnStateChanged;

        await LoadBrandDefault();

        //TODO listen for online layout load
        Global.Configuration.PropertyChanged += Configuration_PropertyChanged;
    }

    private async void ChromaSdkManagerOnStateChanged(object? sender, ChromaSdkStateChangedEventArgs e)
    {
        await RefreshLinkLayout();
    }

    private async void LogitechSdkListenerOnStateChanged(object? sender, EventArgs e)
    {
        await RefreshLinkLayout();
    }

    private async Task RefreshLinkLayout()
    {
        var linksCurrentlyLoaded = _loadLinkLeds;
        _loadLinkLeds = _chromaSdkManager?.ChromaReader is not null || LogitechSdkModule.LogitechSdkListener.State != LightsyncSdkState.Disabled;

        if (linksCurrentlyLoaded != _loadLinkLeds)
        {
            await LoadBrandDefault();
        }
    }

    internal async Task LoadBrandDefault()
    {
        var cancellationTokenSource = _cancellationTokenSource;
        await cancellationTokenSource.CancelAsync();

        var newCancelSource = new CancellationTokenSource();
        var cancellationToken = newCancelSource.Token;
        _cancellationTokenSource = newCancelSource;
        
        var layout = Global.Configuration.KeyboardLocalization;
        var culture = layout switch
        {
            PreferredKeyboardLocalization.None => Thread.CurrentThread.CurrentCulture.Name,
            PreferredKeyboardLocalization.intl => "intl",
            PreferredKeyboardLocalization.us => "en-US",
            PreferredKeyboardLocalization.uk => "en-GB",
            PreferredKeyboardLocalization.ru => "ru-RU",
            PreferredKeyboardLocalization.fr => "fr-FR",
            PreferredKeyboardLocalization.de => "de-DE",
            PreferredKeyboardLocalization.jpn => "ja-JP",
            PreferredKeyboardLocalization.nordic => "nordic",
            PreferredKeyboardLocalization.tr => "tr-TR",
            PreferredKeyboardLocalization.swiss => "de-CH",
            PreferredKeyboardLocalization.abnt2 => "pt-BR",
            PreferredKeyboardLocalization.dvorak => "dvorak",
            PreferredKeyboardLocalization.dvorak_int => "dvorak_int",
            PreferredKeyboardLocalization.hu => "hu-HU",
            PreferredKeyboardLocalization.it => "it-IT",
            PreferredKeyboardLocalization.la => "es-AR",
            PreferredKeyboardLocalization.es => "es-ES",
            PreferredKeyboardLocalization.iso => "iso",
            PreferredKeyboardLocalization.ansi => "ansi",
            _ => Thread.CurrentThread.CurrentCulture.Name
        };

        LoadedLocalization = GetLocalization(culture);

        //Load keyboard layout
        if (!Directory.Exists(_layoutsPath))
        {
            cancellationTokenSource.Dispose();
            return;
        }

        var layoutLoad = new LayoutLoad(_layoutsPath, _loadLinkLeds);
        try
        {
            await layoutLoad.LoadBrand(
                cancellationToken,
                Global.Configuration.KeyboardBrand,
                Global.Configuration.MousePreference,
                Global.Configuration.MousepadPreference,
                Global.Configuration.MouseOrientation,
                Global.Configuration.HeadsetPreference,
                Global.Configuration.ChromaLedsPreference
            );
            CalculateBitmap(layoutLoad.VirtualKeyboardGroup, layoutLoad.KeyboardWidth, layoutLoad.KeyboardHeight);

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await CreateUserControl(layoutLoad.VirtualKeyboardGroup, cancellationToken);
                    KeyboardLayoutUpdated?.Invoke(this);
                }
                catch (Exception e)
                {
                    Global.logger.Error(e, "Keyboard control generation failed");
                }
            }, DispatcherPriority.Loaded, cancellationToken);
            _virtualKeyboardGroup = layoutLoad.VirtualKeyboardGroup;
            LayoutKeyConversion = layoutLoad.LayoutKeyConversion;
        }
        catch (OperationCanceledException)
        {
            // load cancelled, go on
        }

        cancellationTokenSource.Dispose();
    }

    private PreferredKeyboardLocalization GetLocalization(string culture)
    {
        switch (culture)
        {
            case "de-DE":
            case "hsb-DE":
            case "dsb-DE":
                return PreferredKeyboardLocalization.de;
            case "fr-CH":
            case "de-CH":
                return PreferredKeyboardLocalization.swiss;
            case "fr-FR":
            case "br-FR":
            case "oc-FR":
            case "co-FR":
            case "gsw-FR":
                return PreferredKeyboardLocalization.fr;
            case "cy-GB":
            case "gd-GB":
            case "en-GB":
                return PreferredKeyboardLocalization.uk;
            case "ru-RU":
            case "tt-RU":
            case "ba-RU":
            case "sah-RU":
                return PreferredKeyboardLocalization.ru;
            case "en-US":
                return PreferredKeyboardLocalization.us;
            case "da-DK":
            case "se-SE":
            case "nb-NO":
            case "nn-NO":
            case "nordic":
                return PreferredKeyboardLocalization.nordic;
            case "pt-BR":
                return PreferredKeyboardLocalization.abnt2;
            case "dvorak":
                return PreferredKeyboardLocalization.dvorak;
            case "dvorak_int":
                return PreferredKeyboardLocalization.dvorak_int;
            case "hu-HU":
                return PreferredKeyboardLocalization.hu;
            case "it-IT":
                return PreferredKeyboardLocalization.it;
            case "es-AR":
            case "es-BO":
            case "es-CL":
            case "es-CO":
            case "es-CR":
            case "es-EC":
            case "es-MX":
            case "es-PA":
            case "es-PY":
            case "es-PE":
            case "es-UY":
            case "es-VE":
            case "es-419":
                return PreferredKeyboardLocalization.la;
            case "es-ES":
                return PreferredKeyboardLocalization.es;
            case "iso":
                return PreferredKeyboardLocalization.iso;
            case "ansi":
                return PreferredKeyboardLocalization.ansi;
            default:
                return PreferredKeyboardLocalization.intl;
        }
    }

    private static int PixelToByte(double pixel)
    {
        return (int)Math.Round(pixel / (double)Global.Configuration.BitmapAccuracy);
    }

    private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        HashSet<string> relatedProperties =
        [
            nameof(Configuration.BitmapAccuracy),
            nameof(Configuration.VirtualkeyboardKeycapType),
            nameof(Configuration.KeyboardBrand), nameof(Configuration.KeyboardLocalization),
            nameof(Configuration.MousePreference), nameof(Configuration.MouseOrientation),
            nameof(Configuration.MousepadPreference),
            nameof(Configuration.HeadsetPreference),
            nameof(Configuration.ChromaLedsPreference),
        ];
        if (!relatedProperties.Contains(e.PropertyName ?? string.Empty)) return;

        Global.LightingStateManager.PreUpdate += LightingStateManager_LoadLayout;
    }

    private async void LightingStateManager_LoadLayout(object? sender, EventArgs e)
    {
        Global.LightingStateManager.PreUpdate -= LightingStateManager_LoadLayout;
        await LoadBrandDefault();
    }

    private void CalculateBitmap(VirtualGroup virtualKeyboardGroup, float keyboardWidth, float keyboardHeight)
    {
        double curWidth = 0;
        double curHeight = 0;
        double widthMax = 1;
        double heightMax = 1;
        var bitmapMap =
            new Dictionary<DeviceKeys, BitmapRectangle>(Effects.MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

        foreach (var key in virtualKeyboardGroup.GroupedKeys)
        {
            if ((int)key.Tag < 0)
                continue;

            var width = key.Width;
            var widthBit = PixelToByte(width);
            var height = key.Height;
            var heightBit = PixelToByte(height);
            var xOffset = key.MarginLeft;
            var yOffset = key.MarginTop;
            double brX, brY;

            if (key.AbsoluteLocation)
            {
                bitmapMap[key.Tag] = new BitmapRectangle(PixelToByte(xOffset), PixelToByte(yOffset), widthBit, heightBit);
                brX = xOffset + width;
                brY = yOffset + height;
            }
            else
            {
                var x = xOffset + curWidth;
                var y = yOffset + curHeight;

                bitmapMap[key.Tag] = new BitmapRectangle(PixelToByte(x), PixelToByte(y), widthBit, heightBit);

                brX = x + width;
                brY = y + height;

                if (key.LineBreak)
                {
                    curHeight += 37;
                    curWidth = 0;
                }
                else
                {
                    curWidth = brX;
                    curHeight = Math.Max(curHeight, y);
                }
            }
 
            widthMax = Math.Max(widthMax, brX);
            heightMax = Math.Max(heightMax, brY);
        }

        //+1 for rounding error, where the bitmap rectangle B(X)+B(Width) > B(X+Width)
        Effects.Canvas = new EffectCanvas(
            PixelToByte(virtualKeyboardGroup.Region.Width),
            PixelToByte(virtualKeyboardGroup.Region.Height),
            bitmapMap
        )
        {
            WidthCenter = keyboardWidth / 2,
            HeightCenter = keyboardHeight / 2,
        };
    }

    private async Task<Panel> CreateUserControl(VirtualGroup virtualKeyboardGroup, CancellationToken cancellationToken)
    {
        _virtualKeyboardMap.Clear();

        var virtualKb = await VirtualKeyboard;
        var kcg = new KeyboardControlGenerator(false, _virtualKeyboardMap, virtualKeyboardGroup, _layoutsPath, virtualKb, cancellationToken);

        var keyboardControl = await kcg.Generate();
        Effects.Canvas.CanvasGridProperties = new CanvasGridProperties((float)kcg.BaselineX, (float)kcg.BaselineY, (float)virtualKb.Width, (float)virtualKb.Height);
        return keyboardControl;
    }

    private async Task<Panel> CreateUserControlAbstract(VirtualGroup virtualKeyboardGroup, CancellationToken cancellationToken)
    {
        var virtualKb = new Grid();
        var kcg = new KeyboardControlGenerator(true, _virtualKeyboardMap, virtualKeyboardGroup, _layoutsPath, virtualKb, cancellationToken);

        var keyboardControl = await kcg.Generate();
        Effects.Canvas.CanvasGridProperties = new CanvasGridProperties((float)kcg.BaselineX, (float)kcg.BaselineY, (float)virtualKb.Width, (float)virtualKb.Height);
        return keyboardControl;
    }

    public void SetKeyboardColors(Dictionary<DeviceKeys, SimpleColor> keyLights, CancellationToken cancellationToken)
    {
        foreach (var (key, value) in _virtualKeyboardMap)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!keyLights.TryGetValue(key, out var keyColor)) continue;
            // cancel low priority calls when render stops
            var opaqueColor = ColorUtils.MultiplyColorByScalar(keyColor, keyColor.A / 255.0D);
            var drawingColor = Color.FromArgb(255, opaqueColor.R, opaqueColor.G, opaqueColor.B);
            value.SetColor(ColorUtils.DrawingColorToMediaColor(drawingColor));
        }
    }

    public void Dispose()
    {
        if (_chromaSdkManager != null) _chromaSdkManager.StateChanged -= ChromaSdkManagerOnStateChanged;
        LogitechSdkModule.LogitechSdkListener.StateChanged -= LogitechSdkListenerOnStateChanged;

        _rzSdk.Dispose();
        _cancellationTokenSource.Dispose();
        VirtualKeyboard.Dispose();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings.Controls.Keycaps;
using AuroraRgb.Settings.Layouts;
using AuroraRgb.Utils;
using Common;
using Common.Devices;
using RazerSdkReader;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Settings;

[JsonSerializable(typeof(VirtualGroup))]
[JsonSerializable(typeof(VirtualGroupConfiguration))]
[JsonSerializable(typeof(KeyboardLayout))]
internal partial class LayoutsSourceGenerationContext : JsonSerializerContext;

public class KeyboardLayoutManager
{
    private static readonly SemaphoreSlim GenerateLock = new(1, 1);

    private const string CulturesFolder = "kb_layouts";
    
    public Dictionary<DeviceKeys, DeviceKeys> LayoutKeyConversion { get; private set; } = new();

    private readonly VirtualGroup _virtualKeyboardGroup = new();

    private readonly Dictionary<DeviceKeys, Keycap> _virtualKeyboardMap = new();

    public Task<Grid> VirtualKeyboard { get; }

    public Task<Panel> AbstractVirtualKeyboard => CreateUserControl(true);

    public delegate void LayoutUpdatedEventHandler(object? sender);

    public event LayoutUpdatedEventHandler? KeyboardLayoutUpdated;

    public PreferredKeyboardLocalization LoadedLocalization { get; private set; } = PreferredKeyboardLocalization.None;

    private readonly string _layoutsPath;

    private readonly Task<ChromaReader?> _rzSdk;

    public KeyboardLayoutManager(Task<ChromaReader?> rzSdk)
    {
        _rzSdk = rzSdk;
        _layoutsPath = Path.Combine(Global.ExecutingDirectory, CulturesFolder);
        var vkTcs = new TaskCompletionSource<Grid>(TaskCreationOptions.RunContinuationsAsynchronously);
        VirtualKeyboard = vkTcs.Task;
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var grid = new Grid{ Width = 8, Height = 8, MaxWidth = double.PositiveInfinity, MaxHeight = double.PositiveInfinity };
            vkTcs.SetResult(grid);
        }, DispatcherPriority.Loaded);
    }

    public async Task Initialize()
    {
        await LoadBrandDefault();

        Global.Configuration.PropertyChanged += Configuration_PropertyChanged;
    }

    private async Task LoadBrandDefault()
    {
        await LoadBrand(
            Global.Configuration.KeyboardBrand,
            Global.Configuration.MousePreference,
            Global.Configuration.MousepadPreference,
            Global.Configuration.MouseOrientation,
            Global.Configuration.HeadsetPreference,
            Global.Configuration.ChromaLedsPreference
        );
    }

    private async Task LoadBrand(PreferredKeyboard keyboardPreference = PreferredKeyboard.None,
        PreferredMouse mousePreference = PreferredMouse.None,
        PreferredMousepad mousepadPreference = PreferredMousepad.None,
        MouseOrientationType mouseOrientation = MouseOrientationType.RightHanded,
        PreferredHeadset headsetPreference = PreferredHeadset.None,
        PreferredChromaLeds chromaLeds = PreferredChromaLeds.Automatic
    )
    {
#if !DEBUG
        try
        {
#endif
            //Load keyboard layout
            if (!Directory.Exists(_layoutsPath))
            {
                return;
            }

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

            switch (culture)
            {
                case "tr-TR":
                    await LoadCulture("tr");
                    break;
                case "ja-JP":
                    await LoadCulture("jpn");
                    break;
                case "de-DE":
                case "hsb-DE":
                case "dsb-DE":
                    LoadedLocalization = PreferredKeyboardLocalization.de;
                    await LoadCulture("de");
                    break;
                case "fr-CH":
                case "de-CH":
                    LoadedLocalization = PreferredKeyboardLocalization.swiss;
                    await LoadCulture("swiss");
                    break;
                case "fr-FR":
                case "br-FR":
                case "oc-FR":
                case "co-FR":
                case "gsw-FR":
                    LoadedLocalization = PreferredKeyboardLocalization.fr;
                    await LoadCulture("fr");
                    break;
                case "cy-GB":
                case "gd-GB":
                case "en-GB":
                    LoadedLocalization = PreferredKeyboardLocalization.uk;
                    await LoadCulture("uk");
                    break;
                case "ru-RU":
                case "tt-RU":
                case "ba-RU":
                case "sah-RU":
                    LoadedLocalization = PreferredKeyboardLocalization.ru;
                    await LoadCulture("ru");
                    break;
                case "en-US":
                    LoadedLocalization = PreferredKeyboardLocalization.us;
                    await LoadCulture("us");
                    break;
                case "da-DK":
                case "se-SE":
                case "nb-NO":
                case "nn-NO":
                case "nordic":
                    LoadedLocalization = PreferredKeyboardLocalization.nordic;
                    await LoadCulture("nordic");
                    break;
                case "pt-BR":
                    LoadedLocalization = PreferredKeyboardLocalization.abnt2;
                    await LoadCulture("abnt2");
                    break;
                case "dvorak":
                    LoadedLocalization = PreferredKeyboardLocalization.dvorak;
                    await LoadCulture("dvorak");
                    break;
                case "dvorak_int":
                    LoadedLocalization = PreferredKeyboardLocalization.dvorak_int;
                    await LoadCulture("dvorak_int");
                    break;
                case "hu-HU":
                    LoadedLocalization = PreferredKeyboardLocalization.hu;
                    await LoadCulture("hu");
                    break;
                case "it-IT":
                    LoadedLocalization = PreferredKeyboardLocalization.it;
                    await LoadCulture("it");
                    break;
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
                    LoadedLocalization = PreferredKeyboardLocalization.la;
                    await LoadCulture("la");
                    break;
                case "es-ES":
                    LoadedLocalization = PreferredKeyboardLocalization.es;
                    await LoadCulture("es");
                    break;
                case "iso":
                    LoadedLocalization = PreferredKeyboardLocalization.iso;
                    await LoadCulture("iso");
                    break;
                case "ansi":
                    LoadedLocalization = PreferredKeyboardLocalization.ansi;
                    await LoadCulture("ansi");
                    break;
                default:
                    LoadedLocalization = PreferredKeyboardLocalization.intl;
                    await LoadCulture("intl");
                    break;
            }

            if (PeripheralLayoutMap.KeyboardLayoutMap.TryGetValue(keyboardPreference, out var keyboardLayoutFile))
            {
                var layoutConfigPath = Path.Combine(_layoutsPath, keyboardLayoutFile);
                await LoadKeyboard(layoutConfigPath);
            }

            if (PeripheralLayoutMap.MouseLayoutMap.TryGetValue(mousePreference, out var mouseLayoutJsonFile))
            {
                var mouseFeaturePath = Path.Combine(_layoutsPath, "Extra Features", mouseLayoutJsonFile);

                await LoadMouse(mouseOrientation, mouseFeaturePath);
            }

            if (PeripheralLayoutMap.MousepadLayoutMap.TryGetValue(mousepadPreference, out var mousepadLayoutJsonFile))
            {
                var mousepadFeaturePath = Path.Combine(_layoutsPath, "Extra Features", mousepadLayoutJsonFile);

                await LoadGenericLayout(mousepadFeaturePath);
            }

            if (PeripheralLayoutMap.HeadsetLayoutMap.TryGetValue(headsetPreference, out var headsetLayoutJsonFile))
            {
                var headsetFeaturePath = Path.Combine(_layoutsPath, "Extra Features", headsetLayoutJsonFile);

                await LoadGenericLayout(headsetFeaturePath);
            }

            if (chromaLeds == PreferredChromaLeds.Automatic && await _rzSdk is not null)
            {
                chromaLeds = PreferredChromaLeds.Suggested;
            }
            if (PeripheralLayoutMap.ChromaLayoutMap.TryGetValue(chromaLeds, out var chromaLayoutJsonFile))
            {
                var headsetFeaturePath = Path.Combine(_layoutsPath, "Extra Features", chromaLayoutJsonFile);

                await LoadGenericLayout(headsetFeaturePath);
            }
#if !DEBUG
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error loading layouts");
        }
#endif

        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await CreateUserControl();
            KeyboardLayoutUpdated?.Invoke(this);
        });
    }

    private async Task<VirtualGroup?> LoadLayout(string path)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show( path + " could not be found", "Layout not found", MessageBoxButton.OK);
            return null;
        }

        var featureContent = await File.ReadAllTextAsync(path, Encoding.UTF8);
        return JsonSerializer.Deserialize(featureContent, LayoutsSourceGenerationContext.Default.VirtualGroup);
    }

    private async Task LoadKeyboard(string layoutConfigPath)
    {
        if (!File.Exists(layoutConfigPath))
        {
            MessageBox.Show( layoutConfigPath + " could not be found", "Layout not found", MessageBoxButton.OK);
            return;
        }

        var content = await File.ReadAllTextAsync(layoutConfigPath, Encoding.UTF8);
        var layoutConfig = JsonSerializer.Deserialize(content, LayoutsSourceGenerationContext.Default.VirtualGroupConfiguration)!;

        _virtualKeyboardGroup.AdjustKeys(layoutConfig.KeyModifications);
        _virtualKeyboardGroup.RemoveKeys(layoutConfig.KeysToRemove);

        foreach (var key in layoutConfig.KeyConversion.Where(key => !LayoutKeyConversion.ContainsKey(key.Key)))
        {
            LayoutKeyConversion.Add(key.Key, key.Value);
        }

        foreach (var feature in layoutConfig.IncludedFeatures)
        {
            var featurePath = Path.Combine(_layoutsPath, "Extra Features", feature);

            if (!File.Exists(featurePath)) continue;
            var featureContent = await File.ReadAllTextAsync(featurePath, Encoding.UTF8);
            var featureConfig = JsonSerializer.Deserialize(featureContent, LayoutsSourceGenerationContext.Default.VirtualGroup)!;

            _virtualKeyboardGroup.AddFeature(featureConfig.GroupedKeys.ToArray(), featureConfig.OriginRegion);

            foreach (var key in featureConfig.KeyConversion.Where(key => !LayoutKeyConversion.ContainsKey(key.Key)))
            {
                LayoutKeyConversion.Add(key.Key, key.Value);
            }
        }
    }

    private async Task LoadMouse(MouseOrientationType mouseOrientation, string mouseFeaturePath)
    {
        var featureConfig = await LoadLayout(mouseFeaturePath);
        if (featureConfig == null)
        {
            return;
        }

        if (mouseOrientation == MouseOrientationType.LeftHanded)
        {
            if (featureConfig.OriginRegion == KeyboardRegion.TopRight)
                featureConfig.OriginRegion = KeyboardRegion.TopLeft;
            else if (featureConfig.OriginRegion == KeyboardRegion.BottomRight)
                featureConfig.OriginRegion = KeyboardRegion.BottomLeft;

            var outlineWidth = 0.0;

            foreach (var key in featureConfig.GroupedKeys)
            {
                if (outlineWidth == 0.0 && key.Tag == DeviceKeys.NONE)
                {
                    //We found outline (NOTE: Outline has to be first in the grouped keys)
                    outlineWidth = key.Width + 2 * key.MarginLeft;
                }

                key.MarginLeft -= outlineWidth;
            }
        }

        _virtualKeyboardGroup.AddFeature(featureConfig.GroupedKeys.ToArray(), featureConfig.OriginRegion);
    }

    private async Task LoadGenericLayout(string headsetFeaturePath)
    {
        var featureConfig = await LoadLayout(headsetFeaturePath);
        if (featureConfig == null)
        {
            return;
        }

        _virtualKeyboardGroup.AddFeature(featureConfig.GroupedKeys.ToArray(), featureConfig.OriginRegion);
    }

    private static int PixelToByte(double pixel)
    {
        return (int) Math.Round(pixel / (double) Global.Configuration.BitmapAccuracy);
    }

    private void Configuration_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IEnumerable<string> relatedProperties = [
            nameof(Configuration.BitmapAccuracy),
            nameof(Configuration.VirtualkeyboardKeycapType),
            nameof(Configuration.KeyboardBrand), nameof(Configuration.KeyboardLocalization),
            nameof(Configuration.MousePreference), nameof(Configuration.MouseOrientation),
            nameof(Configuration.MousepadPreference),
            nameof(Configuration.HeadsetPreference),
            nameof(Configuration.ChromaLedsPreference),
        ];
        if (!relatedProperties.Contains(e.PropertyName)) return;

        Global.LightingStateManager.PreUpdate += LightingStateManager_LoadLayout;
    }

    private async void LightingStateManager_LoadLayout(object? sender, EventArgs e)
    {
        Global.LightingStateManager.PreUpdate -= LightingStateManager_LoadLayout;
        await LoadBrandDefault();
    }

    private void CalculateBitmap(KeyboardControlGenerator kcg)
    {
        double curWidth = 0;
        double curHeight = 0;
        double widthMax = 1;
        double heightMax = 1;
        var bitmapMap = new Dictionary<DeviceKeys, BitmapRectangle>(Effects.MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

        foreach (var key in _virtualKeyboardGroup.GroupedKeys)
        {
            if (key.Tag.Equals(DeviceKeys.NONE))
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
            PixelToByte(_virtualKeyboardGroup.Region.Width),
            PixelToByte(_virtualKeyboardGroup.Region.Height),
            bitmapMap,
            (float)kcg.BaselineX, (float)kcg.BaselineY,
            (float)kcg.GridWidth, (float)kcg.GridHeight
        );
    }

    private async Task<Panel> CreateUserControl(bool abstractKeycaps = false)
    {
        try
        {
            await GenerateLock.WaitAsync();
            return await CreateUserControlLocked(abstractKeycaps);
        }
        finally
        {
            GenerateLock.Release();
        }
    }

    private async Task<Panel> CreateUserControlLocked(bool abstractKeycaps)
    {
        if (!abstractKeycaps)
            _virtualKeyboardMap.Clear();

        var virtualKb = abstractKeycaps ? new Grid() : await VirtualKeyboard;
        var kcg = new KeyboardControlGenerator(abstractKeycaps, _virtualKeyboardMap, _virtualKeyboardGroup, _layoutsPath, virtualKb);

        var keyboardControl = await kcg.Generate();
        CalculateBitmap(kcg);
        return keyboardControl;
    }

    private async Task LoadCulture(string culture)
    {
        var fileName = "Plain Keyboard\\layout." + culture + ".json";
        var layoutPath = Path.Combine(_layoutsPath, fileName);

        if (!File.Exists(layoutPath))
        {
            return;
        }

        var content = await File.ReadAllTextAsync(layoutPath, Encoding.UTF8);
        var keyboard = JsonSerializer.Deserialize(content, LayoutsSourceGenerationContext.Default.KeyboardLayout)!;

        _virtualKeyboardGroup.Clear(keyboard.Keys);

        LayoutKeyConversion = keyboard.KeyConversion;
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
}
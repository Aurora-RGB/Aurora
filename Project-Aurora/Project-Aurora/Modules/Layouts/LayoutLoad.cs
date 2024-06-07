using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layouts;
using Common.Devices;

namespace AuroraRgb.Modules.Layouts;

public class LayoutLoad(string layoutsPath, bool loadLinkLeds)
{
    public float KeyboardWidth { get; private set; }
    public float KeyboardHeight { get; private set; }
    public Dictionary<DeviceKeys, DeviceKeys> LayoutKeyConversion { get; private set; } = new();
    public VirtualGroup VirtualKeyboardGroup { get; } = new();

    internal async Task LoadBrand(
        CancellationToken cancellationToken,
        PreferredKeyboard keyboardPreference = PreferredKeyboard.None,
        PreferredMouse mousePreference = PreferredMouse.None,
        PreferredMousepad mousepadPreference = PreferredMousepad.None,
        MouseOrientationType mouseOrientation = MouseOrientationType.RightHanded,
        PreferredHeadset headsetPreference = PreferredHeadset.None,
        PreferredChromaLeds chromaLeds = PreferredChromaLeds.Automatic)
    {
#if !DEBUG
        try
        {
#endif
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

            await LoadCulture(GetCulture(culture), cancellationToken);

            if (PeripheralLayoutMap.KeyboardLayoutMap.TryGetValue(keyboardPreference, out var keyboardLayoutFile))
            {
                var layoutConfigPath = Path.Combine(layoutsPath, keyboardLayoutFile);
                await LoadKeyboard(layoutConfigPath, cancellationToken);
                KeyboardWidth = VirtualKeyboardGroup.Region.Width / (int)Global.Configuration.BitmapAccuracy;
                KeyboardHeight = VirtualKeyboardGroup.Region.Height / (int)Global.Configuration.BitmapAccuracy;
            }

            if (PeripheralLayoutMap.MouseLayoutMap.TryGetValue(mousePreference, out var mouseLayoutJsonFile))
            {
                var mouseFeaturePath = Path.Combine(layoutsPath, "Extra Features", mouseLayoutJsonFile);

                await LoadMouse(mouseOrientation, mouseFeaturePath, cancellationToken);
            }

            if (PeripheralLayoutMap.MousepadLayoutMap.TryGetValue(mousepadPreference, out var mousepadLayoutJsonFile))
            {
                var mousepadFeaturePath = Path.Combine(layoutsPath, "Extra Features", mousepadLayoutJsonFile);

                await LoadGenericLayout(mousepadFeaturePath, cancellationToken);
            }

            if (PeripheralLayoutMap.HeadsetLayoutMap.TryGetValue(headsetPreference, out var headsetLayoutJsonFile))
            {
                var headsetFeaturePath = Path.Combine(layoutsPath, "Extra Features", headsetLayoutJsonFile);

                await LoadGenericLayout(headsetFeaturePath, cancellationToken);
            }

            if (chromaLeds == PreferredChromaLeds.Automatic && loadLinkLeds)
            {
                chromaLeds = PreferredChromaLeds.Suggested;
            }

            if (PeripheralLayoutMap.ChromaLayoutMap.TryGetValue(chromaLeds, out var chromaLayoutJsonFile))
            {
                var headsetFeaturePath = Path.Combine(layoutsPath, "Extra Features", chromaLayoutJsonFile);

                await LoadGenericLayout(headsetFeaturePath, cancellationToken);
            }
#if !DEBUG
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error loading layouts");
        }
#endif
    }

    private async Task<VirtualGroup?> LoadLayout(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(path + " could not be found", "Layout not found", MessageBoxButton.OK);
            return null;
        }

        var featureContent = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
        return JsonSerializer.Deserialize(featureContent, LayoutsSourceGenerationContext.Default.VirtualGroup);
    }

    private async Task LoadKeyboard(string layoutConfigPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(layoutConfigPath))
        {
            MessageBox.Show(layoutConfigPath + " could not be found", "Layout not found", MessageBoxButton.OK);
            return;
        }

        var content = await File.ReadAllTextAsync(layoutConfigPath, Encoding.UTF8, cancellationToken);
        var layoutConfig = JsonSerializer.Deserialize(content, LayoutsSourceGenerationContext.Default.VirtualGroupConfiguration)!;

        VirtualKeyboardGroup.AdjustKeys(layoutConfig.KeyModifications);
        VirtualKeyboardGroup.RemoveKeys(layoutConfig.KeysToRemove);

        foreach (var key in layoutConfig.KeyConversion.Where(key => !LayoutKeyConversion.ContainsKey(key.Key)))
        {
            LayoutKeyConversion.Add(key.Key, key.Value);
        }

        foreach (var feature in layoutConfig.IncludedFeatures)
        {
            var featurePath = Path.Combine(layoutsPath, "Extra Features", feature);

            if (!File.Exists(featurePath)) continue;
            var featureContent = await File.ReadAllTextAsync(featurePath, Encoding.UTF8, cancellationToken);
            var featureConfig = JsonSerializer.Deserialize(featureContent, LayoutsSourceGenerationContext.Default.VirtualGroup)!;

            VirtualKeyboardGroup.AddFeature(featureConfig.GroupedKeys.ToArray(), featureConfig.OriginRegion);

            foreach (var key in featureConfig.KeyConversion.Where(key => !LayoutKeyConversion.ContainsKey(key.Key)))
            {
                LayoutKeyConversion.Add(key.Key, key.Value);
            }
        }
    }

    private async Task LoadMouse(MouseOrientationType mouseOrientation, string mouseFeaturePath, CancellationToken cancellationToken)
    {
        var featureConfig = await LoadLayout(mouseFeaturePath, cancellationToken);
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

        VirtualKeyboardGroup.AddFeature(featureConfig.GroupedKeys.ToArray(), featureConfig.OriginRegion);
    }

    private async Task LoadGenericLayout(string headsetFeaturePath, CancellationToken cancellationToken)
    {
        var featureConfig = await LoadLayout(headsetFeaturePath, cancellationToken);
        if (featureConfig == null)
        {
            return;
        }

        VirtualKeyboardGroup.AddFeature(featureConfig.GroupedKeys.ToArray(), featureConfig.OriginRegion);
    }

    private static string GetCulture(string culture)
    {
        switch (culture)
        {
            case "tr-TR":
                return "tr";
            case "ja-JP":
                return "jpn";
            case "de-DE":
            case "hsb-DE":
            case "dsb-DE":
                return "de";
            case "fr-CH":
            case "de-CH":
                return "swiss";
            case "fr-FR":
            case "br-FR":
            case "oc-FR":
            case "co-FR":
            case "gsw-FR":
                return "fr";
            case "cy-GB":
            case "gd-GB":
            case "en-GB":
                return "uk";
            case "ru-RU":
            case "tt-RU":
            case "ba-RU":
            case "sah-RU":
                return "ru";
            case "en-US":
                return "us";
            case "da-DK":
            case "se-SE":
            case "nb-NO":
            case "nn-NO":
            case "nordic":
                return "nordic";
            case "pt-BR":
                return "abnt2";
            case "dvorak":
                return "dvorak";
            case "dvorak_int":
                return "dvorak_int";
            case "hu-HU":
                return "hu";
            case "it-IT":
                return "it";
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
                return "la";
            case "es-ES":
                return "es";
            case "iso":
                return "iso";
            case "ansi":
                return "ansi";
            default:
                return "intl";
        }
    }

    private async Task LoadCulture(string culture, CancellationToken cancellationToken)
    {
        var fileName = "Plain Keyboard\\layout." + culture + ".json";
        var layoutPath = Path.Combine(layoutsPath, fileName);

        if (!File.Exists(layoutPath))
        {
            return;
        }

        var content = await File.ReadAllTextAsync(layoutPath, Encoding.UTF8, cancellationToken);
        var keyboard = JsonSerializer.Deserialize(content, LayoutsSourceGenerationContext.Default.KeyboardLayout)!;

        VirtualKeyboardGroup.Clear(keyboard.Keys);

        LayoutKeyConversion = keyboard.KeyConversion;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.Settings.Controls.Keycaps;
using AuroraRgb.Settings.Layouts;
using AuroraRgb.Utils;
using Common.Devices;

namespace AuroraRgb.Settings;

internal sealed class KeyboardControlGenerator(
    bool abstractKeycaps,
    IDictionary<DeviceKeys, Keycap> virtualKeyboardMap,
    VirtualGroup virtualKeyboardGroup,
    string layoutsPath,
    Panel virtualKeyboard,
    CancellationToken cancellationToken
)
{
    private double _layoutHeight;
    private double _layoutWidth;

    private double _currentHeight;
    private double _currentWidth;

    public double BaselineX { get; private set; }
    public double BaselineY { get; private set; }

    public double GridWidth => virtualKeyboard.Width;
    public double GridHeight => virtualKeyboard.Height;


    internal async Task<Panel> Generate()
    {
        var imagesPath = Path.Combine(layoutsPath, "Extra Features", "images");

        virtualKeyboard.Children.Clear();
        var keyCreations = virtualKeyboardGroup.GroupedKeys
            .OrderBy(a => a.ZIndex)
            .Select(async key =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        CancellationToken().ThrowIfCancellationRequested();
                        CreateKey(key, imagesPath);
                    },
                    DispatcherPriority.Loaded,
                    CancellationToken()
                );
            });
        await Task.WhenAll(keyCreations);

        if (virtualKeyboardGroup.GroupedKeys.Count == 0)
        {
            //No items, display error
            DisplayNoKeyboardError();
        }
        else
        {
            //Update size
            virtualKeyboard.Width = virtualKeyboardGroup.Region.Width;
            virtualKeyboard.Height = virtualKeyboardGroup.Region.Height;
        }

        return virtualKeyboard;
    }

    private CancellationToken CancellationToken()
    {
        return cancellationToken;
    }

    private void DisplayNoKeyboardError()
    {
        var errorMessage = new Label();
        var infoPanel = new DockPanel();
        var infoMessage = new TextBlock
        {
            Text = "No keyboard selected\r\nPlease select your keyboard in the settings",
            TextAlignment = TextAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
        };

        DockPanel.SetDock(infoMessage, Dock.Top);
        infoPanel.Children.Add(infoMessage);

        var infoInstruction = new DockPanel();

        infoInstruction.Children.Add(new TextBlock
        {
            Text = "Press (",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            VerticalAlignment = VerticalAlignment.Center
        });

        infoInstruction.Children.Add(new Image
        {
            Source = new BitmapImage(new Uri(@"Resources/settings_icon.png", UriKind.Relative)),
            Stretch = Stretch.Uniform,
            Height = 40.0,
            VerticalAlignment = VerticalAlignment.Center
        });

        infoInstruction.Children.Add(new TextBlock
        {
            Text = ") and go into \"Devices & Wrappers\" tab",
            Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)),
            VerticalAlignment = VerticalAlignment.Center
        });

        DockPanel.SetDock(infoInstruction, Dock.Bottom);
        infoPanel.Children.Add(infoInstruction);

        errorMessage.Content = infoPanel;

        errorMessage.FontSize = 16.0;
        errorMessage.FontWeight = FontWeights.Bold;
        errorMessage.HorizontalContentAlignment = HorizontalAlignment.Center;
        errorMessage.VerticalContentAlignment = VerticalAlignment.Center;

        virtualKeyboard.Children.Add(errorMessage);

        //Update size
        virtualKeyboard.Width = 850;
        virtualKeyboard.Height = 200;
    }

    private void CreateKey(KeyboardKey key, string imagesPath)
    {
        var keyMarginLeft = key.MarginLeft;
        var keyMarginTop = key.MarginTop;
        var imagePath = !string.IsNullOrWhiteSpace(key.Image) ? Path.Combine(imagesPath, key.Image) : "";

        Keycap keycap;

        //Ghost keycap is used for abstract representation of keys
        if (abstractKeycaps)
            keycap = new Control_GhostKeycap(key, imagePath);
        else
        {
            keycap = Global.Configuration.VirtualkeyboardKeycapType switch
            {
                KeycapType.Default_backglow => new Control_DefaultKeycapBackglow(key, imagePath),
                KeycapType.Default_backglow_only => new Control_DefaultKeycapBackglowOnly(key, imagePath),
                KeycapType.Colorized => new Control_ColorizedKeycap(key, imagePath),
                KeycapType.Colorized_blank => new Control_ColorizedKeycapBlank(key, imagePath),
                _ => new Control_DefaultKeycap(key, imagePath)
            };
        }

        var keyLights = Global.effengine.GetKeyboardLights();
        if (keyLights.TryGetValue(key.Tag, out var keyColor))
        {
            var opaqueColor = ColorUtils.MultiplyColorByScalar(keyColor, keyColor.A / 255.0D);
            var drawingColor = Color.FromArgb(255, opaqueColor.R, opaqueColor.G, opaqueColor.B);
            keycap.SetColor(drawingColor);
        }

        virtualKeyboard.Children.Add(keycap);

        if (key.Tag != DeviceKeys.NONE && !virtualKeyboardMap.ContainsKey(key.Tag) && keycap is { } kc && !abstractKeycaps)
            virtualKeyboardMap.Add(key.Tag, kc);

        if (key.AbsoluteLocation)
            keycap.Margin = new Thickness(key.MarginLeft, key.MarginTop, 0, 0);
        else
            keycap.Margin = new Thickness(_currentWidth + key.MarginLeft, _currentHeight + key.MarginTop, 0, 0);

        if (key.Tag == DeviceKeys.ESC)
        {
            BaselineX = keycap.Margin.Left;
            BaselineY = keycap.Margin.Top;
        }

        if (key.AbsoluteLocation) return;
        if (key.Width + keyMarginLeft > 0)
            _currentWidth += key.Width + keyMarginLeft;

        if (keyMarginTop > 0)
            _currentHeight += keyMarginTop;

        _layoutWidth = Math.Max(_layoutWidth, _currentWidth);

        if (key.LineBreak)
        {
            _currentHeight += 37;
            _currentWidth = 0;
        }

        _layoutHeight = Math.Max(_layoutHeight, _currentHeight);
    }
}
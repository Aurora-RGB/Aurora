using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AuroraRgb.Devices;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;
using Common.Utils;
using MediaColor = System.Windows.Media.Color;

namespace AuroraRgb.Controls;

public partial class Control_DeviceCalibrationItem
{
    private const int PreviewWidth = 256;
    private const int PreviewHeight = 24;

    private readonly SingleConcurrentThread _worker;

    private readonly DeviceManager _deviceManager;
    private readonly DeviceConfig _deviceConfig;

    private DeviceCalibration _calibration;
    private readonly string _deviceKey;

    private bool _loaded;

    public Control_DeviceCalibrationItem(DeviceManager deviceManager, DeviceConfig deviceConfig, RemappableDevice device, DeviceCalibration calibration)
    {
        _deviceManager = deviceManager;
        _deviceConfig = deviceConfig;
        _deviceKey = device.DeviceId;
        _calibration = calibration;

        _worker = new SingleConcurrentThread("Device Calibration", WorkerOnDoWork, ExceptionCallback);

        InitializeComponent();

        DeviceText.Text = device.DeviceSummary;
        LoadCalibration(_calibration);
        _loaded = true;

        RefreshIndicators();
        PopulateDetails();
    }

    private void LoadCalibration(DeviceCalibration calibration)
    {
        BrightnessSlider.Value = calibration.Brightness;
        GammaSlider.Value = calibration.Gamma;
        RedSlider.Value = calibration.RedGain;
        GreenSlider.Value = calibration.GreenGain;
        BlueSlider.Value = calibration.BlueGain;
    }

    private void Slider_OnValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_loaded)
        {
            return;
        }

        // preserve any wizard-built curves/matrix/samples; only the manual fine-tune changes here
        _calibration = _calibration with
        {
            Brightness = BrightnessSlider.Value,
            RedGain = RedSlider.Value,
            GreenGain = GreenSlider.Value,
            BlueGain = BlueSlider.Value,
            Gamma = GammaSlider.Value
        };

        _deviceConfig.DeviceColorCalibrations[_deviceKey] = _calibration;
        RefreshIndicators();
        _worker.Trigger();
    }

    private async Task WorkerOnDoWork()
    {
        await _deviceManager.DevicesPipe.Recalibrate(_deviceKey, _calibration);
    }

    private static void ExceptionCallback(object? sender, SingleThreadExceptionEventArgs eventArgs)
    {
        Global.logger.Error(eventArgs.Exception, "Control_DeviceCalibrationItem._worker");
    }

    private void ResetDevice_OnClick(object sender, RoutedEventArgs e)
    {
        // full reset: clear curves, matrix and 3D-LUT, not just the manual sliders
        _calibration = DeviceCalibration.Identity;
        _loaded = false;
        LoadCalibration(_calibration);
        _loaded = true;

        _deviceConfig.DeviceColorCalibrations[_deviceKey] = _calibration;
        RefreshIndicators();
        PopulateDetails();
        _worker.Trigger();
    }

    #region Indicators

    private void RefreshIndicators()
    {
        StatusText.Text = Describe(_calibration);
        RenderPreview();
    }

    private static string Describe(DeviceCalibration c)
    {
        var parts = new List<string>();
        if (c.Samples is { Length: > 0 } s)
        {
            parts.Add($"3D-LUT · {s.Length} pts");
        }

        if (c.Matrix is { IsIdentity: false })
        {
            parts.Add("Matrix");
        }

        if (c.RedCurve is { IsIdentity: false } || c.GreenCurve is { IsIdentity: false } || c.BlueCurve is { IsIdentity: false })
        {
            parts.Add("Curves");
        }

        if (c.Brightness is not 1.0 || c.RedGain is not 1.0 || c.GreenGain is not 1.0 || c.BlueGain is not 1.0 || c.Gamma is not 1.0)
        {
            parts.Add("Manual");
        }

        return parts.Count > 0 ? string.Join("   ·   ", parts) : "No calibration";
    }

    private void RenderPreview()
    {
        var lookup = _calibration.GetLookup();
        var bitmap = new WriteableBitmap(PreviewWidth, PreviewHeight, 96, 96, PixelFormats.Bgra32, null);
        var stride = PreviewWidth * 4;
        var pixels = new byte[PreviewHeight * stride];

        for (var x = 0; x < PreviewWidth; x++)
        {
            var hue = x / (double)(PreviewWidth - 1) * 360.0;
            var raw = HueColor(hue);
            var calibrated = lookup.Apply(raw);
            for (var y = 0; y < PreviewHeight; y++)
            {
                var c = y < PreviewHeight / 2 ? raw : calibrated;
                var o = y * stride + x * 4;
                pixels[o] = c.B;
                pixels[o + 1] = c.G;
                pixels[o + 2] = c.R;
                pixels[o + 3] = 255;
            }
        }

        bitmap.WritePixels(new Int32Rect(0, 0, PreviewWidth, PreviewHeight), pixels, stride, 0);
        PreviewImage.Source = bitmap;
    }

    private void PopulateDetails()
    {
        var hasMatrix = _calibration.Matrix is { IsIdentity: false } m && m.Values.Length == 9;
        MatrixSection.Visibility = hasMatrix ? Visibility.Visible : Visibility.Collapsed;
        if (hasMatrix)
        {
            MatrixText.Text = FormatMatrix(_calibration.Matrix!.Values);
        }

        SamplesPanel.Children.Clear();
        var hasSamples = _calibration.Samples is { Length: > 0 };
        SamplesSection.Visibility = hasSamples ? Visibility.Visible : Visibility.Collapsed;
        if (hasSamples)
        {
            foreach (var sample in _calibration.Samples!)
            {
                SamplesPanel.Children.Add(BuildSampleSwatch(sample));
            }
        }

        DetailsExpander.Visibility = hasMatrix || hasSamples ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string FormatMatrix(double[] v)
    {
        return string.Format(CultureInfo.InvariantCulture,
            "{0,7:F3} {1,7:F3} {2,7:F3}\n{3,7:F3} {4,7:F3} {5,7:F3}\n{6,7:F3} {7,7:F3} {8,7:F3}",
            v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8]);
    }

    private static UIElement BuildSampleSwatch(ColorSample sample)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 8, 4) };
        panel.Children.Add(Swatch(sample.Source));
        panel.Children.Add(new TextBlock { Text = "→", Margin = new Thickness(2, 0, 2, 0), VerticalAlignment = VerticalAlignment.Center, FontSize = 10 });
        panel.Children.Add(Swatch(sample.Output));
        panel.ToolTip = $"{sample.Source.R},{sample.Source.G},{sample.Source.B}  →  {sample.Output.R},{sample.Output.G},{sample.Output.B}";
        return panel;
    }

    private static Rectangle Swatch(SimpleColor c) => new()
    {
        Width = 16,
        Height = 16,
        RadiusX = 2,
        RadiusY = 2,
        Stroke = new SolidColorBrush(MediaColor.FromArgb(0x66, 0x88, 0x88, 0x88)),
        StrokeThickness = 1,
        Fill = new SolidColorBrush(MediaColor.FromRgb(c.R, c.G, c.B))
    };

    private static SimpleColor HueColor(double hue)
    {
        var h = hue / 60.0;
        var x = (byte)(255 * (1 - Math.Abs(h % 2 - 1)));
        return ((int)h) switch
        {
            0 => new SimpleColor(255, x, 0),
            1 => new SimpleColor(x, 255, 0),
            2 => new SimpleColor(0, 255, x),
            3 => new SimpleColor(0, x, 255),
            4 => new SimpleColor(x, 0, 255),
            _ => new SimpleColor(255, 0, x)
        };
    }

    #endregion
}

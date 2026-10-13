using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Devices;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;
using Common.Utils;

namespace AuroraRgb.Controls;

public partial class Control_CalibrationWizard
{
    // Stage 1 (brightness) samples each channel at these mid levels; 0 and full are anchored.
    private static readonly byte[] CurveLevels = [85, 170];

    // Stage 2 (tonality) palette: white + primaries + secondaries, full intensity.
    private static readonly SimpleColor[] Palette =
    [
        new(255, 255, 255), new(255, 0, 0), new(0, 255, 0), new(0, 0, 255),
        new(255, 255, 0), new(0, 255, 255), new(255, 0, 255)
    ];

    private static readonly string[] PaletteNames = ["WHITE", "RED", "GREEN", "BLUE", "YELLOW", "CYAN", "MAGENTA"];

    // Stage 3 (non-linear fixes): intermediate mixes a single matrix usually can't match.
    private static readonly SimpleColor[] SuggestedSamples =
    [
        new(128, 0, 255), new(255, 0, 128),   // purples
        new(0, 128, 255), new(0, 255, 128),   // teals
        new(128, 255, 0), new(255, 128, 0),   // lime / orange
        new(128, 128, 255), new(128, 255, 128), new(255, 128, 128), // pastels
        new(128, 128, 128)                     // gray
    ];

    private static readonly string[] ChannelNames = ["RED", "GREEN", "BLUE"];

    private readonly DeviceManager _deviceManager;
    private readonly DeviceConfig _deviceConfig;
    private readonly IReadOnlyList<RemappableDevice> _devices;

    private readonly SingleConcurrentThread _worker;

    private readonly byte[][] _curveOut =
    [
        (byte[])CurveLevels.Clone(), (byte[])CurveLevels.Clone(), (byte[])CurveLevels.Clone()
    ];

    private readonly SimpleColor[] _paletteOut = (SimpleColor[])Palette.Clone();

    private readonly List<SimpleColor> _sampleSrc = [];
    private readonly List<SimpleColor> _sampleOut = [];
    private bool _phase3Seeded;
    private DeviceCalibration _baseCalibration = DeviceCalibration.Identity;

    private DeviceCalibration _existing = DeviceCalibration.Identity;

    private readonly List<(int phase, int a, int b)> _steps = [];
    private int _stepIndex = -1;
    private bool _applyingStep;

    private string _referenceId = "";
    private string _targetId = "";
    private SimpleColor _referenceColor;
    private SimpleColor _targetColor;

    public Control_CalibrationWizard(DeviceManager deviceManager, DeviceConfig deviceConfig, IReadOnlyList<RemappableDevice> devices)
    {
        _deviceManager = deviceManager;
        _deviceConfig = deviceConfig;
        _devices = devices;

        _worker = new SingleConcurrentThread("Calibration Wizard", WorkerOnDoWork, ExceptionCallback);

        InitializeComponent();

        foreach (var device in _devices)
        {
            ReferenceCombo.Items.Add(device.DeviceSummary);
            TargetCombo.Items.Add(device.DeviceSummary);
        }

        if (_devices.Count > 0) ReferenceCombo.SelectedIndex = 0;
        if (_devices.Count > 1) TargetCombo.SelectedIndex = 1;
    }

    private static SimpleColor ChannelColor(int channel, byte value) => channel switch
    {
        0 => new SimpleColor(value, 0, 0),
        1 => new SimpleColor(0, value, 0),
        _ => new SimpleColor(0, 0, value)
    };

    private void NextButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_stepIndex < 0)
        {
            StartWizard();
            return;
        }

        if (_stepIndex < _steps.Count - 1)
        {
            _stepIndex++;
            ApplyStep();
        }
        else
        {
            Finish();
        }
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_stepIndex <= 0)
        {
            return;
        }

        _stepIndex--;
        ApplyStep();
    }

    private void StartWizard()
    {
        var refIndex = ReferenceCombo.SelectedIndex;
        var targetIndex = TargetCombo.SelectedIndex;
        if (refIndex < 0 || targetIndex < 0 || refIndex == targetIndex)
        {
            SetupError.Text = "Pick two different devices.";
            SetupError.Visibility = Visibility.Visible;
            return;
        }

        _referenceId = _devices[refIndex].DeviceId;
        _targetId = _devices[targetIndex].DeviceId;
        _existing = _deviceConfig.DeviceColorCalibrations.GetValueOrDefault(_targetId, DeviceCalibration.Identity);

        _sampleSrc.AddRange(SuggestedSamples);
        _sampleOut.AddRange(SuggestedSamples);

        _steps.Clear();
        for (var channel = 0; channel < 3; channel++)
        {
            for (var levelIndex = 0; levelIndex < CurveLevels.Length; levelIndex++)
            {
                _steps.Add((1, channel, levelIndex));
            }
        }

        for (var colorIndex = 0; colorIndex < Palette.Length; colorIndex++)
        {
            _steps.Add((2, colorIndex, 0));
        }

        for (var sampleIndex = 0; sampleIndex < _sampleSrc.Count; sampleIndex++)
        {
            _steps.Add((3, sampleIndex, 0));
        }

        SetupPanel.Visibility = Visibility.Collapsed;
        BackButton.Visibility = Visibility.Visible;

        _stepIndex = 0;
        ApplyStep();
    }

    private void ApplyStep()
    {
        var (phase, a, b) = _steps[_stepIndex];
        _applyingStep = true;

        if (phase == 1)
        {
            var channel = a;
            var input = CurveLevels[b];
            var output = _curveOut[channel][b];
            _referenceColor = ChannelColor(channel, input);
            _targetColor = ChannelColor(channel, output);
            OutputSlider.Value = output;

            Phase1Step.Text = $"Step {_stepIndex + 1}/{_steps.Count} — {ChannelNames[channel]} brightness at {input * 100 / 255}%";
            ShowPanel(1);
        }
        else
        {
            if (phase == 3 && !_phase3Seeded)
            {
                SeedPhase3();
            }

            SimpleColor reference, output;
            string label;
            if (phase == 2)
            {
                reference = Palette[a];
                output = _paletteOut[a];
                label = $"match {PaletteNames[a]}";
            }
            else
            {
                reference = _sampleSrc[a];
                output = _sampleOut[a];
                label = $"fine-tune {reference.R},{reference.G},{reference.B}";
            }

            _referenceColor = reference;
            _targetColor = output;
            RedSlider.Value = output.R;
            GreenSlider.Value = output.G;
            BlueSlider.Value = output.B;

            Phase2Step.Text = $"Step {_stepIndex + 1}/{_steps.Count} — {label}";
            AddColorPanel.Visibility = phase == 3 ? Visibility.Visible : Visibility.Collapsed;
            ShowPanel(2);
        }

        NextButton.Content = _stepIndex == _steps.Count - 1 ? "Finish" : "Next";
        BackButton.IsEnabled = _stepIndex > 0;
        _applyingStep = false;

        _worker.Trigger();
    }

    private void ShowPanel(int phase)
    {
        Phase1Panel.Visibility = phase == 1 ? Visibility.Visible : Visibility.Collapsed;
        Phase2Panel.Visibility = phase == 2 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SeedPhase3()
    {
        _baseCalibration = _existing with
        {
            RedCurve = BuildCurve(0),
            GreenCurve = BuildCurve(1),
            BlueCurve = BuildCurve(2),
            Matrix = CalibrationMath.FitColorMatrix(Palette, _paletteOut)
        };

        var lookup = _baseCalibration.GetLookup();
        for (var i = 0; i < _sampleSrc.Count; i++)
        {
            _sampleOut[i] = lookup.Apply(_sampleSrc[i]);
        }

        _phase3Seeded = true;
    }

    private void OutputSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_applyingStep || _stepIndex < 0)
        {
            return;
        }

        var (phase, channel, levelIndex) = _steps[_stepIndex];
        if (phase != 1)
        {
            return;
        }

        var output = (byte)e.NewValue;
        _curveOut[channel][levelIndex] = output;
        _targetColor = ChannelColor(channel, output);
        _worker.Trigger();
    }

    private void RgbSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_applyingStep || _stepIndex < 0)
        {
            return;
        }

        var (phase, index, _) = _steps[_stepIndex];
        _targetColor = new SimpleColor((byte)RedSlider.Value, (byte)GreenSlider.Value, (byte)BlueSlider.Value);

        if (phase == 2)
        {
            _paletteOut[index] = _targetColor;
        }
        else if (phase == 3)
        {
            _sampleOut[index] = _targetColor;
        }
        else
        {
            return;
        }

        _worker.Trigger();
    }

    private void AddColor_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TryParseColor(CustomColorBox.Text, out var color))
        {
            CustomColorBox.Text = "e.g. 127,0,255";
            return;
        }

        _sampleSrc.Add(color);
        _sampleOut.Add(_phase3Seeded ? _baseCalibration.GetLookup().Apply(color) : color);
        _steps.Insert(_stepIndex + 1, (3, _sampleSrc.Count - 1, 0));

        CustomColorBox.Clear();
        _stepIndex++;
        ApplyStep();
    }

    private static bool TryParseColor(string text, out SimpleColor color)
    {
        color = default;
        var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!byte.TryParse(parts[0], out var r) || !byte.TryParse(parts[1], out var g) || !byte.TryParse(parts[2], out var b))
        {
            return false;
        }

        color = new SimpleColor(r, g, b);
        return true;
    }

    private async Task WorkerOnDoWork()
    {
        await _deviceManager.DevicesPipe.CalibrationPreview(_referenceId, _referenceColor);
        await _deviceManager.DevicesPipe.CalibrationPreview(_targetId, _targetColor);
    }

    private async void Finish()
    {
        if (!_phase3Seeded)
        {
            SeedPhase3();
        }

        var samples = _sampleSrc.Select((src, i) => new ColorSample(src, _sampleOut[i])).ToArray();
        var calibrated = _baseCalibration with { Samples = samples };

        _deviceConfig.DeviceColorCalibrations[_targetId] = calibrated;
        await _deviceManager.DevicesPipe.Recalibrate(_targetId, calibrated);
        await _deviceManager.DevicesPipe.EndCalibration();
        Close();
    }

    private CalibrationCurve BuildCurve(int channel)
    {
        // control points at the calibrated mid levels, with full intensity anchored to itself
        // (the matrix handles full-intensity tonality/scaling).
        byte[] inputs = [CurveLevels[0], CurveLevels[1], 255];
        byte[] outputs = [_curveOut[channel][0], _curveOut[channel][1], 255];
        return new CalibrationCurve(inputs, outputs);
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        // always restore normal rendering
        _deviceManager.DevicesPipe.EndCalibration();
    }

    private static void ExceptionCallback(object? sender, SingleThreadExceptionEventArgs eventArgs)
    {
        Global.logger.Error(eventArgs.Exception, "Control_CalibrationWizard._worker");
    }
}

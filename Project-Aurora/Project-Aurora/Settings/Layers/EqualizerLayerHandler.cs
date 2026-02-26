using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.AudioCapture;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.AudioVisualizer;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common;
using Common.Utils;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public enum EqualizerType
{
    [Description("Power Bars")]
    PowerBars,

    [Description("Waveform")]
    Waveform,

    [Description("Waveform (From bottom)")]
    WaveformBottom
}

public enum EqualizerPresentationType
{
    [Description("Solid Color")]
    SolidColor,

    [Description("Alternating Colors")]
    AlternatingColor,

    [Description("Gradient Notched Color")]
    GradientNotched,

    [Description("Gradient Color Shift")]
    GradientColorShift,

    [Description("Gradient (Horizontal)")]
    GradientHorizontal,

    [Description("Gradient (Vertical)")]
    GradientVertical
}

public enum EqualizerBackgroundMode
{
    [Description("Disabled")]
    Disabled,

    [Description("Always on")]
    AlwaysOn,

    [Description("On sound")]
    EnabledOnSound
}

public enum DeviceFlow
{
    [Description("Output")]
    Output,

    [Description("Input")]
    Input,
}

public partial class EqualizerLayerHandlerProperties : LayerHandlerProperties
{
    private Color? _secondaryColor;
    [JsonProperty("_SecondaryColor")]
    [LogicOverridable("Secondary Color")]
    public Color SecondaryColor
    {
        get => Logic?.SecondaryColor ?? _secondaryColor ?? Color.Empty;
        set => _secondaryColor = value;
    }

    private EffectBrush? _gradient;
    [LogicOverridable("Gradient")]
    [JsonProperty("_Gradient")]
    public EffectBrush Gradient
    {
        get => Logic?._gradient ?? _gradient ?? new EffectBrush(EffectBrush.BrushType.Linear);
        set => _gradient = value;
    }

    private EqualizerType? _eqType;
    [JsonProperty("_EQType")]
    [LogicOverridable("Equalizer Type")]
    public EqualizerType EqType
    {
        get => Logic?._eqType ?? _eqType ?? EqualizerType.PowerBars;
        set => _eqType = value;
    }


    private EqualizerPresentationType? _viewType;
    [JsonProperty("_ViewType")]
    [LogicOverridable("View Type")]
    public EqualizerPresentationType ViewType
    {
        get => Logic?._viewType ?? _viewType ?? EqualizerPresentationType.SolidColor;
        set => _viewType = value;
    }

    private EqualizerBackgroundMode? _backgroundMode;
    [JsonProperty("_BackgroundMode")]
    [LogicOverridable("Background Mode")]
    public EqualizerBackgroundMode BackgroundMode
    {
        get => Logic?._backgroundMode ?? _backgroundMode ?? EqualizerBackgroundMode.Disabled;
        set => _backgroundMode = value;
    }

    private double? _maxAmplitude;
    [JsonProperty("_MaxAmplitude")]
    [LogicOverridable("Max Amplitude")]
    public double MaxAmplitude
    {
        get => Logic?._maxAmplitude ?? _maxAmplitude ?? 20.0f;
        set => _maxAmplitude = value;
    }

    private bool? _scaleWithSystemVolume;
    [JsonProperty("_ScaleWithSystemVolume")]
    [LogicOverridable("Scale with System Volume")]
    public bool ScaleWithSystemVolume
    {
        get => Logic?._scaleWithSystemVolume ?? _scaleWithSystemVolume ?? false;
        set => _scaleWithSystemVolume = value;
    }

    private Color? _dimColor;
    [JsonProperty("_DimColor")]
    [LogicOverridable("Background Color")]
    public Color DimColor
    {
        get => Logic?._dimColor ?? _dimColor ?? Color.Empty;
        set => _dimColor = value;
    }

    private SortedSet<float>? _frequencies;
    [JsonProperty("_Frequencies")]
    public SortedSet<float> Frequencies
    {
        get => Logic?._frequencies ?? _frequencies ?? [];
        set => _frequencies = value;
    }

    private DeviceFlow? _deviceFlow;
    [LogicOverridable("Input/Output")]
    public DeviceFlow DeviceFlow
    {
        get => Logic?._deviceFlow ?? _deviceFlow ?? DeviceFlow.Output;
        set => _deviceFlow = value;
    }

    private string? _deviceId;
    [JsonProperty("_DeviceId", NullValueHandling = NullValueHandling.Ignore)]
    public string DeviceId
    {
        get => Logic?._deviceId ?? _deviceId ?? AudioDevices.DefaultDeviceId;
        set => _deviceId = value;
    }

    private double? _decay;
    [JsonProperty("_Falloff")]
    [LogicOverridable("Decay Falloff")]
    public double Decay    {
        get => Logic?._decay ?? _decay ?? 0.06f;
        set => _decay = value;
    }

    private double? _decayThreshold;
    [JsonProperty("_DecayThreshold")]
    [LogicOverridable("Decay Threshold")]
    public double DecayThreshold    {
        get => Logic?._decayThreshold ?? _decayThreshold ?? 0.02f;
        set => _decayThreshold = value;
    }

    public override void Default()
    {
        base.Default();
        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();
        SecondaryColor = CommonColorUtils.GenerateRandomColor();
        _gradient = new EffectBrush(ColorSpectrum.RainbowLoop);
        _eqType = EqualizerType.PowerBars;
        _viewType = EqualizerPresentationType.SolidColor;
        _maxAmplitude = 1.0f;
        _scaleWithSystemVolume = false;
        _backgroundMode = EqualizerBackgroundMode.Disabled;
        _dimColor = Color.FromArgb(169, 0, 0, 0);
        _frequencies = [50, 95, 130, 180, 250, 350, 500, 620, 700, 850, 1200, 1600, 2200, 3000, 4100, 5600, 7700, 10000];
        _deviceId = "";
    }
}

[LayerHandlerMeta(Name = "Audio Visualizer", IsDefault = true)]
public sealed class EqualizerLayerHandler : LayerHandler<EqualizerLayerHandlerProperties, BitmapEffectLayer>
{
    public event NewLayerRendered? NewLayerRender = delegate { };

    private bool _disposed;

    private readonly Temporary<AudioDeviceProxy> _deviceProxy;

    private void DeviceChanged(object? sender, EventArgs e)
    {
        var deviceProxyValue = sender as AudioDeviceProxy;
        if (deviceProxyValue?.Device == null || deviceProxyValue.WaveIn == null)
            return;
        _freq = deviceProxyValue.Device.AudioClient.MixFormat.SampleRate;
        _frequencyCalculatorClassic.Channels = deviceProxyValue.WaveIn.WaveFormat.Channels;
    }

    private readonly List<float> _fluxArray = [];
    private const int FftLength = 1024; // NAudio fft wants powers of two! was 8192

    // Base rectangle that defines the region that is used to render the audio output
    // Higher values mean a higher initial resolution, but may increase memory usage (looking at you, waveform).
    // KEEP X AND Y AT 0
    // TODO: width same as the number of frequencies
    private static readonly RectangleF SourceRect = new(0, 0, 80, 40);
    private readonly AudioFrequencyRangeCalculatorClassic _frequencyCalculatorClassic = new(FftLength);

    private double[]? _previousFreqResults;
    private int _freq = 48000;
    private readonly SingleColorBrush _backgroundBrush = new(SimpleColor.Transparent);
    private readonly SolidBrush _solidBrush = new(Color.Transparent);
    private readonly SolidBrush _primaryBrush = new(Color.Transparent);
    private readonly SolidBrush _secondaryBrush = new(Color.Transparent);
    private EffectBrush _verticalEffectBrush = new();

    public EqualizerLayerHandler(): base("EqualizerLayer")
    {
        _deviceProxy = new Temporary<AudioDeviceProxy>(() =>
        {
            var deviceProxyFlow = Properties.DeviceFlow switch
            {
                DeviceFlow.Input => DataFlow.Capture,
                DeviceFlow.Output => DataFlow.Render,
            };
            var deviceProxy = new AudioDeviceProxy(deviceProxyFlow);
            deviceProxy.DeviceChanged += DeviceChanged;
            deviceProxy.WaveInDataAvailable += OnDataAvailable;
            deviceProxy.DeviceId = Properties.DeviceId;
            return deviceProxy;
        });
        // TODO: add ValueDestroyed event
        _deviceProxy.ValueCreated += (_, _) =>
        {
            DeviceChanged(this, EventArgs.Empty);
        };
    }

    protected override UserControl CreateControl()
    {
        return new ControlEqualizerLayer(this);
    }
    public override EffectLayer Render(IGameState gameState)
    {
        if (_disposed) return EmptyLayer.Instance;

        var deviceProxy = _deviceProxy.Value;
        var deviceProxyFlow = Properties.DeviceFlow switch
        {
            DeviceFlow.Input => DataFlow.Capture,
            DeviceFlow.Output => DataFlow.Render,
        };
        deviceProxy.Flow = deviceProxyFlow;
        deviceProxy.DeviceId = Properties.DeviceId;

        if (deviceProxy.Device == null)
            return EmptyLayer.Instance;

        var freqs = Properties.Frequencies.ToArray(); //Defined Frequencies

        if (_previousFreqResults == null || _previousFreqResults.Length < freqs.Length)
            _previousFreqResults = new double[freqs.Length];

        var localFft = _frequencyCalculatorClassic.Ffts;

        // do transformations before using them
        for (var position = 0; position < localFft.Length; position++)
        {
            var complex = localFft[position];
            complex.X *= (float)FastFourierTransform.HannWindow(position, localFft.Length);
        }

        var bgEnabled = false;
        switch (Properties.BackgroundMode)
        {
            case EqualizerBackgroundMode.EnabledOnSound:
                if (Array.Exists(localFft, bin => bin.X > 0.0005 || bin.X < -0.0005))
                {
                    bgEnabled = true;
                }
                else
                {
                    EffectLayer.Clear();
                }

                break;
            case EqualizerBackgroundMode.AlwaysOn:
                bgEnabled = true;
                break;
            case EqualizerBackgroundMode.Disabled:
                EffectLayer.Clear();
                break;
            default:
                throw new InvalidOperationException(Properties.BackgroundMode + " is not implemented");
        }

        // Use the new transform render method to draw the equalizer layer
        EffectLayer.DrawTransformed(Properties.Sequence, g =>
        {
            // The system sound as a value between 0.0 and 1.0
            var systemSoundNormalized = _frequencyCalculatorClassic.PeakValue;

            // Scale the Maximum amplitude with the system sound if enabled, so that at 100% volume the max_amp is unchanged.
            // Replaces all Properties.MaxAmplitude calls with the scaled value
            var scaledMaxAmplitude = Properties.MaxAmplitude * (Properties.ScaleWithSystemVolume ? systemSoundNormalized : 1);

            // Here we draw the equalizer relative to our source rectangle
            // and the DrawTransformed method handles sizing and positioning it correctly for us
            // Draw a rectangle background over the entire source rect if bg is enabled
            if (bgEnabled)
            {
                g.DrawRectangle(_backgroundBrush, SourceRect);
            }

            var waveStepAmount = localFft.Length / (int)SourceRect.Width;

            switch (Properties.EqType)
            {
                case EqualizerType.Waveform:
                    var halfHeight = SourceRect.Height / 2f;
                    for (var x = 0; x < (int)SourceRect.Width; x++)
                    {
                        var fi = x * waveStepAmount;
                        var fftVal = localFft.Length > fi ? localFft[fi].X : 0.0f;
                        var brush = GetBrush(fftVal, x, SourceRect.Width);
                        var yOff = Math.Max(Math.Min(fftVal / (float)scaledMaxAmplitude * 1000.0f, halfHeight), -halfHeight);
                        g.DrawRectangle(brush, new RectangleF(x, halfHeight - yOff, 1, yOff * 2), true);
                    }
                    break;
                case EqualizerType.WaveformBottom:
                    for (var x = 0; x < (int)SourceRect.Width; x++)
                    {
                        var fi = x * waveStepAmount;
                        var fftVal = localFft.Length > fi ? localFft[fi].X : 0.0f;
                        var brush = GetBrush(fftVal, x, SourceRect.Width);
                        var yOff = Math.Min(Math.Abs(fftVal / (float)scaledMaxAmplitude) * 1000.0f, SourceRect.Height);
                        g.DrawRectangle(brush, new RectangleF(x, SourceRect.Height - yOff, 1, yOff * 2), true);
                    }
                    break;
                case EqualizerType.PowerBars:
                    //Perform FFT again to get frequencies
                    FastFourierTransform.FFT(false, (int)Math.Log(FftLength, 2.0), localFft);

                    while (_fluxArray.Count < freqs.Length)
                        _fluxArray.Add(0.0f);

                    const float threshold = 300.0f;

                    for (var x = 0; x < freqs.Length - 1; x++)
                    {
                        var startF = FreqToBin(freqs[x], localFft.Length);
                        var endF = FreqToBin(freqs[x + 1], localFft.Length);
                        var flux = 0.0f;

                        for (var j = startF; j <= endF; j++)
                        {
                            var value = (float)Math.Sqrt(localFft[j].X * localFft[j].X + localFft[j].Y * localFft[j].Y);
                            var fluxCalc = (value + Math.Abs(value)) / 2;
                            if (flux < fluxCalc)
                                flux = fluxCalc;

                            flux = flux > threshold ? 0.0f : flux;
                        }
                        _fluxArray[x] = flux;
                    }

                    var barWidth = SourceRect.Width / (freqs.Length - 1);
                    for (var fX = 0; fX < freqs.Length - 1; fX++)
                    {
                        var fftVal = _fluxArray[fX] / scaledMaxAmplitude;
                        fftVal = Math.Min(1.0f, fftVal);

                        if (_previousFreqResults[fX] - fftVal > Properties.DecayThreshold)
                            fftVal = _previousFreqResults[fX] - Properties.Decay;

                        var x = fX * barWidth;
                        var y = SourceRect.Height;
                        var height = fftVal * SourceRect.Height;

                        _previousFreqResults[fX] = fftVal;

                        var brush = GetBrush(-(fX % 2), fX, freqs.Length - 1);
                        g.DrawRectangle(brush, new RectangleF(x, (float)(y - height), barWidth, (float)height), true);
                    }
                    break;
                default:
                    throw new InvalidOperationException(Properties.BackgroundMode + " is not implemented");
            }
        }, SourceRect);
        _frequencyCalculatorClassic.Reset();

        NewLayerRender?.Invoke(EffectLayer.GetBitmap());
        return EffectLayer;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        _frequencyCalculatorClassic.OnDataAvailable(e);
    }

    private int FreqToBin(float freq, int length)
    {
        return (int)freq / (_freq / length);
    }

    private Brush GetBrush(float value, float position, float maxPosition)
    {
        switch (Properties.ViewType)
        {
            case EqualizerPresentationType.AlternatingColor:
                return value >= 0 ? _primaryBrush : _secondaryBrush;
            case EqualizerPresentationType.GradientNotched:
                _solidBrush.Color = Properties.Gradient.GetColorSpectrum().GetColorAt(position, maxPosition);
                return _solidBrush;
            case EqualizerPresentationType.GradientHorizontal:
            {
                var eBrush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                    Start = PointF.Empty,
                    End = new PointF(SourceRect.Width, 0)
                };

                return eBrush.GetDrawingBrush();
            }
            case EqualizerPresentationType.GradientColorShift:
                _solidBrush.Color = Properties.Gradient.GetColorSpectrum().GetColorAt(Time.GetMilliSeconds(), 1000);
                return _solidBrush;
            case EqualizerPresentationType.GradientVertical:
            {
                return _verticalEffectBrush.GetDrawingBrush();
            }
            case EqualizerPresentationType.SolidColor:
            default:
                return _primaryBrush;
        }
    }

    private EffectBrush CreateVerticalEffectBrush()
    {
        var eBrush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
            Start = new PointF(0, SourceRect.Height),
            End = PointF.Empty
        };
        return eBrush;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        _backgroundBrush.Color = (SimpleColor)Properties.DimColor;
        _primaryBrush.Color = Properties.PrimaryColor;
        _secondaryBrush.Color = Properties.SecondaryColor;

        _verticalEffectBrush = CreateVerticalEffectBrush();
    }

    public override void Dispose()
    {
        base.Dispose();

        _disposed = true;
        if (!_deviceProxy.HasValue) return;
        var deviceProxy = _deviceProxy.Value;
        deviceProxy.WaveInDataAvailable -= OnDataAvailable;
        deviceProxy.DeviceChanged -= DeviceChanged; 
        deviceProxy.Dispose();

        _backgroundBrush.Dispose();
        _solidBrush.Dispose();
        _primaryBrush.Dispose();
        _secondaryBrush.Dispose();
    }
}
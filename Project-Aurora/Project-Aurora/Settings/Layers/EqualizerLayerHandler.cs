using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.AudioCapture;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
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

public class EqualizerLayerHandlerProperties : LayerHandlerProperties<EqualizerLayerHandlerProperties>
{
    private Color? _secondaryColor;
    [JsonProperty("_SecondaryColor")]
    [LogicOverridable("Secondary Color")]
    public Color SecondaryColor
    {
        get => Logic?._secondaryColor ?? _secondaryColor ?? Color.Empty;
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

    private float? _maxAmplitude;
    [JsonProperty("_MaxAmplitude")]
    [LogicOverridable("Max Amplitude")]
    public float MaxAmplitude
    {
        get => Logic?._maxAmplitude ?? _maxAmplitude ?? 20.0f;
        set => _maxAmplitude = value;
    }

    [LogicOverridable("Scale with System Volume")]
    public bool? _ScaleWithSystemVolume { get; set; }
    [JsonIgnore]
    public bool ScaleWithSystemVolume => Logic?._ScaleWithSystemVolume ?? _ScaleWithSystemVolume ?? false;

    [LogicOverridable("Background Color")]
    public Color? _DimColor { get; set; }
    [JsonIgnore]
    public Color DimColor => Logic?._DimColor ?? _DimColor ?? Color.Empty;

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

    public EqualizerLayerHandlerProperties() { }

    public EqualizerLayerHandlerProperties(bool empty) : base(empty) { }

    public override void Default()
    {
        base.Default();
        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();
        _secondaryColor = CommonColorUtils.GenerateRandomColor();
        _gradient = new EffectBrush(ColorSpectrum.RainbowLoop);
        _eqType = EqualizerType.PowerBars;
        _viewType = EqualizerPresentationType.SolidColor;
        _maxAmplitude = 1.0f;
        _ScaleWithSystemVolume = false;
        _backgroundMode = EqualizerBackgroundMode.Disabled;
        _DimColor = Color.FromArgb(169, 0, 0, 0);
        _frequencies = new SortedSet<float> { 50, 95, 130, 180, 250, 350, 500, 620, 700, 850, 1200, 1600, 2200, 3000, 4100, 5600, 7700, 10000 };
        _deviceId = "";
    }
}

[LayerHandlerMeta(Name = "Audio Visualizer", IsDefault = true)]
public class EqualizerLayerHandler : LayerHandler<EqualizerLayerHandlerProperties>
{
    public event NewLayerRendered? NewLayerRender = delegate { };

    private AudioDeviceProxy? _deviceProxy;
    private int _channels;
    private int _bitsPerSample;
    private int _bufferIncrement;
    private bool _disposed;
    private AudioDeviceProxy DeviceProxy {  
        get {
            var deviceProxyFlow = Properties.DeviceFlow switch
            {
                DeviceFlow.Input => DataFlow.Capture,
                DeviceFlow.Output => DataFlow.Render,
            };
            if (_deviceProxy != null)
            {
                _deviceProxy.Flow = deviceProxyFlow;
                _deviceProxy.DeviceId = Properties.DeviceId;
                return _deviceProxy;
            }

            _deviceProxy = new AudioDeviceProxy(deviceProxyFlow);
            DeviceChanged(this, EventArgs.Empty);
            _deviceProxy.DeviceChanged += DeviceChanged;
            _deviceProxy.WaveInDataAvailable += OnDataAvailable;
            _deviceProxy.DeviceId = Properties.DeviceId;
            return _deviceProxy;
        }
    }

    private void DeviceChanged(object? sender, EventArgs e)
    {
        if (_deviceProxy?.Device == null || _deviceProxy.WaveIn == null)
            return;
        _freq = _deviceProxy.Device.AudioClient.MixFormat.SampleRate;
        _channels = _deviceProxy.WaveIn.WaveFormat.Channels;
        _bitsPerSample = _deviceProxy.WaveIn.WaveFormat.BitsPerSample;
        _bufferIncrement = _deviceProxy.WaveIn.WaveFormat.BlockAlign;
    }

    private readonly List<float> _fluxArray = [];
    private const int FftLength = 1024; // NAudio fft wants powers of two! was 8192

    // Base rectangle that defines the region that is used to render the audio output
    // Higher values mean a higher initial resolution, but may increase memory usage (looking at you, waveform).
    // KEEP X AND Y AT 0
    private static readonly RectangleF SourceRect = new(0, 0, 80, 40);

    private readonly SampleAggregator _sampleAggregator = new(FftLength);
    private Complex[] _ffts;
    private Complex[] _fftsPrev;

    private float[]? _previousFreqResults;
    private int _freq = 48000;
    private readonly SolidBrush _backgroundBrush = new(Color.Transparent);

    private long _lastRender = Time.GetMillisecondsSinceEpoch();
    private Timer? _aliveTimer;
    private readonly double _inactiveTimeMilliseconds = TimeSpan.FromSeconds(20).TotalMilliseconds;

    public EqualizerLayerHandler(): base("EqualizerLayer")
    {
        _ffts = new Complex[FftLength];
        _fftsPrev = new Complex[FftLength];

        _sampleAggregator.FftCalculated += FftCalculated;
        _sampleAggregator.PerformFft = true;
        
        StartAliveTimer();
    }

    private void StartAliveTimer()
    {
        _aliveTimer = new Timer(AliveTimerCallback, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
    }

    private void AliveTimerCallback(object? state)
    {
        var now = Time.GetMillisecondsSinceEpoch();
        if (now - _lastRender <= _inactiveTimeMilliseconds || _deviceProxy == null) return;
        
        Global.logger.Information("Audio Layer not being used, disposing device");
        // 20 sec passed since last render, dispose proxy
        var deviceProxy = _deviceProxy;
        _deviceProxy = null;
        deviceProxy.Dispose();

        _aliveTimer?.Dispose();
        _aliveTimer = null;
    }

    protected override UserControl CreateControl()
    {
        return new ControlEqualizerLayer(this);
    }
    public override EffectLayer Render(IGameState gamestate)
    {
        if (_disposed) return EffectLayer.EmptyLayer;

        if (DeviceProxy.Device == null)
            return EffectLayer.EmptyLayer;

        _lastRender = Time.GetMillisecondsSinceEpoch();
        if (_aliveTimer == null)
        {
            StartAliveTimer();
        }

        // The system sound as a value between 0.0 and 1.0
        var systemSoundNormalized = DeviceProxy.Device?.AudioMeterInformation?.MasterPeakValue ?? 1f;

        // Scale the Maximum amplitude with the system sound if enabled, so that at 100% volume the max_amp is unchanged.
        // Replaces all Properties.MaxAmplitude calls with the scaled value
        var scaledMaxAmplitude = Properties.MaxAmplitude * (Properties.ScaleWithSystemVolume ? systemSoundNormalized : 1);

        var freqs = Properties.Frequencies.ToArray(); //Defined Frequencies

        var freqResults = new double[freqs.Length];

        if (_previousFreqResults == null || _previousFreqResults.Length < freqs.Length)
            _previousFreqResults = new float[freqs.Length];

        var localFft = _ffts;
        var localFftPrevious = _fftsPrev;

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
            // Here we draw the equalizer relative to our source rectangle
            // and the DrawTransformed method handles sizing and positioning it correctly for us
            g.CompositingMode = CompositingMode.SourceCopy;
            g.CompositingQuality = CompositingQuality.Invalid;
            // Draw a rectangle background over the entire source rect if bg is enabled
            if (bgEnabled)
            {
                g.FillRectangle(_backgroundBrush, SourceRect);
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
                        var yOff = -Math.Max(Math.Min(fftVal / scaledMaxAmplitude * 1000.0f, halfHeight), -halfHeight);
                        g.DrawLine(new Pen(brush), x, halfHeight, x, halfHeight + yOff);
                        brush.Dispose();
                    }
                    break;
                case EqualizerType.WaveformBottom:
                    for (var x = 0; x < (int)SourceRect.Width; x++)
                    {
                        var fi = x * waveStepAmount;
                        var fftVal = localFft.Length > fi ? localFft[fi].X : 0.0f;
                        var brush = GetBrush(fftVal, x, SourceRect.Width);
                        g.DrawLine(new Pen(brush), x, SourceRect.Height, x,
                            SourceRect.Height - Math.Min(Math.Abs(fftVal / scaledMaxAmplitude) * 1000.0f,
                                SourceRect.Height));
                        brush.Dispose();
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
                        var startF = FreqToBin(freqs[x]);
                        var endF = FreqToBin(freqs[x + 1]);
                        var flux = 0.0f;

                        for (var j = startF; j <= endF; j++)
                        {
                            var currFft =
                                (float)Math.Sqrt(localFft[j].X * localFft[j].X + localFft[j].Y * localFft[j].Y);
                            var prevFft = (float)Math.Sqrt(localFftPrevious[j].X * localFftPrevious[j].X +
                                                           localFftPrevious[j].Y * localFftPrevious[j].Y);

                            var value = currFft - prevFft;
                            var fluxCalc = (value + Math.Abs(value)) / 2;
                            if (flux < fluxCalc)
                                flux = fluxCalc;

                            flux = flux > threshold ? 0.0f : flux;
                        }
                        _fluxArray[x] = flux;
                    }

                    var barWidth = SourceRect.Width / (freqs.Length - 1);
                    for (var fX = 0; fX < freqResults.Length - 1; fX++)
                    {
                        var fftVal = _fluxArray[fX] / scaledMaxAmplitude;
                        fftVal = Math.Min(1.0f, fftVal);

                        if (_previousFreqResults[fX] - fftVal > 0.10)
                            fftVal = _previousFreqResults[fX] - 0.15f;

                        var x = fX * barWidth;
                        var y = SourceRect.Height;
                        var height = fftVal * SourceRect.Height;

                        _previousFreqResults[fX] = fftVal;

                        var brush = GetBrush(-(fX % 2), fX, freqResults.Length - 1);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.FillRectangle(brush, x, y - height, barWidth, height);
                        brush.Dispose();
                    }
                    break;
                default:
                    throw new InvalidOperationException(Properties.BackgroundMode + " is not implemented");
            }
        }, SourceRect);
        
        NewLayerRender?.Invoke(EffectLayer.GetBitmap());
        return EffectLayer;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var buffer = e.Buffer;
        var bytesRecorded = e.BytesRecorded;

        // 4 bytes per channel, bufferIncrement is numChannels * 4
        for (var index = 0; index < bytesRecorded; index += _bufferIncrement) // Loop over the bytes, respecting the channel grouping
        {
            var single = BitConverter.ToSingle(buffer, index);
            _sampleAggregator.Add(_channels switch
            {
                2 => Math.Max(single, BitConverter.ToSingle(buffer, index + 4)),
                _ => single
            });
        }
    }

    private void FftCalculated(object? sender, FftEventArgs e)
    {
        _fftsPrev = _ffts;
        _ffts = e.Result;
    }

    private int FreqToBin(float freq)
    {
        return (int)freq / (_freq / _ffts.Length);
    }

    private Brush GetBrush(float value, float position, float maxPosition)
    {
        switch (Properties.ViewType)
        {
            case EqualizerPresentationType.AlternatingColor:
                return value >= 0 ? new SolidBrush(Properties.PrimaryColor) : new SolidBrush(Properties.SecondaryColor);
            case EqualizerPresentationType.GradientNotched:
                return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(position, maxPosition));
            case EqualizerPresentationType.GradientHorizontal:
            {
                var eBrush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                    Start = PointF.Empty,
                    End = new PointF(SourceRect.Width, 0)
                };

                return eBrush.GetDrawingBrush();
            }
            case EqualizerPresentationType.GradientColorShift:
                return new SolidBrush(Properties.Gradient.GetColorSpectrum().GetColorAt(Time.GetMilliSeconds(), 1000));
            case EqualizerPresentationType.GradientVertical:
            {
                var eBrush = new EffectBrush(Properties.Gradient.GetColorSpectrum()) {
                    Start = new PointF(0, SourceRect.Height),
                    End = PointF.Empty
                };

                return eBrush.GetDrawingBrush();
            }
            case EqualizerPresentationType.SolidColor:
            default:
                return new SolidBrush(Properties.PrimaryColor);
        }
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        _backgroundBrush.Color = Properties.DimColor;
    }

    public override void Dispose()
    {
        base.Dispose();

        _disposed = true;
        if (_deviceProxy == null) return;
        _deviceProxy.WaveInDataAvailable -= OnDataAvailable;
        _deviceProxy.DeviceChanged -= DeviceChanged; 
        _deviceProxy.Dispose();
        _deviceProxy = null;
    }
}

public class SampleAggregator
{
    // FFT
    public event EventHandler<FftEventArgs>? FftCalculated;
    public bool PerformFft { get; set; }

    // This Complex is NAudio's own! 
    private readonly Complex[] _fftBuffer;
    private readonly FftEventArgs _fftArgs;
    private int _fftPos;
    private readonly int _fftLength;

    public SampleAggregator(int fftLength)
    {
        if (!IsPowerOfTwo(fftLength))
        {
            throw new ArgumentException("FFT Length must be a power of two");
        }

        _fftLength = fftLength;
        _fftBuffer = new Complex[fftLength];
        _fftArgs = new FftEventArgs(_fftBuffer);
    }

    private bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }

    public void Add(float value)
    {
        if (!PerformFft || FftCalculated == null) return;
        // Remember the window function! There are many others as well.
        _fftBuffer[_fftPos].X = (float)(value * FastFourierTransform.HannWindow(_fftPos, _fftLength));
        _fftBuffer[_fftPos].Y = 0; // This is always zero with audio.
        _fftPos++;
        if (_fftPos < _fftLength) return;
        _fftPos = 0;
        FftCalculated(this, _fftArgs);
    }
}

public class FftEventArgs(Complex[] result) : EventArgs
{
    public Complex[] Result { get; } = result;
}
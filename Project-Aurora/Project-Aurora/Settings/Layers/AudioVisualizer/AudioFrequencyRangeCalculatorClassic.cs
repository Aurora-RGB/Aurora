using System;
using NAudio.Dsp;
using NAudio.Wave;

namespace AuroraRgb.Settings.Layers.AudioVisualizer;

public sealed class AudioFrequencyRangeCalculatorClassic : IAudioFrequencyRangeCalculator
{
    private readonly int _fftLength;
    private readonly SampleAggregator _sampleAggregator;

    public int Channels { get; set; } = 2;

    public Complex[] Ffts { get; private set; }

    public float PeakValue { get; private set; } = 1f;

    public AudioFrequencyRangeCalculatorClassic(int fftLength)
    {
        Ffts = new Complex[fftLength];

        _fftLength = fftLength;
        _sampleAggregator = new SampleAggregator(fftLength);
        _sampleAggregator.FftCalculated += FftCalculated;
    }

    public void OnDataAvailable(WaveInEventArgs e)
    {
        var waveBuffer = new WaveBuffer(e.Buffer) { ByteBufferCount = e.BytesRecorded };
        var bufferCount = waveBuffer.FloatBufferCount;
        var fftIndexRatio = (double)_fftLength / bufferCount;
        var buffer = waveBuffer.FloatBuffer;
        PeakValue = 1;

        for (var freqPlusChannel = 0; freqPlusChannel < bufferCount; freqPlusChannel += Channels)
        {
            var nextFreq = freqPlusChannel + Channels;

            var max = buffer[freqPlusChannel];
            for (var i = freqPlusChannel + 1; i < nextFreq; i++)
            {
                max = Math.Max(max, buffer[i]);
            }

            var fftIndex = (int)Math.Floor(freqPlusChannel * fftIndexRatio);
            _sampleAggregator.Add(max, fftIndex);
            PeakValue = Math.Max(PeakValue, max);
        }

        waveBuffer.Clear();
        _sampleAggregator.Complete();
    }

    public void FftCalculated(object? sender, FftEventArgs e)
    {
        Ffts = e.Result;
    }

    public void Reset()
    {
        _sampleAggregator.Reset();
    }


    private sealed class SampleAggregator
    {
        // FFT
        public event EventHandler<FftEventArgs>? FftCalculated;

        // This Complex is NAudio's own! 
        private readonly Complex[] _fftBuffer;
        private readonly Complex[] _emptyFftBuffer;
        private readonly FftEventArgs _fftArgs;

        public SampleAggregator(int fftLength)
        {
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }

            _fftBuffer = new Complex[fftLength];
            _emptyFftBuffer = new Complex[fftLength];
            _fftArgs = new FftEventArgs(_fftBuffer);
        }

        private static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public void Add(float value, int position)
        {
            if (FftCalculated == null) return;
            // Remember the window function! There are many others as well.
            ref var p = ref _fftBuffer[position];
            if (float.IsNaN(p.X))
            {
                p.X = 0;
            }

            // just save the max value. Transformations will be done when they are needed
            p.X = Math.Max(p.X, value);
            p.Y = 0; // This is always zero with audio.
        }

        public void Complete()
        {
            FftCalculated?.Invoke(this, _fftArgs);
        }

        public void Reset()
        {
            Array.Copy(_emptyFftBuffer, _fftBuffer, _fftBuffer.Length);
        }
    }
}
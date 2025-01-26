using NAudio.Dsp;
using NAudio.Wave;

namespace AuroraRgb.Settings.Layers.AudioVisualizer;

public interface IAudioFrequencyRangeCalculator
{
    int Channels { get; set; }
    Complex[] Ffts { get; }
    void OnDataAvailable(WaveInEventArgs e);
    void FftCalculated(object? sender, FftEventArgs e);
    void Reset();
}
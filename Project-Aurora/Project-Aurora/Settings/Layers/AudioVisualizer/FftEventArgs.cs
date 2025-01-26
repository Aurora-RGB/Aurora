using System;
using NAudio.Dsp;

namespace AuroraRgb.Settings.Layers.AudioVisualizer;

public class FftEventArgs(Complex[] result) : EventArgs
{
    public Complex[] Result { get; } = result;
}
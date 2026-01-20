using System;
using System.Collections.Generic;
using System.Drawing;
using AuroraRgb.Settings;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public interface EffectLayer : IDisposable
{
    DeviceKeys[] ActiveKeys { get; }

    /// <summary>
    /// Fills entire EffectLayer with a specified color.
    /// </summary>
    /// <param name="color">Color to be used during bitmap fill</param>
    void Fill(ref readonly Color color);

    /// <summary>
    /// Paints over the entire bitmap of the EffectLayer with a specified color.
    /// </summary>
    /// <param name="color">Color to be used during bitmap fill</param>
    /// <returns>Itself</returns>
    void FillOver(ref readonly Color color);

    void Clear();

    /// <summary>
    /// Sets a specific DeviceKeys on the bitmap with a specified color.
    /// </summary>
    /// <param name="key">DeviceKey to be set</param>
    /// <param name="color">Color to be used</param>
    /// <returns>Itself</returns>
    void Set(DeviceKeys key, ref readonly Color color);

    /// <summary>
    /// Sets a specific DeviceKeys on the bitmap with a specified color.
    /// </summary>
    /// <param name="keys">Array of DeviceKeys to be set</param>
    /// <param name="color">Color to be used</param>
    void Set(ICollection<DeviceKeys> keys, ref readonly Color color);

    /// <summary>
    /// Sets a specific KeySequence on the bitmap with a specified color.
    /// </summary>
    /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
    /// <param name="color">Color to be used</param>
    /// <returns>Itself</returns>
    void Set(KeySequence sequence, ref readonly Color color);

    /// <summary>
    /// + Operator, sums two EffectLayer together.
    /// </summary>
    /// <param name="lhs">Left Hand Side EffectLayer</param>
    /// <param name="rhs">Right Hand Side EffectLayer</param>
    /// <returns>Left hand side EffectLayer, which is a combination of two passed EffectLayers</returns>
    EffectLayer Add(EffectLayer other);

    /// <summary>
    /// Excludes provided sequence from the layer (Applies a mask)
    /// </summary>
    /// <param name="sequence">The mask to be applied</param>
    void Exclude(KeySequence sequence);

    /// <summary>
    /// Inlcudes provided sequence from the layer (Applies a mask)
    /// </summary>
    void OnlyInclude(KeySequence sequence);

    void SetOpacity(double layerOpacity);

    /// <summary>
    /// Retrieves a color of the specified DeviceKeys key from the bitmap
    /// </summary>
    Color Get(DeviceKeys key);

    void Close();
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using AuroraRgb.Bitmaps;
using AuroraRgb.BrushAdapters;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Common;
using Common.Devices;
using Common.Utils;
using Point = System.Drawing.Point;

namespace AuroraRgb.EffectsEngine;

/// <summary>
/// A class representing a bitmap layer for effects
/// </summary>
public sealed class BitmapEffectLayer : EffectLayer
{
    private static readonly Color TransparentColor = Color.Transparent;

    // Yes, this is no thread-safe but Aurora isn't supposed to take up resources
    // This is done to prevent memory leaks from creating new brushes
    private readonly SingleColorBrush _solidBrush = new()
    {
        Color = SimpleColor.Transparent,
    };

    private readonly string _name;
    //TODO set readability based on GPU or not
    private readonly bool _readable = true;
    private IAuroraBitmap _colormap;

    private bool _invalidated = true;

    internal Rectangle Dimension;

    private IBitmapReader? _bitmapReader;

    public DeviceKeys[] ActiveKeys => Effects.Canvas.Keys;

    [Obsolete("This creates too much garbage memory")]
    public BitmapEffectLayer(string name)
    {
        _name = name;
        _colormap = new RuntimeChangingBitmap(Effects.Canvas.Width, Effects.Canvas.Height);
        Dimension = new Rectangle(0, 0, Effects.Canvas.Width, Effects.Canvas.Height);
    }

    public BitmapEffectLayer(string name, bool persistent) : this(name)
    {
        if (!persistent)
            return;
        WeakEventManager<Effects, EventArgs>.AddHandler(null, nameof(Effects.CanvasChanged), InvalidateColorMap);
    }

    /// <summary>
    /// Retrieves a color of the specified DeviceKeys key from the bitmap
    /// </summary>
    public Color Get(DeviceKeys key)
    {
        if (KeyExcluded(key))
        {
            return TransparentColor;
        }
        
        var keyRectangle = Effects.Canvas.GetRectangle(key);

        if (keyRectangle.IsEmpty)
            return TransparentColor;
        ref readonly var color = ref GetColor(in keyRectangle.Rectangle);
        return color;
    }

    private ref readonly Color GetColor(ref readonly Rectangle rectangle)
    {
        _bitmapReader ??= _colormap.CreateReader();
        return ref _bitmapReader.GetRegionColor(rectangle);
    }

    public void Close()
    {
        _bitmapReader?.Dispose();
        _bitmapReader = null;
    }

    /// <summary>
    /// Creates a new instance of the EffectLayer class with a specified layer name. And applies a LayerEffect onto this EffectLayer instance.
    /// Using the parameters from LayerEffectConfig and a specified region in RectangleF
    /// </summary>
    /// <param name="effect">An enum specifying which LayerEffect to apply</param>
    /// <param name="effectConfig">Configurations for the LayerEffect</param>
    /// <param name="rect">A rectangle specifying what region to apply effects in</param>
    public void DrawGradient(LayerEffects effect, LayerEffectConfig effectConfig, RectangleF rect = new())
    {
        Clear();

        effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 0.067f * effectConfig.Speed;
        effectConfig.ShiftAmount %= Effects.Canvas.BiggestSize;
            
        var shift = effectConfig.AnimationType switch
        {
            AnimationType.TranslateXy => effectConfig.ShiftAmount,
            AnimationType.ZoomIn when effectConfig.Brush.Type == EffectBrush.BrushType.Radial =>
                (Effects.Canvas.BiggestSize - effectConfig.ShiftAmount) * 40.0f % Effects.Canvas.BiggestSize,
            AnimationType.ZoomOut when effectConfig.Brush.Type == EffectBrush.BrushType.Radial =>
                effectConfig.ShiftAmount * 40.0f % Effects.Canvas.BiggestSize,
            _ => 0,
        };
        if (effectConfig.AnimationReverse)
            shift *= -1.0f;

        switch (effect)
        {
            case LayerEffects.ColorOverlay:
                FillOver(effectConfig.Primary);
                break;
            case LayerEffects.ColorBreathing:
                FillOver(effectConfig.Primary);
                var sine = (float)Math.Pow(Math.Sin((double)(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0f) * 2 * Math.PI * effectConfig.Speed), 2);
                FillOver(Color.FromArgb((byte)(sine * 255), effectConfig.Secondary));
                break;
            case LayerEffects.RainbowShift_Horizontal:
                DrawRainbowGradient(0, shift);
                break;
            case LayerEffects.RainbowShift_Vertical:
                DrawRainbowGradient(90, shift);
                break;
            case LayerEffects.RainbowShift_Custom_Angle:
                DrawRainbowGradient(effectConfig.Angle, shift);
                break;
            case LayerEffects.GradientShift_Custom_Angle:
            {
                var brush = effectConfig.Brush.GetDrawingBrush();
                switch (effectConfig.Brush.Type)
                {
                    case EffectBrush.BrushType.Linear:
                    {
                        var linearBrush = (LinearGradientBrush)brush;
                        linearBrush.ResetTransform();
                        if (!rect.IsEmpty)
                        {
                            linearBrush.TranslateTransform(rect.X, rect.Y);
                            linearBrush.ScaleTransform(rect.Width * 100 / effectConfig.GradientSize, rect.Height * 100 / effectConfig.GradientSize);
                        }
                        else
                        {
                            linearBrush.ScaleTransform(Dimension.Height * 100 / effectConfig.GradientSize, Dimension.Height * 100 / effectConfig.GradientSize);
                        }

                        linearBrush.RotateTransform(effectConfig.Angle);
                        linearBrush.TranslateTransform(shift, shift);
                        break;
                    }
                    case EffectBrush.BrushType.Radial:
                    {
                        var radialBrush = (PathGradientBrush)brush;
                        radialBrush.ResetTransform();
                        if (effectConfig.AnimationType is AnimationType.ZoomIn or AnimationType.ZoomOut)
                        {
                            var percent = shift / Effects.Canvas.BiggestSize;
                            var xOffset = Effects.Canvas.Width / 2.0f * percent;
                            var yOffset = Effects.Canvas.Height / 2.0f * percent;

                            radialBrush.WrapMode = WrapMode.Clamp;

                            if (!rect.IsEmpty)
                            {
                                xOffset = rect.Width / 2.0f * percent;
                                yOffset = rect.Height / 2.0f * percent;

                                radialBrush.TranslateTransform(rect.X + xOffset, rect.Y + yOffset);
                                radialBrush.ScaleTransform((rect.Width - 2.0f * xOffset) * 100 / effectConfig.GradientSize, (rect.Height - 2.0f * yOffset) * 100 / effectConfig.GradientSize);
                            }
                            else
                            {
                                radialBrush.ScaleTransform((Effects.Canvas.Height + xOffset) * 100 / effectConfig.GradientSize, (Effects.Canvas.Height + yOffset) * 100 / effectConfig.GradientSize);
                            }
                        }
                        else
                        {
                            if (!rect.IsEmpty)
                            {
                                radialBrush.TranslateTransform(rect.X, rect.Y);
                                radialBrush.ScaleTransform(rect.Width * 100 / effectConfig.GradientSize, rect.Height * 100 / effectConfig.GradientSize);
                            }
                            else
                            {
                                radialBrush.ScaleTransform(Effects.Canvas.Height * 100 / effectConfig.GradientSize, Effects.Canvas.Height * 100 / effectConfig.GradientSize);
                            }
                        }

                        radialBrush.RotateTransform(effectConfig.Angle);
                        break;
                    }
                }

                Fill(brush);
                break;
            }
        }

        effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
    }

    private void DrawRainbowGradient(float angle, float shift)
    {
        var rainbowBrush = CreateRainbowBrush();
        rainbowBrush.RotateTransform(angle);
        rainbowBrush.TranslateTransform(shift, shift);

        Fill(rainbowBrush);
        rainbowBrush.Dispose();
    }

    public void Dispose()
    {
        _excludeSequence = new KeySequence();
        _keySequence = new KeySequence();
        _lastFreeform = new FreeFormObject();
        _colormap.Dispose();
        _solidBrush.Dispose();
        _excludeKeysCache.Dispose();
        _onlyIncludeKeysCache?.Dispose();
    }

    /// <summary>
    /// Creates a rainbow gradient brush, to be used in effects.
    /// </summary>
    /// <returns>Rainbow LinearGradientBrush</returns>
    private LinearGradientBrush CreateRainbowBrush()
    {
        Color[] colors =
        [
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 127, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(0, 0, 255),
            Color.FromArgb(75, 0, 130),
            Color.FromArgb(139, 0, 255),
            Color.FromArgb(255, 0, 0)
        ];
        var numColors = colors.Length;
        var blendPositions = new float[numColors];
        for (var i = 0; i < numColors; i++)
        {
            blendPositions[i] = i / (numColors - 1f);
        }
            
        var colorBlend = new ColorBlend
        {
            Colors = colors,
            Positions = blendPositions
        };

        var brush = new LinearGradientBrush(
            new Point(0, 0),
            new Point(Effects.Canvas.BiggestSize, 0),
            Color.Red, Color.Red);
        brush.InterpolationColors = colorBlend;

        return brush;
    }

    public void Fill(ref readonly Color color)
    {
        _colormap.Reset();
        _solidBrush.Color = (SimpleColor)color;
        _colormap.ReplaceRectangle(_solidBrush, Dimension);
        Invalidate();
    }

    /// <summary>
    /// Fills the entire bitmap of the EffectLayer with a specified brush.
    /// </summary>
    /// <param name="brush">Brush to be used during bitmap fill</param>
    public void Fill(Brush brush)
    {
        _colormap.Reset();
        _colormap.ReplaceRectangle(brush, Dimension);
        Invalidate();
    }

    /// <inheritdoc />
    public void FillOver(ref readonly Color color)
    {
        _colormap.Reset();
        _solidBrush.Color = (SimpleColor)color;
        _colormap.DrawRectangle(_solidBrush, Dimension);

        Invalidate();
    }

    /// <summary>
    /// Paints over the entire bitmap of the EffectLayer with a specified color.
    /// </summary>
    /// <param name="color">Color to be used during bitmap fill</param>
    /// <returns>Itself</returns>
    public void FillOver(Brush brush)
    {
        _colormap.Reset();
        _colormap.DrawRectangle(brush, Dimension);
        Invalidate();
    }

    public void Clear()
    {
        _colormap.Fill(ClearingBrush);
        _lastColor = Color.Empty;
        Invalidate();
    }

    private FreeFormObject _lastFreeform = new();
    private bool _ksChanged = true;

    /// <inheritdoc />
    public void Set(DeviceKeys key, ref readonly Color color)
    {
        SetOneKey(key, in color);
    }

    /// <inheritdoc />
    public void Set(ICollection<DeviceKeys> keys, ref readonly Color color)
    {
        foreach (var key in keys)
            SetOneKey(key, in color);
    }

    /// <inheritdoc />
    public void Set(KeySequence sequence, ref readonly Color color)
    {
        _solidBrush.Color = (SimpleColor)color;
        Set(sequence, _solidBrush);
    }

    /// <summary>
    /// Sets a specific KeySequence on the bitmap with a specified brush.
    /// </summary>
    /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
    /// <param name="brush">Brush to be used</param>
    /// <returns>Itself</returns>
    public void Set(KeySequence sequence, Brush brush)
    {
        if (!ReferenceEquals(sequence, _keySequence))
        {
            WeakEventManager<ObservableCollection<DeviceKeys>, EventArgs>.RemoveHandler(_keySequence.Keys,
                nameof(_keySequence.Keys.CollectionChanged), FreeformOnValuesChanged);
            _keySequence = sequence;
            WeakEventManager<ObservableCollection<DeviceKeys>, EventArgs>.AddHandler(_keySequence.Keys,
                nameof(_keySequence.Keys.CollectionChanged), FreeformOnValuesChanged);
            FreeformOnValuesChanged(this, EventArgs.Empty);
        }
        if (_previousSequenceType != sequence.Type)
        {
            _previousSequenceType = sequence.Type;
            _ksChanged = true;
        }

        if (_ksChanged && !_invalidated)
        {
            Clear();
            _ksChanged = false;
        }

        if (sequence.Type == KeySequenceType.Sequence)
        {
            foreach (var key in sequence.Keys)
                SetOneKey(key, brush);
        }
        else
        {
            if (!ReferenceEquals(sequence.Freeform, _lastFreeform))
            {
                WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(_lastFreeform, nameof(_lastFreeform.ValuesChanged), FreeformOnValuesChanged);
                _lastFreeform = sequence.Freeform;
                WeakEventManager<FreeFormObject, EventArgs>.AddHandler(_lastFreeform, nameof(_lastFreeform.ValuesChanged), FreeformOnValuesChanged);
                FreeformOnValuesChanged(this, EventArgs.Empty);
            }
            else if (brush is SolidBrush solidBrush)
            {
                if (sequence.Freeform.Equals(_lastFreeform) && _lastColor == solidBrush.Color)
                {
                    return;
                }
                _lastColor = solidBrush.Color;
            }

            var xPos = (float)Math.Round((sequence.Freeform.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth);
            var yPos = (float)Math.Round((sequence.Freeform.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight);
            var width = (float)Math.Round(sequence.Freeform.Width * Effects.Canvas.EditorToCanvasWidth);
            var height = (float)Math.Round(sequence.Freeform.Height * Effects.Canvas.EditorToCanvasHeight);

            if (width < 3) width = 3;
            if (height < 3) height = 3;

            var rect = new RectangleF(xPos, yPos, width, height);   //TODO dependant property? parameter?

            var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);
            using var myMatrix = new Matrix();
            myMatrix.RotateAt(sequence.Freeform.Angle, rotatePoint, MatrixOrder.Append);    //TODO dependant property? parameter?

            _colormap.Reset();
            _colormap.SetTransform(myMatrix);
            _colormap.ReplaceRectangle(brush, rect);
            Invalidate();
        }
    }

    public void Set(KeySequence sequence, IAuroraBrush brush)
    {
        if (!ReferenceEquals(sequence, _keySequence))
        {
            WeakEventManager<ObservableCollection<DeviceKeys>, EventArgs>.RemoveHandler(_keySequence.Keys,
                nameof(_keySequence.Keys.CollectionChanged), FreeformOnValuesChanged);
            _keySequence = sequence;
            WeakEventManager<ObservableCollection<DeviceKeys>, EventArgs>.AddHandler(_keySequence.Keys,
                nameof(_keySequence.Keys.CollectionChanged), FreeformOnValuesChanged);
            FreeformOnValuesChanged(this, EventArgs.Empty);
        }
        if (_previousSequenceType != sequence.Type)
        {
            _previousSequenceType = sequence.Type;
            _ksChanged = true;
        }

        if (_ksChanged && !_invalidated)
        {
            Clear();
            _ksChanged = false;
        }

        if (sequence.Type == KeySequenceType.Sequence)
        {
            foreach (var key in sequence.Keys)
                SetOneKey(key, brush);
        }
        else
        {
            if (!ReferenceEquals(sequence.Freeform, _lastFreeform))
            {
                WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(_lastFreeform, nameof(_lastFreeform.ValuesChanged), FreeformOnValuesChanged);
                _lastFreeform = sequence.Freeform;
                WeakEventManager<FreeFormObject, EventArgs>.AddHandler(_lastFreeform, nameof(_lastFreeform.ValuesChanged), FreeformOnValuesChanged);
                FreeformOnValuesChanged(this, EventArgs.Empty);
            }
            else if (brush is SingleColorBrush solidBrush)
            {
                if (sequence.Freeform.Equals(_lastFreeform) && _lastColorSimple == solidBrush.Color)
                {
                    return;
                }
                _lastColorSimple = solidBrush.Color;
            }

            var xPos = (float)Math.Round((sequence.Freeform.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth);
            var yPos = (float)Math.Round((sequence.Freeform.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight);
            var width = (float)Math.Round(sequence.Freeform.Width * Effects.Canvas.EditorToCanvasWidth);
            var height = (float)Math.Round(sequence.Freeform.Height * Effects.Canvas.EditorToCanvasHeight);

            if (width < 3) width = 3;
            if (height < 3) height = 3;

            var rect = new RectangleF(xPos, yPos, width, height);   //TODO dependant property? parameter?

            var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);
            using var myMatrix = new Matrix();
            myMatrix.RotateAt(sequence.Freeform.Angle, rotatePoint, MatrixOrder.Append);    //TODO dependant property? parameter?

            _colormap.Reset();
            _colormap.SetTransform(myMatrix);
            _colormap.ReplaceRectangle(brush, rect);
            Invalidate();
        }
    }

    private void FreeformOnValuesChanged(object? sender, EventArgs args)
    {
        _ksChanged = true;
    }

    /// <summary>
    /// Allows drawing some arbitrary content to the sequence bounds, including translation, scaling and rotation.<para/>
    /// Usage:<code>
    /// someEffectLayer.DrawTransformed(Properties.Sequence,<br/>
    ///     m => {<br/>
    ///         // We are prepending the transformations since we want the mirroring to happen BEFORE the rotation and scaling happens.<br/>
    ///         m.Translate(100, 0, MatrixOrder.Prepend); // These two are backwards because we are Prepending (so this is prepended first)<br/>
    ///         m.Scale(-1, 1, MatrixOrder.Prepend); // Then this is prepended before the tranlate.<br/>
    ///     },<br/>
    ///     gfx => {<br/>
    ///         gfx.FillRectangle(Brushes.Red, 0, 0, 30, 100);<br/>
    ///         gfx.FillRectangle(Brushes.Blue, 70, 0, 30, 100);<br/>
    ///     },
    ///     new RectangleF(0, 0, 100, 100);</code>
    /// This code will draw an X-mirrored image of a red stipe and a blue stripe (with a transparent gap in between) to the target keysequence area.
    /// </summary>
    /// <param name="sequence">The target sequence whose bounds will be used as the target location on the drawing canvas.</param>
    /// <param name="configureMatrix">An action that further configures the transformation matrix before render is called.</param>
    /// <param name="render">An action that receives a transformed graphics context and can render whatever it needs to.</param>
    /// <param name="sourceRegion">The source region of the rendered content. This is used when calculating the transformation matrix, so that this
    ///     rectangle in the render context is transformed to the keysequence bounds in the layer's context. Note that no clipping is performed.</param>
    public void DrawTransformed(KeySequence sequence, Action<Matrix> configureMatrix, Action<IAuroraBitmap> render, RectangleF sourceRegion)
    {
        // The matrix represents the transformation that will be applied to the rendered content
        using var matrix = new Matrix();

        // The bounds represent the target position of the render part
        var bounds = sequence.GetAffectedRegion();

        // First, calculate the scaling required to transform the sourceRect's size into the bounds' size
        float sx = bounds.Width / sourceRegion.Width, sy = bounds.Height / sourceRegion.Height;

        // Perform this scale first
        // Note: that if the scale is zero, when setting the graphics transform to the matrix,
        // it throws an error, so we must have NON-ZERO values
        // Note 2: Also tried using float.Epsilon but this also caused the exception,
        // so a somewhat small number will have to suffice. Not noticed any visual issues with 0.001f.
        matrix.Scale(Math.Max(.001f, sx), Math.Max(.001f, sy), MatrixOrder.Append);

        // Second, for freeform objects, apply the rotation. This needs to be done AFTER the scaling,
        // else the scaling is applied to the rotated object, which skews it
        // We rotate around the central point of the source region, but we need to take the scaling of the dimensions into account
        if (sequence.Type == KeySequenceType.FreeForm)
            matrix.RotateAt(
                sequence.Freeform.Angle,
                new PointF((sourceRegion.Left + sourceRegion.Width / 2f) * sx, (sourceRegion.Top + sourceRegion.Height / 2f) * sy), MatrixOrder.Append);

        // Third, we can translate the matrix from the source to the target location.
        matrix.Translate(bounds.X - sourceRegion.Left, bounds.Y - sourceRegion.Top, MatrixOrder.Append);

        // Finally, call the custom matrix configure action
        configureMatrix(matrix);

        // Apply the matrix transform to the graphics context and then render
        _colormap.Reset();
        _colormap.SetClip(bounds);
        _colormap.SetTransform(matrix);
        render(_colormap);

        _colormap.Reset();
        _colormap.OnlyInclude(sequence);

        Invalidate();
    }

    /// <summary>
    /// Allows drawing some arbitrary content to the sequence bounds, including translation, scaling and rotation.<para/>
    /// See <see cref="DrawTransformed(KeySequence, Action{Matrix}, Action{Graphics}, RectangleF)"/> for usage.
    /// </summary>
    /// <param name="sequence">The target sequence whose bounds will be used as the target location on the drawing canvas.</param>
    /// <param name="render">An action that receives a transformed graphics context and can render whatever it needs to.</param>
    /// <param name="sourceRegion">The source region of the rendered content. This is used when calculating the transformation matrix, so that this
    ///     rectangle in the render context is transformed to the keysequence bounds in the layer's context. Note that no clipping is performed.</param>
    public void DrawTransformed(KeySequence sequence, Action<IAuroraBitmap> render, RectangleF sourceRegion)
    {
        DrawTransformed(sequence, DefaultMatrixAction, render, sourceRegion);
    }

    /// <summary>
    /// Allows drawing some arbitrary content to the sequence bounds, including translation, scaling and rotation.
    /// Uses the full canvas size as the source region.<para/>
    /// See <see cref="DrawTransformed(KeySequence, Action{Matrix}, Action{Graphics}, RectangleF)"/> for usage.
    /// </summary>
    /// <param name="sequence">The target sequence whose bounds will be used as the target location on the drawing canvas.</param>
    /// <param name="render">An action that receives a transformed graphics context and can render whatever it needs to.</param>
    public void DrawTransformed(KeySequence sequence, Action<IAuroraBitmap> render)
    {
        DrawTransformed(sequence, render, Dimension);
    }

    /// <summary>
    /// Sets one DeviceKeys key with a specific color on the bitmap
    /// </summary>
    /// <param name="key">DeviceKey to be set</param>
    /// <param name="color">Color to be used</param>
    private void SetOneKey(DeviceKeys key, ref readonly Color color)
    {
        _solidBrush.Color = (SimpleColor)color;
        SetOneKey(key, _solidBrush);
    }

    private static readonly SolidBrush ClearingBrush = new(Color.Transparent);
    private static readonly Action<Matrix> DefaultMatrixAction = _ => { };
    private Color _lastColor = Color.Empty;
    private SimpleColor _lastColorSimple = SimpleColor.Transparent;

    /// <summary>
    /// Sets one DeviceKeys key with a specific brush on the bitmap
    /// </summary>
    /// <param name="key">DeviceKey to be set</param>
    /// <param name="brush">Brush to be used</param>
    private void SetOneKey(DeviceKeys key, Brush brush)
    {
        var keyRectangle = Effects.Canvas.GetRectangle(key);
        _invalidated = true;

        if (keyRectangle.Top < 0 || keyRectangle.Bottom > Effects.Canvas.Height ||
            keyRectangle.Left < 0 || keyRectangle.Right > Effects.Canvas.Width)
        {
            Global.logger.Warning("Couldn't set key color {Key}. Key is outside canvas", key);
            return;
        }

        try
        {
            _colormap.Reset();
            _colormap.ReplaceRectangle(brush, keyRectangle.Rectangle);
        }catch { /* ignore */}
    }

    private void SetOneKey(DeviceKeys key, IAuroraBrush brush)
    {
        var keyRectangle = Effects.Canvas.GetRectangle(key);
        _invalidated = true;

        if (keyRectangle.Top < 0 || keyRectangle.Bottom > Effects.Canvas.Height ||
            keyRectangle.Left < 0 || keyRectangle.Right > Effects.Canvas.Width)
        {
            Global.logger.Warning("Couldn't set key color {Key}. Key is outside canvas", key);
            return;
        }

        try
        {
            _colormap.Reset();
            _colormap.ReplaceRectangle(brush, keyRectangle.Rectangle);
        }catch { /* ignore */}
    }

    public IAuroraBitmap GetGraphics()
    {
        Invalidate();
        _colormap.Reset();
        return _colormap;
    }

    public IAuroraBitmap GetBitmap()
    {
        return _colormap;
    }

    /// <inheritdoc/>
    public EffectLayer Add(EffectLayer other)
    {
        if (ReferenceEquals(other, EmptyLayer.Instance))
        {
            return this;
        }

        var g = GetGraphics();
        switch (other)
        {
            case BitmapEffectLayer bitmapEffectLayer:
                g.DrawRectangle(bitmapEffectLayer);
                break;
            case NoRenderLayer noRenderLayer:
                var activeKeys = noRenderLayer.ActiveKeys;
                foreach (var key in activeKeys)
                {
                    _solidBrush.Color = (SimpleColor)noRenderLayer.Get(key);
                    var bitmapRectangle = Effects.Canvas.GetRectangle(key).Rectangle;
                    g.DrawRectangle(_solidBrush, bitmapRectangle);
                }
                break;
            case EmptyLayer:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(other), other, null);
        }

        Invalidate();
        return this;
    }

    /// <inheritdoc />
    public void SetOpacity(double value)
    {
        if (MathUtils.NearlyEqual(_colormap.Opacity, (float)value, 0.0001f)) return;
        _colormap.Opacity = (float) value;
        Invalidate();
    }

    private KeySequenceType _previousSequenceType;
    private KeySequence _keySequence = new();

    /// <summary>
    /// Draws a percent effect on the layer bitmap using a KeySequence with solid colors.
    /// </summary>
    /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    public void PercentEffect(Color foregroundColor, ref readonly Color backgroundColor, KeySequence sequence, double value,
        double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive,
        double flashPast = 0.0, bool flashReversed = false, bool blinkBackground = false)
    {
        if (_previousSequenceType != sequence.Type)
        {
            Clear();
            _previousSequenceType = sequence.Type;
        }
        if (sequence.Type == KeySequenceType.Sequence)
            PercentEffect(foregroundColor, backgroundColor, sequence.Keys, value, total, percentEffectType,
                flashPast, flashReversed, blinkBackground);
        else
            PercentEffect(foregroundColor, backgroundColor, sequence.Freeform, value, total, percentEffectType, flashPast, flashReversed, blinkBackground);
    }

    /// <summary>
    /// Draws a percent effect on the layer bitmap using a KeySequence and a ColorSpectrum.
    /// </summary>
    /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    public void PercentEffect(ColorSpectrum spectrum, KeySequence sequence, double value, double total = 1.0D,
        PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
        bool flashReversed = false)
    {
        if (_previousSequenceType != sequence.Type)
        {
            Clear();
            _previousSequenceType = sequence.Type;
        }
        if (sequence.Type == KeySequenceType.Sequence)
            PercentEffect(spectrum, sequence.Keys.ToArray(), value, total, percentEffectType, flashPast, flashReversed);
        else
            PercentEffect(spectrum, sequence.Freeform,       value, total, percentEffectType, flashPast, flashReversed);
    }

    /// <summary>
    /// Draws a percent effect on the layer bitmap using an array of DeviceKeys keys and solid colors.
    /// </summary>
    /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    public void PercentEffect(Color foregroundColor, Color backgroundColor, IReadOnlyList<DeviceKeys> keys, double value,
        double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
        bool flashReversed = false, bool blinkBackground = false)
    {
        var progressTotal = value / total;
        if (progressTotal < 0.0)
            progressTotal = 0.0;
        else if (progressTotal > 1.0)
            progressTotal = 1.0;

        var progress = progressTotal * keys.Count;

        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            if (blinkBackground)
                backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
            else
                foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
        }

        if (percentEffectType is PercentEffectType.Highest_Key or PercentEffectType.Highest_Key_Blend && keys.Count > 0)
        {
            var activeKey = (int)Math.Ceiling(Math.Clamp(value, 0, 1) / (total / keys.Count)) - 1;
            var col = percentEffectType == PercentEffectType.Highest_Key ?
                foregroundColor : ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal);
            for (var i = 0; i < keys.Count; i++)
            {
                if (i != activeKey)
                {
                    SetOneKey(keys[i], Color.Transparent);
                }
            }
            SetOneKey(keys[activeKey], col);

        }
        else
        {
            for (var i = 0; i < keys.Count; i++)
            {
                var currentKey = keys[i];

                switch (percentEffectType)
                {
                    case PercentEffectType.AllAtOnce:
                        SetOneKey(currentKey, ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal));
                        break;
                    case PercentEffectType.Progressive_Gradual:
                        if (i == (int)progress)
                        {
                            var percent = progress - i;
                            SetOneKey(currentKey, ColorUtils.BlendColors(backgroundColor, foregroundColor, percent));
                        }
                        else if (i < (int)progress)
                            SetOneKey(currentKey, foregroundColor);
                        else
                            SetOneKey(currentKey, backgroundColor);
                        break;
                    default:
                        SetOneKey(currentKey, i < (int) progress ? foregroundColor : backgroundColor);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Draws a percent effect on the layer bitmap using DeviceKeys keys and a ColorSpectrum.
    /// </summary>
    /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    private void PercentEffect(ColorSpectrum spectrum, DeviceKeys[] keys, double value, double total,
        PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
        bool flashReversed = false)
    {
        var progressTotal = (value / total) switch
        {
            < 0.0 => 0.0,
            > 1.0 => 1.0,
            _ => value / total
        };
        if (progressTotal < _percentProgress)
        {
            Clear();
        }
        _percentProgress = progressTotal;

        var progress = progressTotal * keys.Length;

        var flashAmount = 1.0;

        if (flashPast > 0.0)
        {
            if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
            {
                flashAmount = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            }
        }

        switch (percentEffectType)
        {
            case PercentEffectType.AllAtOnce:
                foreach (var currentKey in keys)
                {
                    SetOneKey(currentKey, spectrum.GetColorAt((float)progressTotal, 1.0f, flashAmount));
                }
                break;
            case PercentEffectType.Progressive_Gradual:
                Clear();
                for (var i = 0; i < keys.Length; i++)
                {
                    var currentKey = keys[i];
                    var color = spectrum.GetColorAt((float)i / (keys.Length - 1), 1.0f, flashAmount);
                    if (i == (int)progress)
                    {
                        var percent = progress - i;
                        SetOneKey(
                            currentKey,
                            ColorUtils.MultiplyColorByScalar(color, percent)
                        );
                    }
                    else if (i < (int)progress)
                        SetOneKey(currentKey, color);
                }
                break;
            default:
                for (var i = 0; i < keys.Length; i++)
                {
                    var currentKey = keys[i];
                    if (i < (int)progress)
                        SetOneKey(currentKey, spectrum.GetColorAt((float)i / (keys.Length - 1), 1.0f, flashAmount));
                }
                break;
        }
    }

    /// <summary>
    /// Draws a percent effect on the layer bitmap using a FreeFormObject and solid colors.
    /// </summary>
    /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
    /// <param name="freeform">The FreeFormObject that the progress effect will be drawn on</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    private void PercentEffect(Color foregroundColor, Color backgroundColor, FreeFormObject freeform, double value,
        double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
        bool flashReversed = false, bool blinkBackground = false)
    {
        var progressTotal = value / total;
        switch (progressTotal)
        {
            case < 0.0:
            case double.NaN:
                progressTotal = 0.0;
                break;
            case > 1.0:
                progressTotal = 1.0;
                break;
        }

        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            switch (blinkBackground)
            {
                case false:
                    foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
                    break;
                case true:
                    backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
                    break;
            }
        }

        var xPos = (float)Math.Round((freeform.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth);
        var yPos = (float)Math.Round((freeform.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight);
        var width = freeform.Width * Effects.Canvas.EditorToCanvasWidth;
        var height = freeform.Height * Effects.Canvas.EditorToCanvasHeight;

        if (width < 3) width = 3;
        if (height < 3) height = 3;
            
        _colormap.Reset();
        var g = _colormap;
        if (percentEffectType == PercentEffectType.AllAtOnce)
        {
            var rect = new RectangleF(xPos, yPos, width, height);

            var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

            using var myMatrix = new Matrix();
            myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

            g.SetTransform(myMatrix);
            var color = ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal);
            _solidBrush.Color = (SimpleColor)color;
            g.DrawRectangle(_solidBrush, rect);
        }
        else
        {
            var progress = progressTotal * width;
            var rect = new RectangleF(xPos, yPos, (float)progress, height);
            var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

            using var myMatrix = new Matrix();
            myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

            g.SetTransform(myMatrix);

            var rectRest = new RectangleF(xPos, yPos, width, height);
            _solidBrush.Color = (SimpleColor)backgroundColor;
            g.DrawRectangle(_solidBrush, rectRest);
            _solidBrush.Color = (SimpleColor)foregroundColor;
            g.DrawRectangle(_solidBrush, rect);
        }

        Invalidate();
    }

    public void Invalidate()
    {
        _bitmapReader = null;
        _invalidated = true;
    }
    private void InvalidateColorMap(object? sender, EventArgs args)
    {
        var oldBitmap = _colormap;
        _colormap = new RuntimeChangingBitmap(Effects.Canvas.Width, Effects.Canvas.Height);
        oldBitmap.Dispose();
        _ksChanged = true;
        Dimension.Height = Effects.Canvas.Height;
        Dimension.Width = Effects.Canvas.Width;
        Invalidate();
    }

    private double _percentProgress = -1;
    /// <summary>
    ///  Draws a percent effect on the layer bitmap using a FreeFormObject and a ColorSpectrum.
    /// </summary>
    /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
    /// <param name="freeform">The FreeFormObject that the progress effect will be drawn on</param>
    /// <param name="value">The current progress value</param>
    /// <param name="total">The maxiumum progress value</param>
    private void PercentEffect(ColorSpectrum spectrum, FreeFormObject freeform, double value, double total,
        PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
        bool flashReversed = false)
    {
        var progressTotal = MathUtils.Clamp(value / total, 0, 1);
        if (progressTotal < _percentProgress)
        {
            Clear();
        }
        _percentProgress = progressTotal;

        var flashAmount = 1.0;

        if (flashPast > 0.0)
        {
            if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
            {
                flashAmount = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            }
        }

        var xPos = (float)Math.Round((freeform.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth);
        var yPos = (float)Math.Round((freeform.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight);
        var width = freeform.Width * Effects.Canvas.EditorToCanvasWidth;
        var height = freeform.Height * Effects.Canvas.EditorToCanvasHeight;

        if (width < 2) width = 2;
        if (height < 2) height = 2;

        _colormap.Reset();
        var g = _colormap;
        if (percentEffectType == PercentEffectType.AllAtOnce)
        {
            var rect = new RectangleF(xPos, yPos, width, height);

            var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

            using var myMatrix = new Matrix();
            myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

            g.SetTransform(myMatrix);
            var color = spectrum.GetColorAt((float)progressTotal, 1.0f, flashAmount);
            _solidBrush.Color = (SimpleColor)color;
            g.DrawRectangle(_solidBrush, rect);
        }
        else
        {
            var progress = progressTotal * width;

            var rect = new RectangleF(xPos, yPos, (float)progress, height);

            var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

            using var myMatrix = new Matrix();
            myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

            g.SetTransform(myMatrix);
            var brush = spectrum.ToLinearGradient(width, 0, xPos, 0, flashAmount);
            brush.WrapMode = WrapMode.Tile;
            g.DrawRectangle(brush, rect);
        }
        Invalidate();
    }

    private KeySequence _excludeSequence = new();
    private readonly ZoneKeysCache _excludeKeysCache = new();
    private ZoneKeysCache? _onlyIncludeKeysCache;

    /// <inheritdoc />
    public void Exclude(KeySequence sequence)
    {
        if (_excludeSequence.Equals(sequence))
        {
            return;
        }

        var freeform = _excludeSequence.Freeform;
        WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(freeform, nameof(freeform.ValuesChanged), FreeformOnValuesChanged);
        _excludeKeysCache.SetSequence(sequence);
        _excludeSequence = sequence;
        WeakEventManager<FreeFormObject, EventArgs>.AddHandler(freeform, nameof(freeform.ValuesChanged), FreeformOnValuesChanged);
        FreeformOnValuesChanged(this, EventArgs.Empty);
 
        Invalidate();
    }

    /// <inheritdoc />
    public void OnlyInclude(KeySequence sequence)
    {
        _colormap.OnlyInclude(sequence);
        _onlyIncludeKeysCache ??= new();
        _onlyIncludeKeysCache.SetSequence(sequence);
        Invalidate();
    }

    private bool KeyExcluded(DeviceKeys key)
    {
        if (_excludeKeysCache.GetKeys().Contains(key)) return true;
        if (_onlyIncludeKeysCache == null) return false;
        return !_onlyIncludeKeysCache.GetKeys().Contains(key);
    }

    public override string ToString() => _name;
}
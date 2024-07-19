using System;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using ColorBox.Implementation;

namespace AuroraRgb.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_GradientFillLayer.xaml
/// </summary>
public partial class Control_GradientFillLayer
{
    private bool settingsset;

    public Control_GradientFillLayer()
    {
        InitializeComponent();
    }

    public Control_GradientFillLayer(GradientFillLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not GradientFillLayerHandler || settingsset) return;
        effect_speed_slider.Value = ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Speed;
        effect_speed_label.Text = "x " + ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Speed;
        CheckBox_FillEntire.IsChecked = ((GradientFillLayerHandler)DataContext).Properties._FillEntireKeyboard;
        var brush = ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Brush.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }

        KeySequence_keys.Sequence = ((GradientFillLayerHandler)DataContext).Properties._Sequence;

        settingsset = true;
    }

    private void Gradient_editor_BrushChanged(object? sender, BrushChangedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is GradientFillLayerHandler && sender is ColorBox.Implementation.ColorBox colorBox)
            ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Brush = new EffectBrush(colorBox.Brush);
    }

    private void Button_SetGradientRainbow_Click(object? sender, RoutedEventArgs e)
    {
        ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Brush = new EffectBrush(ColorSpectrum.Rainbow);

        var brush = ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Brush.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }
    }

    private void Button_SetGradientRainbowLoop_Click(object? sender, RoutedEventArgs e)
    {
        ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Brush = new EffectBrush(ColorSpectrum.RainbowLoop);

        var brush = ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Brush.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }
    }

    private void effect_speed_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || !settingsset || DataContext is not GradientFillLayerHandler || sender is not Slider slider) return;
        ((GradientFillLayerHandler)DataContext).Properties.GradientConfig.Speed = (float)slider.Value;

        if (effect_speed_label != null)
            effect_speed_label.Text = "x " + slider.Value;
    }

    private void CheckBox_FillEntire_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is GradientFillLayerHandler && sender is CheckBox checkBox)
            ((GradientFillLayerHandler)DataContext).Properties._FillEntireKeyboard = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && settingsset && DataContext is GradientFillLayerHandler)
            ((GradientFillLayerHandler)DataContext).Properties._Sequence = e.NewValue;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }
}
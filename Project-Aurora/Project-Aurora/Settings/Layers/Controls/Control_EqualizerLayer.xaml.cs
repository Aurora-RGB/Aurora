using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.AudioCapture;
using AuroraRgb.Utils;
using ColorBox.Implementation;
using Xceed.Wpf.Toolkit;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_EqualizerLayer.xaml
/// </summary>
public partial class ControlEqualizerLayer
{
    private bool _settingsSet;

    private Window? _previewWindow;
    private readonly Image _previewImage = new();
    private static bool _previewWindowOpen;

    public ControlEqualizerLayer()
    {
        InitializeComponent();
    }

    public ControlEqualizerLayer(EqualizerLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not EqualizerLayerHandler || _settingsSet) return;
        affectedKeys.Sequence = ((EqualizerLayerHandler)DataContext).Properties._Sequence;

        eq_type.SelectedValue = ((EqualizerLayerHandler)DataContext).Properties.EqType;
        eq_view_type.SelectedValue = ((EqualizerLayerHandler)DataContext).Properties.ViewType;
        eq_background_mode.SelectedValue = ((EqualizerLayerHandler)DataContext).Properties.BackgroundMode;
        Clr_primary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(
            ((EqualizerLayerHandler)DataContext).Properties._PrimaryColor ?? System.Drawing.Color.Empty);
        Clr_secondary_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(((EqualizerLayerHandler)DataContext).Properties.SecondaryColor);

        var brush = ((EqualizerLayerHandler)DataContext).Properties.Gradient.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }

        updown_max_amplitude_value.Value = (int)((EqualizerLayerHandler)DataContext).Properties.MaxAmplitude;
        Clr_dim_color.SelectedColor = ColorUtils.DrawingColorToMediaColor(((EqualizerLayerHandler)DataContext).Properties.DimColor);
        lstbx_frequencies.ItemsSource = ((EqualizerLayerHandler)DataContext).Properties.Frequencies;
        chkbox_scale_with_system_volume.IsChecked = ((EqualizerLayerHandler)DataContext).Properties.ScaleWithSystemVolume;

        _settingsSet = true;
    }

    private void eq_type_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && sender is ComboBox comboBox)
            ((EqualizerLayerHandler)DataContext).Properties.EqType = (EqualizerType)comboBox.SelectedValue;
    }

    private void eq_view_type_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && sender is ComboBox comboBox)
            ((EqualizerLayerHandler)DataContext).Properties.ViewType = (EqualizerPresentationType)comboBox.SelectedValue;
    }

    private void eq_background_mode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && sender is ComboBox comboBox)
            ((EqualizerLayerHandler)DataContext).Properties.BackgroundMode = (EqualizerBackgroundMode)comboBox.SelectedValue;
    }

    private void Clr_primary_color_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler &&
            sender is ColorPicker { SelectedColor: { } } picker)
            ((EqualizerLayerHandler)DataContext).Properties._PrimaryColor =
                ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void Clr_secondary_color_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler &&
            (sender as ColorPicker)?.SelectedColor != null)
            ((EqualizerLayerHandler)DataContext).Properties.SecondaryColor =
                ColorUtils.MediaColorToDrawingColor(((ColorPicker)sender).SelectedColor.Value);
    }

    private void Gradient_editor_BrushChanged(object? sender, BrushChangedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && sender is ColorBox.Implementation.ColorBox box)
            ((EqualizerLayerHandler)DataContext).Properties.Gradient = new EffectBrush(box.Brush);
    }

    private void Button_SetGradientRainbow_Click(object? sender, RoutedEventArgs e)
    {
        ((EqualizerLayerHandler)DataContext).Properties.Gradient = new EffectBrush(ColorSpectrum.Rainbow);

        var brush = ((EqualizerLayerHandler)DataContext).Properties.Gradient.GetMediaBrush();
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
        ((EqualizerLayerHandler)DataContext).Properties.Gradient = new EffectBrush(ColorSpectrum.RainbowLoop);

        var brush = ((EqualizerLayerHandler)DataContext).Properties.Gradient.GetMediaBrush();
        try
        {
            gradient_editor.Brush = brush;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not set brush");
        }
    }

    private void updown_max_amplitude_value_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && (sender as IntegerUpDown)?.Value != null)
            ((EqualizerLayerHandler)DataContext).Properties.MaxAmplitude = ((IntegerUpDown)sender).Value.Value;
    }

    private void chkbox_scale_with_system_sound_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && (sender as CheckBox)?.IsChecked != null)
            ((EqualizerLayerHandler)DataContext).Properties.ScaleWithSystemVolume = ((CheckBox)sender).IsChecked.Value;
    }

    private void Clr_dim_color_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler && (sender as ColorPicker)?.SelectedColor != null)
            ((EqualizerLayerHandler)DataContext).Properties.DimColor = ColorUtils.MediaColorToDrawingColor(((ColorPicker)sender).SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && _settingsSet && DataContext is EqualizerLayerHandler)
        {
            ((EqualizerLayerHandler)DataContext).Properties._Sequence = e.NewValue;
        }
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();
        SetDeviceComboBoxSource();

        Loaded -= UserControl_Loaded;
    }

    private void btn_AddFreq_Click(object? sender, RoutedEventArgs e)
    {
        if (!float.TryParse(txtBox_newFreqValue.Text, out var value))
        {
            MessageBox.Show("Entered value is not a number!");
            return;
        }

        if (value is < 0.0f or > 20000.0f)
        {
            MessageBox.Show("Frequency must be in-between 0 Hz and 20000 Hz");
            return;
        }

        ((EqualizerLayerHandler)DataContext).Properties.Frequencies.Add(value);

        lstbx_frequencies.Items.Refresh();
    }

    private void btn_DeleteFreq_Click(object? sender, RoutedEventArgs e)
    {
        if (lstbx_frequencies.SelectedItem == null) return;
        ((EqualizerLayerHandler)DataContext).Properties.Frequencies.Remove((float)lstbx_frequencies.SelectedItem);

        lstbx_frequencies.Items.Refresh();
    }

    private void btn_ShowPreviewWindow_Click(object? sender, RoutedEventArgs e)
    {
        if(_previewWindow == null)
        {
            if(_previewWindowOpen)
            {
                MessageBox.Show("Equalizer preview already open for another layer.\r\nPlease close it.");
                return;
            }

            _previewWindow = new Window();
            _previewWindow.Closed += Preview_window_Closed;
            _previewWindow.ResizeMode = ResizeMode.NoResize;
            _previewWindow.SizeToContent = SizeToContent.WidthAndHeight;

            _previewWindow.Title = "Equalizer preview";
            _previewWindow.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            ((EqualizerLayerHandler)DataContext).NewLayerRender += Control_EqualizerLayer_NewLayerRender;

            _previewImage.SnapsToDevicePixels = true;
            _previewImage.HorizontalAlignment = HorizontalAlignment.Stretch;
            _previewImage.VerticalAlignment = VerticalAlignment.Stretch;
            _previewImage.MinWidth = Effects.Canvas.Width;
            _previewImage.MinHeight = Effects.Canvas.Height;
            _previewImage.Width = Effects.Canvas.Width * 4;
            _previewImage.Height = Effects.Canvas.Height * 4;

            _previewWindow.Content = _previewImage;

            _previewWindow.UpdateLayout();
            _previewWindow.Show();
        }
        else
        {
            _previewWindow.BringIntoView();
        }

        _previewWindowOpen = true;
    }

    private void Preview_window_Closed(object? sender, EventArgs e)
    {
        _previewWindow = null;
        ((EqualizerLayerHandler)DataContext).NewLayerRender -= Control_EqualizerLayer_NewLayerRender;
        _previewWindowOpen = false;
    }

    private void Control_EqualizerLayer_NewLayerRender(GdiBitmap bitmap)
    {
        try
        {
            Dispatcher.Invoke(
                () =>
                {
                    using var memory = new MemoryStream();
                    bitmap.Bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    _previewImage.Source = bitmapImage;
                }, DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            Global.logger.Warning(ex, "Error in equalizer layer render");
        }
    }

    private void InputOutput_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var layerHandler = (EqualizerLayerHandler)DataContext;
        var deviceFlow = (DeviceFlow)InputOutputComboBox.SelectedValue;
        layerHandler.Properties.DeviceFlow = deviceFlow;

        SetDeviceComboBoxSource();
    }

    private void SetDeviceComboBoxSource()
    {
        var deviceFlow = (DeviceFlow)InputOutputComboBox.SelectedValue;
        switch (deviceFlow)
        {
            case DeviceFlow.Input:
                DeviceComboBox.ItemsSource = AudioDevices.RecordingDevices;
                break;
            case DeviceFlow.Output:
                DeviceComboBox.ItemsSource = AudioDevices.PlaybackDevices;
                break;
        }
    }
}
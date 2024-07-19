using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBackgroundLayer.xaml
/// </summary>
public partial class Control_CSGOBackgroundLayer
{
    private bool settingsset;

    public Control_CSGOBackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOBackgroundLayer(CSGOBackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is not CSGOBackgroundLayerHandler layerHandler || settingsset) return;
        ColorPicker_CT.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.CtColor);
        ColorPicker_T.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.TColor);
        ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.DefaultColor);
        Checkbox_DimEnabled.IsChecked = layerHandler.Properties.DimEnabled;
        TextBox_DimValue.Content = (int)layerHandler.Properties.DimDelay + "s";
        Slider_DimSelector.Value = layerHandler.Properties.DimDelay;
        IntegerUpDown_DimAmount.Value = layerHandler.Properties.DimAmount;

        settingsset = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_CT_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.CtColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_T_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.TColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.DefaultColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void Checkbox_DimEnabled_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler layerHandler && sender is CheckBox { IsChecked: not null } box)
            layerHandler.Properties.DimEnabled  = box.IsChecked.Value;
    }

    private void Slider_DimSelector_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || !settingsset || DataContext is not CSGOBackgroundLayerHandler layerHandler || sender is not Slider slider) return;
        layerHandler.Properties.DimDelay = slider.Value;

        TextBox_DimValue.Content = (int)slider.Value + "s";
    }

    private void IntegerUpDown_DimAmount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOBackgroundLayerHandler layerHandler && sender is IntegerUpDown down)
        {
            layerHandler.Properties.DimAmount = down.Value ?? 0;
        }
    }
}
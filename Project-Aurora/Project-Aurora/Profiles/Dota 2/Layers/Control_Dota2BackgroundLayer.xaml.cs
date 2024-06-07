using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.Dota_2.Layers;

/// <summary>
/// Interaction logic for Control_Dota2BackgroundLayer.xaml
/// </summary>
public partial class Control_Dota2BackgroundLayer
{
    private bool _settingsSet;

    public Control_Dota2BackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_Dota2BackgroundLayer(Dota2BackgroundLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not Dota2BackgroundLayerHandler backgroundLayer || _settingsSet) return;
        ColorPicker_Dire.SelectedColor = ColorUtils.DrawingColorToMediaColor(backgroundLayer.Properties.DireColor);
        ColorPicker_Radiant.SelectedColor = ColorUtils.DrawingColorToMediaColor(backgroundLayer.Properties.RadiantColor);
        ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor(backgroundLayer.Properties.DefaultColor);
        Checkbox_DimEnabled.IsChecked = backgroundLayer.Properties.DimEnabled;
        TextBox_DimValue.Content = (int)backgroundLayer.Properties.DimDelay + "s";
        Slider_DimSelector.Value = backgroundLayer.Properties.DimDelay;

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Dire_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2BackgroundLayerHandler backgroundLayer && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
             backgroundLayer.Properties.DireColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Radiant_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2BackgroundLayerHandler backgroundLayer && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
             backgroundLayer.Properties.RadiantColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2BackgroundLayerHandler backgroundLayer && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
             backgroundLayer.Properties.DefaultColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void Checkbox_DimEnabled_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2BackgroundLayerHandler backgroundLayer && sender is CheckBox box && box.IsChecked.HasValue)
             backgroundLayer.Properties.DimEnabled  = box.IsChecked.Value;
    }

    private void Slider_DimSelector_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || !_settingsSet || DataContext is not Dota2BackgroundLayerHandler backgroundLayer || sender is not Slider slider) return;
        backgroundLayer.Properties.DimDelay = slider.Value;

        TextBox_DimValue.Content = (int)slider.Value + "s";
    }
}
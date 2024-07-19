using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.Witcher3.Layers;

/// <summary>
/// Interaction logic for Control_Witcher3BackgroundLayer.xaml
/// </summary>
public partial class Control_Witcher3BackgroundLayer
{
    private bool settingsset;

    public Control_Witcher3BackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_Witcher3BackgroundLayer(Witcher3BackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    private void SetSettings()
    {
        if (DataContext is not Witcher3BackgroundLayerHandler || settingsset) return;
        ColorPicker_Aard.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext).Properties.AardColor);
        ColorPicker_Igni.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext).Properties.IgniColor);
        ColorPicker_Quen.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext).Properties.QuenColor);
        ColorPicker_Yrden.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext).Properties.YrdenColor);
        ColorPicker_Axii.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext).Properties.AxiiColor);
        ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Witcher3BackgroundLayerHandler)DataContext).Properties.DefaultColor);

        settingsset = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Aard_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.AardColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Igni_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.IgniColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Quen_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.QuenColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Yrden_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.YrdenColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Axii_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.AxiiColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is Witcher3BackgroundLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.DefaultColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }
}
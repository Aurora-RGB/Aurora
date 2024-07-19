using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBurningLayer.xaml
/// </summary>
public partial class Control_CSGOBurningLayer
{
    private bool _settingsSet;

    public Control_CSGOBurningLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOBurningLayer(CSGOBurningLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    public void SetSettings()
    {
        if (DataContext is not CSGOBurningLayerHandler layerHandler || _settingsSet) return;
        ColorPicker_Burning.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.BurningColor);
        checkBox_Animated.IsChecked = layerHandler.Properties.Animated;

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Burning_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOBurningLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.BurningColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void checkBox_Animated_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOBurningLayerHandler layerHandler && sender is CheckBox { IsChecked: not null } box)
            layerHandler.Properties.Animated = box.IsChecked.Value;
    }
}
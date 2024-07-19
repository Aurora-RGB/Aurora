using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGODeathLayer.xaml
/// </summary>
public partial class Control_CSGODeathLayer
{
    private bool _settingsSet;

    public Control_CSGODeathLayer()
    {
        InitializeComponent();
    }

    public Control_CSGODeathLayer(CSGODeathLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGODeathLayerHandler layerHandler && !_settingsSet)
        {
            CSGODeathLayerHandlerProperties properties = layerHandler.Properties;

            ColorPicker_DeathColor.SelectedColor = ColorUtils.DrawingColorToMediaColor(properties.DeathColor);
            IntegerUpDown_FadeOutAfter.Value = properties.FadeOutAfter;

            _settingsSet = true;
        }
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_DeathColor_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGODeathLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.DeathColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void IntegerUpDown_FadeOutAfter_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGODeathLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown { Value: not null } down)
            layerHandler.Properties.FadeOutAfter = down.Value ?? 0;
    }
}
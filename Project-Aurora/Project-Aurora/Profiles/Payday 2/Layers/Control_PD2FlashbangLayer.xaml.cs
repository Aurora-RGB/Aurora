using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.Payday_2.Layers;

/// <summary>
/// Interaction logic for Control_PD2FlashbangLayer.xaml
/// </summary>
public partial class Control_PD2FlashbangLayer
{
    private bool _settingsSet;

    public Control_PD2FlashbangLayer()
    {
        InitializeComponent();
    }

    public Control_PD2FlashbangLayer(PD2FlashbangLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    public void SetSettings()
    {
        if (DataContext is PD2FlashbangLayerHandler layerHandler && !_settingsSet)
        {
            ColorPicker_Flashbang.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.FlashbangColor);

            _settingsSet = true;
        }
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Flashbang_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is PD2FlashbangLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.FlashbangColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }
}
using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_WinningTeamLayer.xaml
/// </summary>
public partial class Control_CSGOWinningTeamLayer
{
    private bool settingsset;

    public Control_CSGOWinningTeamLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOWinningTeamLayer(CSGOWinningTeamLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOWinningTeamLayerHandler layerHandler && !settingsset)
        {
            ColorPicker_CT.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.CtColor);
            ColorPicker_T.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.TColor);

            settingsset = true;
        }
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_CT_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOWinningTeamLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            layerHandler.Properties.CtColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }

    private void ColorPicker_T_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOWinningTeamLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            layerHandler.Properties.CtColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
    }
}
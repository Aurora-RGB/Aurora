using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.EliteDangerous.Layers;

/// <summary>
/// Interaction logic for Control_EliteDangerousBackgroundLayer.xaml
/// </summary>
public partial class Control_EliteDangerousBackgroundLayer
{
    private bool settingsset;

    public Control_EliteDangerousBackgroundLayer()
    {
        InitializeComponent();
    }

    public Control_EliteDangerousBackgroundLayer(EliteDangerousBackgroundLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is EliteDangerousBackgroundLayerHandler dataContext && !settingsset)
        {
            ColorPicker_CombatMode.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.CombatModeColor);
            ColorPicker_DiscoveryMode.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.DiscoveryModeColor);

            settingsset = true;
        }
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_CombatMode_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is EliteDangerousBackgroundLayerHandler context && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
            context.Properties.CombatModeColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_DiscoveryMode_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && settingsset && DataContext is EliteDangerousBackgroundLayerHandler context && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
            context.Properties.DiscoveryModeColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }
}
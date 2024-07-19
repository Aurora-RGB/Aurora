using System.Windows;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.Dota_2.Layers;

/// <summary>
/// Interaction logic for Control_Dota2RespawnLayer.xaml
/// </summary>
public partial class Control_Dota2RespawnLayer
{
    private bool _settingsSet;

    public Control_Dota2RespawnLayer()
    {
        InitializeComponent();
    }

    public Control_Dota2RespawnLayer(Dota2RespawnLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    public void SetSettings()
    {
        if (DataContext is not Dota2RespawnLayerHandler layerHandler || _settingsSet) return;
        ColorPicker_background.SelectedColor = ColorUtils.DrawingColorToMediaColor( layerHandler.Properties.BackgroundColor);
        ColorPicker_respawn.SelectedColor = ColorUtils.DrawingColorToMediaColor( layerHandler.Properties.RespawnColor);
        ColorPicker_respawning.SelectedColor = ColorUtils.DrawingColorToMediaColor( layerHandler.Properties.RespawningColor);
        KeySequence_sequence.Sequence =  layerHandler.Properties.Sequence;

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_background_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2RespawnLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
             layerHandler.Properties.BackgroundColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);

    }

    private void ColorPicker_respawn_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2RespawnLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
             layerHandler.Properties.RespawnColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_respawning_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2RespawnLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } picker)
             layerHandler.Properties.RespawningColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void KeySequence_sequence_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2RespawnLayerHandler layerHandler)
             layerHandler.Properties.Sequence = e.NewValue;
    }
}
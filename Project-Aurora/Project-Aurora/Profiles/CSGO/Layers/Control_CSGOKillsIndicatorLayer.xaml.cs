using System.Windows;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOKillIndicatorLayer.xaml
/// </summary>
public partial class Control_CSGOKillIndicatorLayer
{
    private bool _settingsSet;

    public Control_CSGOKillIndicatorLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOKillIndicatorLayer(CSGOKillIndicatorLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not CSGOKillIndicatorLayerHandler || _settingsSet) return;
        ColorPicker_RegularKill.SelectedColor = ColorUtils.DrawingColorToMediaColor(((CSGOKillIndicatorLayerHandler)DataContext).Properties.RegularKillColor);
        ColorPicker_HeadshotKill.SelectedColor = ColorUtils.DrawingColorToMediaColor(((CSGOKillIndicatorLayerHandler)DataContext).Properties.HeadshotKillColor);
        KeySequence_keys.Sequence = ((CSGOKillIndicatorLayerHandler)DataContext).Properties.Sequence;

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_RegularKill_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOKillIndicatorLayerHandler csgoHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            csgoHandler.Properties.RegularKillColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void ColorPicker_HeadshotKill_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOKillIndicatorLayerHandler csgoHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            csgoHandler.Properties.HeadshotKillColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOKillIndicatorLayerHandler csgoHandler)
        {
            csgoHandler.Properties.Sequence = e.NewValue;
        }
    }
}
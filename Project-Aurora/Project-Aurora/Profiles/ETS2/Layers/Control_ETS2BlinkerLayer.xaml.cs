using System.Windows;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.ETS2.Layers;

/// <summary>
/// Interaction logic for Control_ETS2BlinkerLayer.xaml
/// </summary>
public partial class Control_ETS2BlinkerLayer
{
    private bool _settingsSet;

    public Control_ETS2BlinkerLayer() {
        InitializeComponent();
    }

    public Control_ETS2BlinkerLayer(ETS2BlinkerLayerHandler datacontext) {
        InitializeComponent();
        DataContext = datacontext;
    }

    private ETS2BlinkerLayerHandler Context => (ETS2BlinkerLayerHandler)DataContext;

    private void SetSettings()
    {
        if (DataContext is not ETS2BlinkerLayerHandler || _settingsSet) return;
        ColorPicker_BlinkerOn.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties.BlinkerOnColor);
        ColorPicker_BlinkerOff.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties.BlinkerOffColor);
        LeftBlinker_keys.Sequence = Context.Properties.LeftBlinkerSequence;
        RightBlinker_keys.Sequence = Context.Properties.RightBlinkerSequence;
        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) {
        SetSettings();
        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_BlinkerOn_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
        if (IsLoaded && _settingsSet && DataContext is ETS2BlinkerLayerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            Context.Properties.BlinkerOnColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_BlinkerOff_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
        if (IsLoaded && _settingsSet && DataContext is ETS2BlinkerLayerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            Context.Properties.BlinkerOffColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void LeftBlinker_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e) {
        if (IsLoaded && _settingsSet && DataContext is ETS2BlinkerLayerHandler)
            Context.Properties.LeftBlinkerSequence = e.NewValue;
    }

    private void RightBlinker_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e) {
        if (IsLoaded && _settingsSet && DataContext is ETS2BlinkerLayerHandler)
            Context.Properties.RightBlinkerSequence = e.NewValue;
    }
}
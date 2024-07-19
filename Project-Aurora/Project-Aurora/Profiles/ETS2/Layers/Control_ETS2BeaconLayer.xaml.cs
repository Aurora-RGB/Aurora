using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;
using KeySequence = AuroraRgb.Settings.KeySequence;

namespace AuroraRgb.Profiles.ETS2.Layers;

/// <summary>
/// Interaction logic for Control_ETS2BeaconLayer.xaml
/// </summary>
public partial class Control_ETS2BeaconLayer
{

    private bool _settingsSet;

    public Control_ETS2BeaconLayer() {
        InitializeComponent();
    }

    public Control_ETS2BeaconLayer(ETS2BeaconLayerHandler datacontext) {
        DataContext = datacontext;
        InitializeComponent();
    }

    private ETS2BeaconLayerHandler context => (ETS2BeaconLayerHandler)DataContext;
    private bool IsReady => IsLoaded && _settingsSet && DataContext is ETS2BeaconLayerHandler;

    public void SetSettings()
    {
        if (DataContext is not ETS2BeaconLayerHandler || _settingsSet) return;
        lightMode.SelectedValue = context.Properties.BeaconStyle;
        speedSlider.Value = context.Properties.Speed;
        speedSlider.IsEnabled = context.Properties.BeaconStyle == ETS2_BeaconStyle.Simple_Flash;
        beaconColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(context.Properties.PrimaryColor);
        keyPicker.Sequence = context.Properties.Sequence;
        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) {
        SetSettings();
        Loaded -= UserControl_Loaded;
    }

    private void lightMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsReady || sender is not ComboBox comboBox) return;
        context.Properties.BeaconStyle = (ETS2_BeaconStyle)comboBox.SelectedValue;
        speedSlider.IsEnabled = context.Properties.BeaconStyle == ETS2_BeaconStyle.Simple_Flash;
    }

    private void speedSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        if (IsReady && sender is Slider slider)
            context.Properties.Speed = (float)slider.Value;
    }

    private void beaconColorPicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
        if (IsReady && sender is ColorPicker { SelectedColor: not null } colorPicker)
            context.Properties.PrimaryColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void keyPicker_SequenceUpdated(object sender, RoutedPropertyChangedEventArgs<KeySequence> e) {
        if (IsReady)
            context.Properties.Sequence = e.NewValue;
    }
}
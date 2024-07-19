using System.Windows;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOTypingIndicatorLayer.xaml
/// </summary>
public partial class Control_CSGOTypingIndicatorLayer
{
    private bool settingsset;

    public Control_CSGOTypingIndicatorLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOTypingIndicatorLayer(CSGOTypingIndicatorLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is CSGOTypingIndicatorLayerHandler layerHandler && !settingsset)
        {
            ColorPicker_TypingKeys.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.TypingKeysColor);
            KeySequence_keys.Sequence =  layerHandler.Properties.Sequence;

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
        if (IsLoaded && settingsset && DataContext is CSGOTypingIndicatorLayerHandler layerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker
            {
                SelectedColor: not null
            } picker)
             layerHandler.Properties.TypingKeysColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && settingsset && DataContext is CSGOTypingIndicatorLayerHandler layerHandler)
        {
             layerHandler.Properties.Sequence = e.NewValue;
        }
    }
}
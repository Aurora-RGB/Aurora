using System.Windows;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.Discord.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBackgroundLayer.xaml
/// </summary>
public partial class ControlDiscordVoiceActivityLayer
{
    private bool _settingsSet;

    public ControlDiscordVoiceActivityLayer()
    {
        InitializeComponent();
    }

    public ControlDiscordVoiceActivityLayer(DiscordVoiceActivityLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    public void SetSettings()
    {
        if (DataContext is not DiscordVoiceActivityLayerHandler layerHandler || _settingsSet) return;
        MutedColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.MutedColor);
        SpeakingColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.SpeakingColor);
        DefaultColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.DefaultColor);
        KeySequenceControl.Sequence = layerHandler.Properties.Sequence;

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Muted_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is DiscordVoiceActivityLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.MutedColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Speaking_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is DiscordVoiceActivityLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.SpeakingColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is DiscordVoiceActivityLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } picker)
            layerHandler.Properties.DefaultColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && _settingsSet && DataContext is DiscordVoiceActivityLayerHandler layerHandler)
        {
            layerHandler.Properties.Sequence = e.NewValue;
        }
    }
}
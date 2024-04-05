using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.CSGO.Layers;

/// <summary>
/// Interaction logic for Control_CSGOBombLayer.xaml
/// </summary>
public partial class Control_CSGOBombLayer
{
    private bool _settingsSet;

    public Control_CSGOBombLayer()
    {
        InitializeComponent();
    }

    public Control_CSGOBombLayer(CSGOBombLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not CSGOBombLayerHandler csgoBombLayerHandler || _settingsSet) return;

        ColorPicker_Flash.SelectedColor = ColorUtils.DrawingColorToMediaColor(csgoBombLayerHandler.Properties.FlashColor);
        ColorPicker_Primed.SelectedColor = ColorUtils.DrawingColorToMediaColor(csgoBombLayerHandler.Properties.PrimedColor);
        Checkbox_GradualEffect.IsChecked = csgoBombLayerHandler.Properties.GradualEffect;
        KeySequence_keys.Sequence = csgoBombLayerHandler.Properties.Sequence;

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_Flash_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOBombLayerHandler csgoBombLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } colorPicker)
            csgoBombLayerHandler.Properties.FlashColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void ColorPicker_Primed_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOBombLayerHandler csgoBombLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker { SelectedColor: not null } colorPicker)
            csgoBombLayerHandler.Properties.PrimedColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void Checkbox_GradualEffect_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOBombLayerHandler csgoBombLayerHandler && sender is CheckBox { IsChecked: not null } checkBox)
            csgoBombLayerHandler.Properties.GradualEffect = checkBox.IsChecked.Value;
    }

    private void KeySequence_keys_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && _settingsSet && DataContext is CSGOBombLayerHandler csgoBombLayerHandler)
        {
            csgoBombLayerHandler.Properties._Sequence = e.NewValue;
        }
    }
}
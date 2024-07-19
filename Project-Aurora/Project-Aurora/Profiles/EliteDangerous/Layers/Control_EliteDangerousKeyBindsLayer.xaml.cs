using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.EliteDangerous.Layers;

/// <summary>
/// Interaction logic for Control_EliteDangerousKeyBindsLayer.xaml
/// </summary>
public partial class Control_EliteDangerousKeyBindsLayer
{
    private bool _settingsset;

    public Control_EliteDangerousKeyBindsLayer()
    {
        InitializeComponent();
    }

    public Control_EliteDangerousKeyBindsLayer(EliteDangerousKeyBindsLayerHandler datacontext)
    {
        InitializeComponent();

        DataContext = datacontext;
    }

    public void SetSettings()
    {
        if (DataContext is EliteDangerousKeyBindsLayerHandler dataContext && !_settingsset)
        {
            ColorPicker_HudModeCombat.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.HudModeCombatColor);
            ColorPicker_HudModeDiscovery.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.HudModeDiscoveryColor);
            ColorPicker_Ui.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.UiColor);
            ColorPicker_UiAlt.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.UiAltColor);
            ColorPicker_ShipStuff.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.ShipStuffColor);
            ColorPicker_Camera.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.CameraColor);
            ColorPicker_Defence.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.DefenceColor);
            ColorPicker_Offence.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.OffenceColor);
            ColorPicker_MovementSpeed.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.MovementSpeedColor);
            ColorPicker_MovementSecondary.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.MovementSecondaryColor);
            ColorPicker_Wing.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.WingColor);
            ColorPicker_Navigation.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.NavigationColor);
            ColorPicker_ModeEnable.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.ModeEnableColor);
            ColorPicker_ModeDisable.SelectedColor = ColorUtils.DrawingColorToMediaColor(dataContext.Properties.ModeDisableColor);

            _settingsset = true;
        }
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void ColorPicker_HudModeCombat_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.HudModeCombatColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_HudModeDiscovery_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.HudModeDiscoveryColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Ui_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.UiColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_UiAlt_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.UiAltColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_ShipStuff_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.ShipStuffColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Camera_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.CameraColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Defence_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.DefenceColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Offence_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.OffenceColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_MovementSpeed_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.MovementSpeedColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_MovementSecondary_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.MovementSecondaryColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Wing_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.WingColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_Navigation_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.NavigationColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_ModeEnable_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.ModeEnableColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }

    private void ColorPicker_ModeDisable_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsset && DataContext is EliteDangerousKeyBindsLayerHandler dataContext && sender is ColorPicker { SelectedColor: not null } picker)
            dataContext.Properties.ModeDisableColor = ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value);
    }
}
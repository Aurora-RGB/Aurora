using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Settings.Controls;

/// <summary>
/// Interaction logic for Control_LayerControlPresenter.xaml
/// </summary>
public partial class Control_LayerControlPresenter
{
    private static readonly Canvas EmptyContent = new();
    private bool _isSettingNewLayer;

    private Layer? _Layer;

    public Layer? Layer { get => _Layer;
        set { _Layer = value; SetLayer(value); } }

    public Control_LayerControlPresenter()
    {
        InitializeComponent();
    }

    private async void SetLayer(Layer layer)
    {
        _isSettingNewLayer = true;

        DataContext = layer;

        cmbLayerType.ItemsSource = layer.AssociatedApplication.AllowedLayers.OrderBy(l => l.Order).ThenBy(l => l.Name);
        cmbLayerType.SelectedValue = Layer.Handler.GetType();

        ctrlLayerTypeConfig.Content = EmptyContent;
        ctrlLayerTypeConfig.Content = await layer.Control;
        chkLayerSmoothing.IsChecked = Layer.Handler.EnableSmoothing;
        chk_ExcludeMask.IsChecked = Layer.Handler._EnableExclusionMask ?? false;
        keyseq_ExcludeMask.Sequence = Layer.Handler._ExclusionMask;
        sldr_Opacity.Value = (Layer.Handler._Opacity ?? 1d) * 100.0;
        lbl_Opacity_Text.Text = $"{(int)sldr_Opacity.Value} %";

        grdLayerConfigs.Visibility = Visibility.Hidden;
        overridesEditor.Visibility = Visibility.Hidden;
        btnConfig.Visibility = Visibility.Visible;
        btnOverrides.Visibility = Visibility.Visible;
        grd_LayerControl.IsHitTestVisible = true;
        grd_LayerControl.Effect = null;
        _isSettingNewLayer = false;

        overridesEditor.Layer = layer;
    }

    private void cmbLayerType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _isSettingNewLayer || sender is not ComboBox comboBox) return;
        _Layer?.Dispose();
        ResetLayer((Type)comboBox.SelectedValue);
    }

    private async void ResetLayer(Type type)
    {
        if (!IsLoaded || _isSettingNewLayer || type == null) return;

        _Layer.Handler = (ILayerHandler)Activator.CreateInstance(type);

        ctrlLayerTypeConfig.Content = EmptyContent;
        ctrlLayerTypeConfig.Content = await _Layer.Control;
        chkLayerSmoothing.IsChecked = _Layer.Handler.EnableSmoothing;
        chk_ExcludeMask.IsChecked = Layer.Handler._EnableExclusionMask ?? false;
        keyseq_ExcludeMask.Sequence = Layer.Handler._ExclusionMask;
        sldr_Opacity.Value = (int)(Layer.Handler.Opacity * 100.0f);
        lbl_Opacity_Text.Text = $"{(int)sldr_Opacity.Value} %";
        await _Layer.AssociatedApplication.SaveProfiles();

        overridesEditor.ForcePropertyListUpdate();
    }

    private void btnReset_Click(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && !_isSettingNewLayer && sender is Button)
        {
            ResetLayer((Type)cmbLayerType.SelectedValue);
        }
    }

    private void btnConfig_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _isSettingNewLayer || sender is not Button) return;

        var v = grdLayerConfigs.IsVisible;
        grdLayerConfigs.Visibility = v ? Visibility.Hidden : Visibility.Visible;
        grd_LayerControl.IsHitTestVisible = v;
        grd_LayerControl.Effect = v ? null : new BlurEffect();
        btnOverrides.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
    }

    private void chkLayerSmoothing_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && !_isSettingNewLayer && sender is CheckBox checkBox)
            Layer.Handler.EnableSmoothing = checkBox.IsChecked.Value;
    }

    private void chk_ExcludeMask_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && !_isSettingNewLayer && sender is CheckBox checkBox)
            Layer.Handler._EnableExclusionMask = checkBox.IsChecked.Value;
    }

    private void keyseq_ExcludeMask_SequenceUpdated(object? sender, RoutedPropertyChangedEventArgs<KeySequence> e)
    {
        if (IsLoaded && !_isSettingNewLayer)
            Layer.Handler._ExclusionMask = e.NewValue;
    }

    private void sldr_Opacity_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded || _isSettingNewLayer) return;

        Layer.Handler._Opacity = (float)e.NewValue / 100.0f;
        lbl_Opacity_Text.Text = $"{(int)e.NewValue} %";
    }

    private void btnOverrides_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _isSettingNewLayer) return;

        var v = overridesEditor.IsVisible;
        overridesEditor.Visibility = v ? Visibility.Hidden : Visibility.Visible;
        grd_LayerControl.IsHitTestVisible = v;
        btnConfig.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
    }
}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using AuroraRgb.EffectsEngine;
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

        CmbLayerType.ItemsSource = layer.AssociatedApplication.AllowedLayers.OrderBy(l => l.Order).ThenBy(l => l.Name);
        CmbLayerType.SelectedValue = Layer.Handler.GetType();

        CtrlLayerTypeConfig.Content = EmptyContent;
        CtrlLayerTypeConfig.Content = await layer.Control;
        ChkExcludeMask.IsChecked = Layer.Handler._EnableExclusionMask ?? false;
        KeyseqExcludeMask.Sequence = Layer.Handler._ExclusionMask;
        SldrOpacity.Value = (Layer.Handler._Opacity ?? 1d) * 100.0;
        LblOpacityText.Text = $"{(int)SldrOpacity.Value} %";

        GrdLayerConfigs.Visibility = Visibility.Hidden;
        OverridesEditor.Visibility = Visibility.Hidden;
        BtnConfig.Visibility = Visibility.Visible;
        BtnOverrides.Visibility = Visibility.Visible;
        GrdLayerControl.IsHitTestVisible = true;
        GrdLayerControl.Effect = null;
        _isSettingNewLayer = false;

        OverridesEditor.Layer = layer;

        var effectLayerType = layer.Handler.GetEffectLayerType();
        if (effectLayerType == typeof(NoRenderLayer))
        {
            PowerTooltip.Visibility = Visibility.Visible;
            PowerTooltip.Text = "\ud83c\udf43";
            PowerTooltip.CircleBackground = Brushes.Green;
            PowerTooltip.HintTooltip = "Non-rendering layer. Minimum background usage";
        }else if (effectLayerType == typeof(BitmapEffectLayer))
        {
            PowerTooltip.Visibility = Visibility.Visible;
            PowerTooltip.Text = "\ud83c\udf42";
            PowerTooltip.CircleBackground = Brushes.Chocolate;
            PowerTooltip.HintTooltip = "Rendering layer. Background usage may be high depending how often the visual changes";
        }
        else
        {
            PowerTooltip.Visibility = Visibility.Hidden;
        }
        HighUsageTooltip.Visibility = layer.Handler.HighResource() ? Visibility.Visible : Visibility.Hidden;
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

        CtrlLayerTypeConfig.Content = EmptyContent;
        CtrlLayerTypeConfig.Content = await _Layer.Control;
        ChkExcludeMask.IsChecked = Layer.Handler._EnableExclusionMask ?? false;
        KeyseqExcludeMask.Sequence = Layer.Handler._ExclusionMask;
        SldrOpacity.Value = (int)(Layer.Handler.Opacity * 100.0f);
        LblOpacityText.Text = $"{(int)SldrOpacity.Value} %";
        _Layer.AssociatedApplication.SaveProfiles();

        OverridesEditor.ForcePropertyListUpdate();
    }

    private void btnReset_Click(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && !_isSettingNewLayer && sender is Button)
        {
            ResetLayer((Type)CmbLayerType.SelectedValue);
        }
    }

    private void btnConfig_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _isSettingNewLayer || sender is not Button) return;

        var v = GrdLayerConfigs.IsVisible;
        GrdLayerConfigs.Visibility = v ? Visibility.Hidden : Visibility.Visible;
        GrdLayerControl.IsHitTestVisible = v;
        GrdLayerControl.Effect = v ? null : new BlurEffect();
        BtnOverrides.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
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
        LblOpacityText.Text = $"{(int)e.NewValue} %";
    }

    private void btnOverrides_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _isSettingNewLayer) return;

        var v = OverridesEditor.IsVisible;
        OverridesEditor.Visibility = v ? Visibility.Hidden : Visibility.Visible;
        GrdLayerControl.IsHitTestVisible = v;
        BtnConfig.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
    }
}
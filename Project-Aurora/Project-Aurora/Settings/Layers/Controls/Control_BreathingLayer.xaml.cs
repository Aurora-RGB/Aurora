using System.Diagnostics;
using System.Windows;

namespace AuroraRgb.Settings.Layers.Controls;

public partial class Control_BreathingLayer
{
    public Control_BreathingLayer() {
        InitializeComponent();
    }

    public Control_BreathingLayer(BreathingLayerHandler context) {
        InitializeComponent();
        DataContext = context;
    }

    private void CurveGraphs_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer", "https://www.desmos.com/calculator/yxhba9jczx");
    }
}
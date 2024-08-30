using System.Windows;

namespace AuroraRgb.Profiles.Payday_2.Layers;

/// <summary>
/// Interaction logic for Control_PD2StatesLayer.xaml
/// </summary>
public partial class Control_PD2StatesLayer
{
    public Control_PD2StatesLayer()
    {
        InitializeComponent();
    }

    public Control_PD2StatesLayer(PD2StatesLayerHandler datacontext)
    {
        DataContext = datacontext.Properties;
        InitializeComponent();
    }

    private void sldSwanSongSpeed_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        lblSwanSongSpeed.Content = $"x {sldSwanSongSpeed.Value:0.00}";
    }
}
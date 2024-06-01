using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Profiles.LeagueOfLegends.GSI.Nodes;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.LeagueOfLegends.Layers;

/// <summary>
/// Interaction logic for UserControl1.xaml
/// </summary>
public partial class LoLBackgroundLayer
{
    private LoLBackgroundLayerHandler Context => (LoLBackgroundLayerHandler)DataContext;

    private Champion _selectedChampion;

    public LoLBackgroundLayer()
    {
        InitializeComponent();
    }

    public LoLBackgroundLayer(LoLBackgroundLayerHandler context)
    {
        InitializeComponent();

        DataContext = context;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void SetSettings()
    {
        championPicker.SelectedItem = Champion.None;
    }

    private void championPicker_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox)
            return;

        _selectedChampion = (Champion)comboBox.SelectedItem;

        colorPicker.SelectedColor = Context.Properties.ChampionColors[_selectedChampion].ToMediaColor();
    }

    private void colorPicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (sender is not ColorPicker picker)
            return;

        Context.Properties.ChampionColors[_selectedChampion] = (picker.SelectedColor ?? Color.FromArgb(0,0,0,0)).ToDrawingColor();
    }
}
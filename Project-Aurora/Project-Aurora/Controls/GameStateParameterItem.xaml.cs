using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace AuroraRgb.Controls;

public partial class GameStateParameterItem
{
    private DispatcherTimer _timer;

    public GameStateParameterItem()
    {
        InitializeComponent();
    }

    public PropertyEntryToValueConverter Converter { get; set; }

    private void GameStateParameterItem_OnLoaded(object sender, RoutedEventArgs e)
    {
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, TimerTick, Dispatcher);
        _timer.Start();
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        ValueText.Text = Converter.Convert(DataContext, typeof(string), null, CultureInfo.InvariantCulture).ToString();
    }

    private void GameStateParameterItem_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
    }
}
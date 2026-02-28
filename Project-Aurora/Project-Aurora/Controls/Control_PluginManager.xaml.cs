using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Settings;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_DeviceManager.xaml
/// </summary>
public partial class Control_PluginManager
{
    private readonly Task<PluginManager> _host;

    public Control_PluginManager(Task<PluginManager> host)
    {
        _host = host;

        InitializeComponent();

        DataContext = host;
    }

    private async void chkEnabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender == null)
            return;
        var chk = (CheckBox)sender;
        var plugin = (KeyValuePair<string, IPlugin>)chk.DataContext;
        (await _host).SetPluginEnabled(plugin.Key, (bool)chk.IsChecked);
    }
}
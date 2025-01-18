using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using Microsoft.Scripting.Utils;

namespace AuroraRgb.Controls;

public partial class Control_AdvancedSettings
{
    public ObservableCollection<CheckedListItem> AvailableInterfaces { get; } = [];
    
    public Control_AdvancedSettings()
    {
        DataContext = this;
        
        InitializeComponent();
    }

    private void Control_AdvancedSettings_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(IsInterNetwork)
            .Select(i => new CheckedListItem(i.Name, Global.Configuration.HttpListenInterfaceNames.Contains(i.Name)));
        AvailableInterfaces.AddRange(interfaces);
    }

    private static bool IsInterNetwork(NetworkInterface networkInterface)
    {
        if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
        {
            return false;
        }
        return networkInterface.GetIPProperties()
            .UnicastAddresses
            .Any(ip => ip.Address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6);
    }

    public class CheckedListItem(string name, bool isChecked)
    {
        private bool _isChecked = isChecked;
        public string Name { get; } = name;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                Global.Configuration.HttpListenInterfaceNames = value
                    ? Global.Configuration.HttpListenInterfaceNames.Append(Name).ToList()
                    : Global.Configuration.HttpListenInterfaceNames.Except([Name]).ToList();
                _isChecked = value;
            }
        }
    }
}
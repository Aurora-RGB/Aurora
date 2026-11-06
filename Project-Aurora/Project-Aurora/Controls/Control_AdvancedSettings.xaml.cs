using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using AuroraRgb.Modules;
using AuroraRgb.Utils;
using Microsoft.Scripting.Utils;

namespace AuroraRgb.Controls;

public partial class Control_AdvancedSettings
{
    public ObservableCollection<CheckedListItem> AvailableInterfaces { get; } = [];
    public ObservableCollection<ProcessorListItem> AvailableProcessors { get; } = [];
    
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

        PerformanceModeModule.AffinityChanged -= AffinityOnChanged;
        PerformanceModeModule.AffinityChanged += AffinityOnChanged;
        AffinityStatusText.Text = PerformanceModeModule.AffinityStatus;
        AvailableProcessors.Clear();
        try
        {
            foreach (var cpu in CpuSetAffinity.GetCpuSets())
                AvailableProcessors.Add(new ProcessorListItem(cpu,
                    Global.Configuration.ManualCpuAffinity?.Contains(cpu.Key) == true));
            ManualSelectionMessage.Text = "";
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or
                                           EntryPointNotFoundException or DllNotFoundException or
                                           System.IO.InvalidDataException or OverflowException)
        {
            ManualSelectionMessage.Text = "Windows CPU topology is unavailable. Manual selection cannot be applied.";
            Global.logger.Warning(exception, "Unable to enumerate processors for advanced settings");
        }

        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(IsInterNetwork)
            .Select(i => new CheckedListItem(i.Name, Global.Configuration.HttpListenInterfaceNames.Contains(i.Name)));
        AvailableInterfaces.Clear();
        AvailableInterfaces.AddRange(interfaces);
    }

    private void Control_AdvancedSettings_OnUnloaded(object sender, RoutedEventArgs e)
    {
        PerformanceModeModule.AffinityChanged -= AffinityOnChanged;
    }

    private void AffinityOnChanged(object? sender, EventArgs e)
    {
        Dispatcher.InvokeAsync(() => AffinityStatusText.Text = PerformanceModeModule.AffinityStatus);
    }

    private void ApplyCpuSelection_OnClick(object sender, RoutedEventArgs e)
    {
        var selected = AvailableProcessors.Where(c => c.Available && c.IsChecked).Select(c => c.Key).ToList();
        if (selected.Count == 0)
        {
            ManualSelectionMessage.Text = "Select at least one available processor. Your previous selection is unchanged.";
            return;
        }
        Global.Configuration.ManualCpuAffinity = selected;
        ManualSelectionMessage.Text = "Selection saved.";
    }

    public sealed class ProcessorListItem(CpuSet cpu, bool isChecked)
    {
        public string Key => cpu.Key;
        public string Name => cpu.Available ? cpu.Name : $"{cpu.Name} (reserved by Windows)";
        public bool Available => cpu.Available;
        public bool IsChecked { get; set; } = isChecked;
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

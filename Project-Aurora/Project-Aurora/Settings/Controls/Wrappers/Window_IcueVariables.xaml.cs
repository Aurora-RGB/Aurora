using System;
using System.Collections.ObjectModel;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Icue;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Controls.Wrappers;

public partial class Window_IcueVariables : IDisposable
{
    private readonly TransparencyComponent _transparencyComponent;
    public ObservableCollection<IcueGsiStateStore> StateStore { get; }

    public Window_IcueVariables()
    {
        var gsi = IcueModule.AuroraIcueServer.Gsi;
        StateStore = new ObservableCollection<IcueGsiStateStore>(gsi.StateStore.Values);
        gsi.GameChanged += GsiOnGameChanged;

        InitializeComponent();

        // Set the DataContext to the window so XAML can bind to the StateStore property
        DataContext = this;
        UpdateEmptyTextVisibility();

        _transparencyComponent = new TransparencyComponent(this, null);
    }

    private void GsiOnGameChanged(object? sender, EventArgs e)
    {
        var gsi = IcueModule.AuroraIcueServer.Gsi;
        Dispatcher.Invoke(() =>
        {
            StateStore.Clear();
            foreach (var store in gsi.StateStore.Values)
                StateStore.Add(store);
            UpdateEmptyTextVisibility();
        });
    }
    
    private void UpdateEmptyTextVisibility()
    {
        EmptyText.Visibility = StateStore.Count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public void Dispose()
    {
        _transparencyComponent.Dispose();
    }
}
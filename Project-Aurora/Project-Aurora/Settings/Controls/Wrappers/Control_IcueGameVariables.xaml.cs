using System;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Modules.Icue;

namespace AuroraRgb.Settings.Controls.Wrappers;

public partial class Control_IcueGameVariables
{
    private IcueGsiStateStore? _gameStorage;
    
    public IcueGsiStateStore? GameStorage
    {
        get => (IcueGsiStateStore?)GetValue(GameStorageProperty);
        set => SetValue(GameStorageProperty, value);
    }

    public static readonly DependencyProperty GameStorageProperty =
        DependencyProperty.Register(
            nameof(GameStorage),
            typeof(IcueGsiStateStore),
            typeof(Control_IcueGameVariables),
            new PropertyMetadata(null, OnGameStorageChanged));


    public Control_IcueGameVariables()
    {
        InitializeComponent();
    }

    private static void OnGameStorageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (Control_IcueGameVariables)d;

        if (e.OldValue is IcueGsiStateStore oldStore)
            oldStore.StateChanged -= control.GameStorageOnStateChanged;

        control._gameStorage = e.NewValue as IcueGsiStateStore;
        control.DataContext = control._gameStorage;

        if (control._gameStorage is not null)
        {
            control._gameStorage.StateChanged += control.GameStorageOnStateChanged;
            control.RefreshLists();
        }
        else
        {
            control.ClearLists();
        }
    }

    private void Control_IcueGameVariables_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_gameStorage is not null)
            _gameStorage.StateChanged -= GameStorageOnStateChanged;
    }

    private void GameStorageOnStateChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(RefreshLists);
    }

    private void RefreshLists()
    {
        if (_gameStorage is null) { ClearLists(); return; }

        StatesList.Children.Clear();
        foreach (var state in _gameStorage.States)
            StatesList.Children.Add(new TextBlock { Text = state });

        EventsList.Children.Clear();
        foreach (var ev in _gameStorage.Events)
            EventsList.Children.Add(new TextBlock { Text = ev });
    }

    private void ClearLists()
    {
        StatesList.Children.Clear();
        EventsList.Children.Clear();
    }
}
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Settings.Controls;

namespace AuroraRgb.Profiles.Generic_Application;

public partial class Control_GenericApplication
{
    private readonly Application _app;

    public Control_GenericApplication(Application profile)
    {
        _app = profile;
        SetSettings();

        InitializeComponent();
    }

    private void Profile_manager_ProfileChanged(object? sender, EventArgs e)
    {
        SetSettings();
    }

    private void SetSettings()
    {
        DataContext = _app;
    }

    private void Control_GenericApplication_OnLoaded(object sender, RoutedEventArgs e)
    {
        ProcessListBox.ItemsSource = _app.Config.ProcessNames;
        
        _app.ProfileChanged -= Profile_manager_ProfileChanged;
        _app.ProfileChanged += Profile_manager_ProfileChanged;

        UpdateRemoveButton();
    }

    private void Control_GenericApplication_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _app.ProfileChanged -= Profile_manager_ProfileChanged;
    }

    private async void RemoveSelectedButton_OnClick(object sender, RoutedEventArgs e)
    {
        var selectedItem = ProcessListBox.SelectedItems[0]!;
        _app.Config.ProcessNames = _app.Config.ProcessNames.Where(p => !p.Equals(selectedItem)).ToArray();
        ((GenericApplicationSettings)_app.Settings).AdditionalProcesses = _app.Config.ProcessNames;
        ProcessListBox.ItemsSource = _app.Config.ProcessNames;

        AddProcessButton.IsEnabled = false;
        RemoveSelectedButton.IsEnabled = false;
        await _app.SaveSettings();

        UpdateRemoveButton();
    }

    private async void AddProcessButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Window_ProcessSelection { CheckCustomPathExists = true, ButtonLabel = "Add Profile", Title = "Add Profile" };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ChosenExecutablePath))
            return; // do not need to check if dialog is already in excluded_programs since it is a Set and only contains unique items by definition

        var filename = Path.GetFileName(dialog.ChosenExecutablePath.ToLowerInvariant());
        var eventName = Path.GetFileNameWithoutExtension(filename);

        var lightingStateManager = Global.LightingStateManager!;
        if (lightingStateManager.ApplicationManager.Events.TryGetValue(eventName, out _))
        {
            MessageBox.Show("Profile for this application already exists.");
            return;
        }

        _app.Config.ProcessNames = _app.Config.ProcessNames.Concat([filename]).ToArray();
        ((GenericApplicationSettings)_app.Settings).AdditionalProcesses = _app.Config.ProcessNames;

        ProcessListBox.ItemsSource = _app.Config.ProcessNames;

        AddProcessButton.IsEnabled = false;
        RemoveSelectedButton.IsEnabled = false;
        await _app.SaveSettings();

        UpdateRemoveButton();
    }

    private void UpdateRemoveButton()
    {
        RemoveSelectedButton.IsEnabled = SelectedIsAdditional();
        AddProcessButton.IsEnabled = true;

        bool SelectedIsAdditional()
        {
            return ProcessListBox.SelectedIndex > 0;
        }
    }

    private void ProcessListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateRemoveButton();
    }
}
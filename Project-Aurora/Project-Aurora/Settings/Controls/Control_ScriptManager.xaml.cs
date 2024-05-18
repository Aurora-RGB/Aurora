using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Application = AuroraRgb.Profiles.Application;

namespace AuroraRgb.Settings.Controls;

/// <summary>
/// Interaction logic for Control_ProfileManager.xaml
/// </summary>
public partial class Control_ScriptManager
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty ProfileManagerProperty = DependencyProperty.Register(nameof(ProfileManager), typeof(Application), typeof(Control_ScriptManager));

    public Application ProfileManager
    {
        get => (Application)GetValue(ProfileManagerProperty);
        set
        {
            SetValue(ProfileManagerProperty, value);

            value.ProfileChanged += (sender, e) => {
                Scripts = (sender as Application)?.Profile.ScriptSettings;
            };
            Scripts = value.Profile.ScriptSettings;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty ScriptsProperty = DependencyProperty.Register(nameof(Scripts), typeof(Dictionary<string, ScriptSettings>), typeof(Control_ScriptManager));

    public Dictionary<string, ScriptSettings> Scripts
    {
        get => (Dictionary<string, ScriptSettings>)GetValue(ScriptsProperty);
        set => SetValue(ScriptsProperty, value);
    }

    public Control_ScriptManager()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void CheckBox_Checked(object? sender, RoutedEventArgs e)
    {
        ProfileManager.SaveProfiles();
    }
}
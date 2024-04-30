using AuroraRgb.Settings;

namespace AuroraRgb.Profiles.EliteDangerous;

/// <summary>
/// Interaction logic for Control_EliteDangerous.xaml
/// </summary>
public partial class Control_EliteDangerous
{
    private Application profile_manager;

    public Control_EliteDangerous(Application profile)
    {
        InitializeComponent();

        profile_manager = profile;

        if (!(profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled)
        {
            (profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
        }
    }
}
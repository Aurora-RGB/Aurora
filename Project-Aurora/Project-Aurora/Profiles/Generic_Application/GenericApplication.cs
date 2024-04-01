using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Profiles.Generic_Application;

public class GenericApplication : Application
{
    public override ImageSource Icon
    {
        get
        {
            if (icon != null) return icon;
            var iconPath = Path.Combine(GetProfileFolderPath(), "icon.png");

            if (File.Exists(iconPath))
            {
                var memStream = new MemoryStream(File.ReadAllBytes(iconPath));
                var b = new BitmapImage();
                b.BeginInit();
                b.StreamSource = memStream;
                b.EndInit();

                icon = b;
            }
            else
                icon = new BitmapImage(new Uri(@"Resources/unknown_app_icon.png", UriKind.Relative));

            return icon;
        }
    }

    public GenericApplication(string processName) : base(new LightEventConfig(new Lazy<LightEvent>(() => new Event_GenericApplication())) {
        Name = "Generic Application",
        ID = processName,
        ProcessNames = [processName],
        SettingsType = typeof(GenericApplicationSettings),
        ProfileType = typeof(GenericApplicationProfile),
        OverviewControlType = typeof(Control_GenericApplication),
        GameStateType = typeof(GameState_Wrapper),
    })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }

    protected override async Task LoadSettings(Type settingsType)
    {
        await base.LoadSettings(settingsType);

        if (Settings is GenericApplicationSettings genericApplicationSettings)
        {
            var additionalProcessNames = genericApplicationSettings.AdditionalProcesses;
            if (additionalProcessNames.Length != 0)
            {
                if (additionalProcessNames[0] != Config.ProcessNames[0])
                {
                    Config.ProcessNames = Config.ProcessNames.Concat(additionalProcessNames).ToArray();
                }
                else
                {
                    Config.ProcessNames = additionalProcessNames;
                }
            }
        }
    }

    public override string GetProfileFolderPath()
    {
        return Path.Combine(Global.AppDataDirectory, "AdditionalProfiles", Config.ID);
    }

    protected override ApplicationProfile CreateNewProfile(string profileName)
    {
        ApplicationProfile profile = (ApplicationProfile)Activator.CreateInstance(Config.ProfileType);
        profile.ProfileName = profileName;
        profile.ProfileFilepath = Path.Combine(GetProfileFolderPath(), GetUnusedFilename(GetProfileFolderPath(), profile.ProfileName) + ".json");
        return profile;
    }
}
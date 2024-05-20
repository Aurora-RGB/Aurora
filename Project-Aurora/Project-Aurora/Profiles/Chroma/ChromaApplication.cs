using System;
using System.Linq;
using System.Threading.Tasks;
using AuroraRgb.Modules;

namespace AuroraRgb.Profiles.Chroma;

public sealed class ChromaApplication : Application
{
    public ChromaApplication() : base(new LightEventConfig
    {
        Name = "Chroma Apps",
        ID = "chroma",
        ProcessNames = [],
        ProfileType = typeof(RazerChromaProfile),
        OverviewControlType = typeof(Control_Chroma),
        GameStateType = typeof(GameState_Wrapper),
        IconURI = "Resources/chroma.png",
        EnableByDefault = true,
    })
    {
        RazerSdkModule.RzSdkManager.Result.ChromaRegistrySettings.ChromaAppsChanged += ChromaRegistrySettingsOnChromaAppsChanged;
    }

    private async void ChromaRegistrySettingsOnChromaAppsChanged(object? sender, EventArgs e)
    {
        await SetProfileApplication();
    }

    private async Task SetProfileApplication()
    {
        var chromaRegistrySettings = (await RazerSdkModule.RzSdkManager).ChromaRegistrySettings;
        Config.ProcessNames = chromaRegistrySettings.AllChromaApps
            .Where(processName => !string.IsNullOrWhiteSpace(processName))
            .Where(s => !chromaRegistrySettings.ExcludedPrograms.Contains(s))
            .ToArray();
    }

    public override void Dispose()
    {
        base.Dispose();
        
        RazerSdkModule.RzSdkManager.Result.ChromaRegistrySettings.ChromaAppsChanged -= ChromaRegistrySettingsOnChromaAppsChanged;
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Razer;

namespace AuroraRgb.Profiles.Chroma;

public sealed class ChromaApplication() : Application(new LightEventConfig
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
    public override async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        var baseInit = await base.Initialize(cancellationToken);

        var chromaRegistrySettings = await GetChromaRegistrySettings();
        chromaRegistrySettings.ChromaAppsChanged += ChromaRegistrySettingsOnChromaAppsChanged;
        Config.ProcessNames = chromaRegistrySettings.AllChromaApps
            .Where(processName => !string.IsNullOrWhiteSpace(processName))
            .Where(s => !chromaRegistrySettings.ExcludedPrograms.Contains(s))
            .ToArray();
        return baseInit;
    }

    private static async Task<ChromaRegistrySettings> GetChromaRegistrySettings()
    {
        return (await RazerSdkModule.RzSdkManager).ChromaRegistrySettings;
    }

    private async void ChromaRegistrySettingsOnChromaAppsChanged(object? sender, EventArgs e)
    {
        await SetProfileApplication();
    }

    private async Task SetProfileApplication()
    {
        var chromaRegistrySettings = await GetChromaRegistrySettings();
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
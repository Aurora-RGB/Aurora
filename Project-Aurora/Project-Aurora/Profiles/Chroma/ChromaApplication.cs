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
    IconURI = "Resources/chroma.png",
    EnableByDefault = true,
    Priority = 6,
})
{
    private ChromaEventReader? _reader;

    public override async Task<bool> Initialize(CancellationToken cancellationToken)
    {
        var baseInit = await base.Initialize(cancellationToken);

        // #292 standalone: activate when a SetEventName game is actually firing events — no Synapse/Chroma,
        // no Razer registry needed. The firing game's process (resolved from the event record's PID) is added
        // to the process list so Aurora applies this profile while that game is in the foreground.
        _reader = await ChromaEventModule.Reader;
        _reader.EventReceived += OnChromaEvent;

        // Best-effort extra: also include registry-listed Chroma apps IF the Razer SDK happens to be present.
        _ = TryMergeRegistryApps();

        return baseInit;
    }

    private void OnChromaEvent(object? sender, ChromaGameEvent e)
    {
        if (sender is not ChromaEventReader reader)
            return;

        var process = reader.CurrentProcess;
        if (string.IsNullOrEmpty(process) ||
            Config.ProcessNames.Contains(process, StringComparer.OrdinalIgnoreCase))
            return;

        Config.ProcessNames = Config.ProcessNames.Append(process).ToArray();
    }

    private async Task TryMergeRegistryApps()
    {
        try
        {
            var settings = (await RazerSdkModule.RzSdkManager).ChromaRegistrySettings;
            settings.ChromaAppsChanged += ChromaRegistrySettingsOnChromaAppsChanged;
            MergeRegistryApps(settings);
        }
        catch
        {
            // No Razer SDK present (Synapse/Chroma uninstalled) -> rely purely on event-driven activation.
        }
    }

    private async void ChromaRegistrySettingsOnChromaAppsChanged(object? sender, EventArgs e)
    {
        try
        {
            MergeRegistryApps((await RazerSdkModule.RzSdkManager).ChromaRegistrySettings);
        }
        catch
        {
            // ignore
        }
    }

    private void MergeRegistryApps(ChromaRegistrySettings settings)
    {
        var apps = settings.AllChromaApps
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Where(p => !settings.ExcludedPrograms.Contains(p));

        Config.ProcessNames = Config.ProcessNames
            .Concat(apps)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_reader is not null)
            _reader.EventReceived -= OnChromaEvent;

        try
        {
            RazerSdkModule.RzSdkManager.Result.ChromaRegistrySettings.ChromaAppsChanged -= ChromaRegistrySettingsOnChromaAppsChanged;
        }
        catch
        {
            // the Razer SDK may never have loaded (Synapse/Chroma absent)
        }
    }
}
